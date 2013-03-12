//-----------------------------------------------------------------------
// <copyright file="AsyncSocketTcpServer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.AsyncSocket
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// Async Socket Tcp Server Class.
    /// </summary>
    public class AsyncSocketTcpServer : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// The socket used to listen for incoming connection requests.
        /// </summary>
        private Socket _listenSocket;

        /// <summary>
        /// The total number of clients connected to the server.
        /// </summary>
        private long _connectedSocketsCount;

        /// <summary>
        /// Thread-safe dictionary of connected socket client.
        /// </summary>
        private Dictionary<int, SocketAsyncEventArgs> _sessionDictionary;

        /// <summary>
        /// Field _acceptSocketAsyncEventArgs.
        /// </summary>
        private SocketAsyncEventArgs _acceptSocketAsyncEventArgs;

        /// <summary>
        /// Field _readerWriterLock.
        /// </summary>
        private ReaderWriterLock _readerWriterLock = new ReaderWriterLock();

        /// <summary>
        /// Counter of the total bytes received by AsyncSocketTcpServer.
        /// </summary>
        private long _totalBytesReceived;

        /// <summary>
        /// Counter of the total bytes sent by AsyncSocketTcpServer.
        /// </summary>
        private long _totalBytesSent;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSocketTcpServer" /> class.
        /// </summary>
        public AsyncSocketTcpServer()
            : this(IPEndPoint.MinPort, 8192, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSocketTcpServer" /> class.
        /// </summary>
        /// <param name="port">Local port to listen.</param>
        /// <param name="bufferSize">Buffer size to use with receive data.</param>
        /// <param name="useIPv6">if set to <c>true</c> use IPv6; otherwise, use IPv4.</param>
        public AsyncSocketTcpServer(int port, int bufferSize = 8192, bool useIPv6 = false)
        {
            this.LocalPort = port;
            this.BufferSize = bufferSize;
            this.UseIPv6 = useIPv6;

            this._sessionDictionary = new Dictionary<int, SocketAsyncEventArgs>();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AsyncSocketTcpServer" /> class.
        /// </summary>
        ~AsyncSocketTcpServer()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Client Connected Event.
        /// </summary>
        public event EventHandler<AsyncSocketSessionEventArgs> Connected;

        /// <summary>
        /// Client Disconnected Event.
        /// </summary>
        public event EventHandler<AsyncSocketSessionEventArgs> Disconnected;

        /// <summary>
        /// Server Data Received Event.
        /// </summary>
        public event EventHandler<AsyncSocketSessionEventArgs> DataReceived;

        /// <summary>
        /// Server Data Sent Event.
        /// </summary>
        public event EventHandler<AsyncSocketSessionEventArgs> DataSent;

        /// <summary>
        /// Error Occurred Event.
        /// </summary>
        public event EventHandler<AsyncSocketErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// Server Started Event.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Server Stopped Event.
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Gets the maximum amount of connected sockets.
        /// </summary>
        public long PeakConnectedSocketsCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets buffer size to use with receive data.
        /// </summary>
        public int BufferSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets local port to listen.
        /// </summary>
        public int LocalPort
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use IPv6 or IPv4.
        /// </summary>
        public bool UseIPv6
        {
            get;
            set;
        }

        /// <summary>
        /// Gets total bytes received by AsyncSocketTcpServer.
        /// </summary>
        public long TotalBytesReceived
        {
            get
            {
                return Interlocked.Read(ref this._totalBytesReceived);
            }
        }

        /// <summary>
        /// Gets total bytes sent by AsyncSocketTcpServer.
        /// </summary>
        public long TotalBytesSent
        {
            get
            {
                return Interlocked.Read(ref this._totalBytesSent);
            }
        }

        /// <summary>
        /// Gets current numbers of connected sockets.
        /// </summary>
        public long ConnectedSocketsCount
        {
            get
            {
                this.CheckDisposed();

                return Interlocked.Read(ref this._connectedSocketsCount);
            }
        }

        /// <summary>
        /// Gets a value indicating whether AsyncSocketTcpServer is working or not.
        /// </summary>
        public bool IsListening
        {
            get
            {
                this.CheckDisposed();

                try
                {
                    return this._listenSocket == null ? false : this._listenSocket.IsBound;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Start AsyncSocketTcpServer.
        /// </summary>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Start(bool throwOnError = false)
        {
            this.CheckDisposed();

            if (!this.IsListening)
            {
                try
                {
                    IPEndPoint localIPEndPoint = new IPEndPoint(this.UseIPv6 ? IPAddress.IPv6Any : IPAddress.Any, this.LocalPort);

                    this.CloseAcceptSocketAsyncEventArgs();
                    this.CloseListenSocket();
                    this.ClearSessionDictionary();

                    this._listenSocket = new Socket(localIPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    ////this._listenSocket.NoDelay = true;
                    this._listenSocket.Bind(localIPEndPoint);
                    this._listenSocket.Listen(AsyncSocketTcpServerConstants.Backlog);

                    this.StartAccept();

                    Debug.WriteLine(AsyncSocketTcpServerConstants.TcpServerStartSucceeded);

                    this.RaiseEvent(this.Started);

                    return true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(AsyncSocketTcpServerConstants.TcpServerStartException);

                    ExceptionHandler.Log(e);

                    this.RaiseEvent(
                                    this.ErrorOccurred,
                                    new AsyncSocketErrorEventArgs(
                                                                  AsyncSocketTcpServerConstants.TcpServerStartException,
                                                                  e,
                                                                  AsyncSocketErrorCodeEnum.TcpServerStartException));

                    if (throwOnError)
                    {
                        throw;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Stop AsyncSocketTcpServer.
        /// </summary>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Stop(bool throwOnError = false)
        {
            this.CheckDisposed();

            if (this.IsListening)
            {
                try
                {
                    this.CloseAcceptSocketAsyncEventArgs();
                    this.CloseListenSocket();
                    this.ClearSessionDictionary();
                    this._readerWriterLock.ReleaseLock();
                    this._connectedSocketsCount = 0;

                    Debug.WriteLine(AsyncSocketTcpServerConstants.TcpServerStopSucceeded);

                    this.RaiseEvent(this.Stopped);

                    return true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(AsyncSocketTcpServerConstants.TcpServerStopException);

                    ExceptionHandler.Log(e);

                    this.RaiseEvent(
                                    this.ErrorOccurred,
                                    new AsyncSocketErrorEventArgs(
                                                                  AsyncSocketTcpServerConstants.TcpServerStopException,
                                                                  e,
                                                                  AsyncSocketErrorCodeEnum.TcpServerStopException));

                    if (throwOnError)
                    {
                        throw;
                    }
                }
                finally
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
            }

            return false;
        }

        /// <summary>
        /// Get connected socket RemoteIPEndPoint.
        /// </summary>
        /// <param name="sessionId">Connected socket Id.</param>
        /// <returns>Connected socket RemoteIPEndPoint.</returns>
        public IPEndPoint GetRemoteIPEndPoint(int sessionId)
        {
            this.CheckDisposed();

            SocketAsyncEventArgs sessionSocketAsyncEventArgs = null;

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                this._sessionDictionary.TryGetValue(sessionId, out sessionSocketAsyncEventArgs);
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
            }
            finally
            {
                this._readerWriterLock.ReleaseReaderLock();
            }

            if (sessionSocketAsyncEventArgs != null)
            {
                Socket sessionSocket = sessionSocketAsyncEventArgs.UserToken as Socket;

                if (sessionSocket != null)
                {
                    return sessionSocket.RemoteEndPoint as IPEndPoint;
                }
            }

            return null;
        }

        /// <summary>
        /// Get current connected sockets collection.
        /// </summary>
        /// <returns>Instance of Dictionary{int, IPEndPoint}.</returns>
        public Dictionary<int, IPEndPoint> GetSessionCollection()
        {
            this.CheckDisposed();

            Dictionary<int, IPEndPoint> result = new Dictionary<int, IPEndPoint>();

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                foreach (KeyValuePair<int, SocketAsyncEventArgs> item in this._sessionDictionary)
                {
                    try
                    {
                        if (item.Value != null)
                        {
                            Socket sessionSocket = item.Value.UserToken as Socket;

                            if (sessionSocket != null)
                            {
                                result.Add(item.Key, sessionSocket.RemoteEndPoint as IPEndPoint);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
            }
            finally
            {
                this._readerWriterLock.ReleaseReaderLock();
            }

            return result;
        }

        /// <summary>
        /// Close connected socket.
        /// </summary>
        /// <param name="sessionId">Connected socket Id.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool CloseSession(int sessionId)
        {
            this.CheckDisposed();

            if (!this.IsListening)
            {
                return false;
            }

            SocketAsyncEventArgs sessionSocketAsyncEventArgs = null;

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                this._sessionDictionary.TryGetValue(sessionId, out sessionSocketAsyncEventArgs);
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
            }
            finally
            {
                this._readerWriterLock.ReleaseReaderLock();
            }

            if (sessionSocketAsyncEventArgs != null)
            {
                IPEndPoint sessionIPEndPoint = null;

                try
                {
                    sessionIPEndPoint = (sessionSocketAsyncEventArgs.UserToken as Socket).RemoteEndPoint as IPEndPoint;
                }
                catch
                {
                }

                this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

                try
                {
                    if (this._sessionDictionary.ContainsKey(sessionId))
                    {
                        this._sessionDictionary.Remove(sessionId);
                        Interlocked.Decrement(ref this._connectedSocketsCount);
                    }

                    this.CloseSessionSocketAsyncEventArgs(sessionSocketAsyncEventArgs);

                    this.RaiseEvent(this.Disconnected, new AsyncSocketSessionEventArgs(sessionId, sessionIPEndPoint));

                    return true;
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                }
                finally
                {
                    this._readerWriterLock.ReleaseWriterLock();
                }
            }

            return false;
        }

        /// <summary>
        /// Sends data asynchronously to a connected socket.
        /// </summary>
        /// <param name="sessionId">Connected socket session Id.</param>
        /// <param name="buffer">Data to send.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public bool Send(int sessionId, byte[] buffer)
        {
            this.CheckDisposed();

            if (!this.IsListening)
            {
                return false;
            }

            SocketAsyncEventArgs sessionSocketAsyncEventArgs = null;

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                this._sessionDictionary.TryGetValue(sessionId, out sessionSocketAsyncEventArgs);
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
            }
            finally
            {
                this._readerWriterLock.ReleaseReaderLock();
            }

            if (sessionSocketAsyncEventArgs != null)
            {
                Socket sessionSocket = sessionSocketAsyncEventArgs.UserToken as Socket;

                if (sessionSocket != null)
                {
                    SocketAsyncEventArgs sendSocketAsyncEventArgs = null;

                    try
                    {
                        sendSocketAsyncEventArgs = new SocketAsyncEventArgs();
                        sendSocketAsyncEventArgs.UserToken = sessionSocket;
                        sendSocketAsyncEventArgs.SetBuffer(buffer, 0, buffer.Length);
                        sendSocketAsyncEventArgs.Completed += this.SendSocketAsyncEventArgsCompleted;

                        try
                        {
                            bool willRaiseEvent = sessionSocket.SendAsync(sendSocketAsyncEventArgs);
                            if (!willRaiseEvent)
                            {
                                this.ProcessSend(sendSocketAsyncEventArgs);
                            }

                            return true;
                        }
                        catch
                        {
                            throw;
                        }
                    }
                    catch (Exception e)
                    {
                        if (sendSocketAsyncEventArgs != null)
                        {
                            sendSocketAsyncEventArgs.Completed -= this.SendSocketAsyncEventArgsCompleted;
                            sendSocketAsyncEventArgs.UserToken = null;
                            sendSocketAsyncEventArgs.Dispose();
                            sendSocketAsyncEventArgs = null;
                        }

                        ExceptionHandler.Log(e);

                        this.RaiseEvent(
                                        this.ErrorOccurred,
                                        new AsyncSocketErrorEventArgs(
                                                                      AsyncSocketTcpServerConstants.TcpServerSendException,
                                                                      e,
                                                                      AsyncSocketErrorCodeEnum.TcpServerSendException));
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AsyncSocketTcpServer" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AsyncSocketTcpServer" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AsyncSocketTcpServer" /> class.
        /// protected virtual for non-sealed class; private for sealed class.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;

            if (disposing)
            {
                // dispose managed resources
                ////if (managedResource != null)
                ////{
                ////    managedResource.Dispose();
                ////    managedResource = null;
                ////}

                try
                {
                    this.CloseAcceptSocketAsyncEventArgs();
                    this.CloseListenSocket();
                    this.ClearSessionDictionary();
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                }
                finally
                {
                    this._acceptSocketAsyncEventArgs = null;
                    this._listenSocket = null;
                    this._sessionDictionary = null;
                    this._readerWriterLock.ReleaseLock();
                }
            }

            // free native resources
            ////if (nativeResource != IntPtr.Zero)
            ////{
            ////    Marshal.FreeHGlobal(nativeResource);
            ////    nativeResource = IntPtr.Zero;
            ////}
        }

        /// <summary>
        /// Method RaiseEvent.
        /// </summary>
        /// <param name="eventHandler">Instance of EventHandler.</param>
        /// <param name="eventArgs">Instance of AsyncSocketSessionEventArgs.</param>
        private void RaiseEvent(EventHandler<AsyncSocketSessionEventArgs> eventHandler, AsyncSocketSessionEventArgs eventArgs)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<AsyncSocketSessionEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, eventArgs);
            }
        }

        /// <summary>
        /// Method RaiseEvent.
        /// </summary>
        /// <param name="eventHandler">Instance of EventHandler.</param>
        /// <param name="eventArgs">Instance of AsyncSocketErrorEventArgs.</param>
        private void RaiseEvent(EventHandler<AsyncSocketErrorEventArgs> eventHandler, AsyncSocketErrorEventArgs eventArgs)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<AsyncSocketErrorEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, eventArgs);
            }
        }

        /// <summary>
        /// Method RaiseEvent.
        /// </summary>
        /// <param name="eventHandler">Instance of EventHandler.</param>
        private void RaiseEvent(EventHandler eventHandler)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, null);
            }
        }

        /// <summary>
        /// Method CloseListenSocket.
        /// </summary>
        private void CloseListenSocket()
        {
            if (this._listenSocket != null)
            {
                try
                {
                    this._listenSocket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                }
                finally
                {
                    this._listenSocket.Close();
                    this._listenSocket = null;
                }
            }
        }

        /// <summary>
        /// Method ClearSessionDictionary.
        /// </summary>
        private void ClearSessionDictionary()
        {
            if (this._sessionDictionary != null)
            {
                lock (((ICollection)this._sessionDictionary).SyncRoot)
                {
                    foreach (KeyValuePair<int, SocketAsyncEventArgs> item in this._sessionDictionary)
                    {
                        if (item.Value != null)
                        {
                            this.CloseSessionSocketAsyncEventArgs(item.Value);
                        }
                    }

                    this._sessionDictionary.Clear();
                    this._connectedSocketsCount = 0;
                }
            }
        }

        /// <summary>
        /// Method StartAccept.
        /// </summary>
        private void StartAccept()
        {
            if (this.IsListening)
            {
                if (this._acceptSocketAsyncEventArgs == null)
                {
                    this._acceptSocketAsyncEventArgs = new SocketAsyncEventArgs();
                    this._acceptSocketAsyncEventArgs.Completed += this.AcceptSocketAsyncEventArgsCompleted;
                }
                else
                {
                    this._acceptSocketAsyncEventArgs.AcceptSocket = null;
                }

                try
                {
                    bool willRaiseEvent = this._listenSocket.AcceptAsync(this._acceptSocketAsyncEventArgs);
                    if (!willRaiseEvent)
                    {
                        this.ProcessAccept();
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);

                    this.RaiseEvent(
                                    this.ErrorOccurred,
                                    new AsyncSocketErrorEventArgs(
                                                                  AsyncSocketTcpServerConstants.TcpServerAcceptSessionException,
                                                                  e,
                                                                  AsyncSocketErrorCodeEnum.TcpServerAcceptSessionException));

                    this.StartAccept();
                }
            }
        }

        /// <summary>
        /// Method AcceptSocketAsyncEventArgsCompleted.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="acceptSocketAsyncEventArgs">Instance of SocketAsyncEventArgs.</param>
        private void AcceptSocketAsyncEventArgsCompleted(object sender, SocketAsyncEventArgs acceptSocketAsyncEventArgs)
        {
            try
            {
                switch (acceptSocketAsyncEventArgs.LastOperation)
                {
                    case SocketAsyncOperation.Accept:
                        this.ProcessAccept();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Method ProcessAccept.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        private void ProcessAccept()
        {
            if (this._acceptSocketAsyncEventArgs != null && this._acceptSocketAsyncEventArgs.SocketError == SocketError.Success)
            {
                SocketAsyncEventArgs receiveSocketAsyncEventArgs = null;

                try
                {
                    receiveSocketAsyncEventArgs = new SocketAsyncEventArgs();
                    receiveSocketAsyncEventArgs.UserToken = this._acceptSocketAsyncEventArgs.AcceptSocket;
                    receiveSocketAsyncEventArgs.SetBuffer(new byte[this.BufferSize], 0, this.BufferSize);
                    receiveSocketAsyncEventArgs.Completed += this.ReceiveSocketAsyncEventArgsCompleted;

                    int sessionId = receiveSocketAsyncEventArgs.UserToken.GetHashCode();
                    IPEndPoint sessionIPEndPoint = this._acceptSocketAsyncEventArgs.AcceptSocket.RemoteEndPoint as IPEndPoint;

                    this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

                    try
                    {
                        if (!this._sessionDictionary.ContainsKey(sessionId))
                        {
                            this._sessionDictionary.Add(sessionId, receiveSocketAsyncEventArgs);
                            Interlocked.Increment(ref this._connectedSocketsCount);
                        }
                        else
                        {
                            this._sessionDictionary[sessionId] = receiveSocketAsyncEventArgs;
                        }

                        if (this._connectedSocketsCount > this.PeakConnectedSocketsCount)
                        {
                            this.PeakConnectedSocketsCount = Interlocked.Read(ref this._connectedSocketsCount);
                        }

                        this.RaiseEvent(this.Connected, new AsyncSocketSessionEventArgs(sessionId, sessionIPEndPoint));
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);

                        this.RaiseEvent(
                                        this.ErrorOccurred,
                                        new AsyncSocketErrorEventArgs(
                                                                      AsyncSocketTcpServerConstants.TcpServerAcceptSessionException,
                                                                      e,
                                                                      AsyncSocketErrorCodeEnum.TcpServerAcceptSessionException));
                    }
                    finally
                    {
                        this._readerWriterLock.ReleaseWriterLock();
                    }

                    try
                    {
                        bool willRaiseEvent = this._acceptSocketAsyncEventArgs.AcceptSocket.ReceiveAsync(receiveSocketAsyncEventArgs);
                        if (!willRaiseEvent)
                        {
                            this.ProcessReceive(receiveSocketAsyncEventArgs);
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);

                        this.RaiseEvent(
                                        this.ErrorOccurred,
                                        new AsyncSocketErrorEventArgs(
                                                                      AsyncSocketTcpServerConstants.TcpServerReceiveException,
                                                                      e,
                                                                      AsyncSocketErrorCodeEnum.TcpServerReceiveException));
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);

                    this.RaiseEvent(
                                    this.ErrorOccurred,
                                    new AsyncSocketErrorEventArgs(
                                                                  AsyncSocketTcpServerConstants.TcpServerAcceptSessionException,
                                                                  e,
                                                                  AsyncSocketErrorCodeEnum.TcpServerAcceptSessionException));

                    if (receiveSocketAsyncEventArgs != null)
                    {
                        receiveSocketAsyncEventArgs.Completed -= this.ReceiveSocketAsyncEventArgsCompleted;
                        receiveSocketAsyncEventArgs.UserToken = null;
                        receiveSocketAsyncEventArgs.Dispose();
                        receiveSocketAsyncEventArgs = null;
                    }
                }
            }

            this.StartAccept();
        }

        /// <summary>
        /// Method ReceiveSocketAsyncEventArgsCompleted.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="receiveSocketAsyncEventArgs">Instance of SocketAsyncEventArgs.</param>
        private void ReceiveSocketAsyncEventArgsCompleted(object sender, SocketAsyncEventArgs receiveSocketAsyncEventArgs)
        {
            try
            {
                switch (receiveSocketAsyncEventArgs.LastOperation)
                {
                    case SocketAsyncOperation.Receive:
                        this.ProcessReceive(receiveSocketAsyncEventArgs);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Method ProcessReceive.
        /// </summary>
        /// <param name="receiveSocketAsyncEventArgs">Instance of SocketAsyncEventArgs.</param>
        private void ProcessReceive(SocketAsyncEventArgs receiveSocketAsyncEventArgs)
        {
            if (receiveSocketAsyncEventArgs != null && receiveSocketAsyncEventArgs.BytesTransferred > 0 && receiveSocketAsyncEventArgs.SocketError == SocketError.Success)
            {
                int sessionId = -1;

                try
                {
                    sessionId = receiveSocketAsyncEventArgs.UserToken.GetHashCode();
                }
                catch
                {
                }

                IPEndPoint sessionIPEndPoint = null;

                try
                {
                    sessionIPEndPoint = (receiveSocketAsyncEventArgs.UserToken as Socket).RemoteEndPoint as IPEndPoint;
                }
                catch
                {
                }

                try
                {
                    Interlocked.Add(ref this._totalBytesReceived, receiveSocketAsyncEventArgs.BytesTransferred);

                    Socket sessionSocket = receiveSocketAsyncEventArgs.UserToken as Socket;

                    this.RaiseEvent(
                                    this.DataReceived,
                                    new AsyncSocketSessionEventArgs(
                                                                    sessionId,
                                                                    sessionIPEndPoint,
                                                                    receiveSocketAsyncEventArgs.Buffer,
                                                                    receiveSocketAsyncEventArgs.BytesTransferred,
                                                                    receiveSocketAsyncEventArgs.Offset));

                    try
                    {
                        bool willRaiseEvent = sessionSocket.ReceiveAsync(receiveSocketAsyncEventArgs);
                        if (!willRaiseEvent)
                        {
                            this.ProcessReceive(receiveSocketAsyncEventArgs);
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);

                    this.RaiseEvent(
                                    this.ErrorOccurred,
                                    new AsyncSocketErrorEventArgs(
                                                                  AsyncSocketTcpServerConstants.TcpServerReceiveException,
                                                                  e,
                                                                  AsyncSocketErrorCodeEnum.TcpServerReceiveException));
                }
            }
            else
            {
                int sessionId = -1;

                try
                {
                    sessionId = receiveSocketAsyncEventArgs.UserToken.GetHashCode();
                }
                catch
                {
                }

                IPEndPoint sessionIPEndPoint = null;

                try
                {
                    sessionIPEndPoint = (receiveSocketAsyncEventArgs.UserToken as Socket).RemoteEndPoint as IPEndPoint;
                }
                catch
                {
                }

                this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

                try
                {
                    if (this._sessionDictionary.ContainsKey(sessionId))
                    {
                        this._sessionDictionary.Remove(sessionId);
                        Interlocked.Decrement(ref this._connectedSocketsCount);
                    }

                    this.CloseSessionSocketAsyncEventArgs(receiveSocketAsyncEventArgs);

                    this.RaiseEvent(this.Disconnected, new AsyncSocketSessionEventArgs(sessionId, sessionIPEndPoint));
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                }
                finally
                {
                    this._readerWriterLock.ReleaseWriterLock();
                }
            }
        }

        /// <summary>
        /// Method SendSocketAsyncEventArgsCompleted.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="sendSocketAsyncEventArgs">Instance of SocketAsyncEventArgs.</param>
        private void SendSocketAsyncEventArgsCompleted(object sender, SocketAsyncEventArgs sendSocketAsyncEventArgs)
        {
            try
            {
                switch (sendSocketAsyncEventArgs.LastOperation)
                {
                    case SocketAsyncOperation.Send:
                        this.ProcessSend(sendSocketAsyncEventArgs);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Method ProcessSend.
        /// </summary>
        /// <param name="sendSocketAsyncEventArgs">Instance of SocketAsyncEventArgs.</param>
        private void ProcessSend(SocketAsyncEventArgs sendSocketAsyncEventArgs)
        {
            if (sendSocketAsyncEventArgs != null && sendSocketAsyncEventArgs.BytesTransferred > 0 && sendSocketAsyncEventArgs.SocketError == SocketError.Success)
            {
                int sessionId = -1;

                try
                {
                    sessionId = sendSocketAsyncEventArgs.UserToken.GetHashCode();
                }
                catch
                {
                }

                IPEndPoint sessionIPEndPoint = null;

                try
                {
                    sessionIPEndPoint = (sendSocketAsyncEventArgs.UserToken as Socket).RemoteEndPoint as IPEndPoint;
                }
                catch
                {
                }

                try
                {
                    Interlocked.Add(ref this._totalBytesSent, sendSocketAsyncEventArgs.BytesTransferred);

                    this.RaiseEvent(
                                    this.DataSent,
                                    new AsyncSocketSessionEventArgs(
                                                                    sessionId,
                                                                    sessionIPEndPoint,
                                                                    sendSocketAsyncEventArgs.Buffer,
                                                                    sendSocketAsyncEventArgs.BytesTransferred,
                                                                    sendSocketAsyncEventArgs.Offset));
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                }
            }

            if (sendSocketAsyncEventArgs != null)
            {
                sendSocketAsyncEventArgs.Completed -= this.SendSocketAsyncEventArgsCompleted;
                sendSocketAsyncEventArgs.UserToken = null;
                sendSocketAsyncEventArgs.Dispose();
                sendSocketAsyncEventArgs = null;
            }
        }

        /// <summary>
        /// Method CloseAcceptSocketAsyncEventArgs.
        /// </summary>
        private void CloseAcceptSocketAsyncEventArgs()
        {
            if (this._acceptSocketAsyncEventArgs != null)
            {
                this._acceptSocketAsyncEventArgs.Completed -= this.AcceptSocketAsyncEventArgsCompleted;

                Socket sessionSocket = this._acceptSocketAsyncEventArgs.UserToken as Socket;

                if (sessionSocket != null)
                {
                    try
                    {
                        sessionSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        sessionSocket.Close();
                        sessionSocket = null;
                    }
                }

                if (this._acceptSocketAsyncEventArgs.AcceptSocket != null)
                {
                    try
                    {
                        this._acceptSocketAsyncEventArgs.AcceptSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        this._acceptSocketAsyncEventArgs.AcceptSocket.Close();
                        this._acceptSocketAsyncEventArgs.AcceptSocket = null;
                    }
                }

                this._acceptSocketAsyncEventArgs.UserToken = null;
                this._acceptSocketAsyncEventArgs.Dispose();
                this._acceptSocketAsyncEventArgs = null;
            }
        }

        /// <summary>
        /// Method CloseSessionSocketAsyncEventArgs.
        /// </summary>
        /// <param name="receiveSocketAsyncEventArgs">Instance of SocketAsyncEventArgs.</param>
        private void CloseSessionSocketAsyncEventArgs(SocketAsyncEventArgs receiveSocketAsyncEventArgs)
        {
            if (receiveSocketAsyncEventArgs != null)
            {
                receiveSocketAsyncEventArgs.Completed -= this.ReceiveSocketAsyncEventArgsCompleted;

                if (receiveSocketAsyncEventArgs.UserToken != null)
                {
                    Socket sessionSocket = receiveSocketAsyncEventArgs.UserToken as Socket;

                    if (sessionSocket != null)
                    {
                        try
                        {
                            sessionSocket.Shutdown(SocketShutdown.Both);
                        }
                        catch
                        {
                        }
                        finally
                        {
                            sessionSocket.Close();
                            sessionSocket = null;
                        }
                    }
                }

                if (receiveSocketAsyncEventArgs.AcceptSocket != null)
                {
                    try
                    {
                        receiveSocketAsyncEventArgs.AcceptSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        receiveSocketAsyncEventArgs.AcceptSocket.Close();
                        receiveSocketAsyncEventArgs.AcceptSocket = null;
                    }
                }

                receiveSocketAsyncEventArgs.UserToken = null;
                receiveSocketAsyncEventArgs.Dispose();
                receiveSocketAsyncEventArgs = null;
            }
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.Net.AsyncSocket.AsyncSocketTcpServer");
            }
        }
    }
}
