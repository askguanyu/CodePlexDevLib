﻿//-----------------------------------------------------------------------
// <copyright file="AsyncSocketServer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.AsyncSocket
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// Implements the connection logic for the socket server.
    /// </summary>
    public class AsyncSocketServer : IDisposable
    {
        /// <summary>
        /// Thread-safe dictionary of connected socket tokens
        /// </summary>
        private ConcurrentDictionary<Guid, AsyncSocketUserTokenEventArgs> _tokens;

        /// <summary>
        /// Thread-safe dictionary of connected socket tokens with IP as primary key
        /// </summary>
        private ConcurrentDictionary<IPAddress, AsyncSocketUserTokenEventArgs> _singleIPTokens;

        /// <summary>
        /// the maximum number of connections the class is designed to handle simultaneously
        /// </summary>
        private int _numConnections;

        /// <summary>
        /// buffer size to use for each socket I/O operation
        /// </summary>
        private int _bufferSize;

        /// <summary>
        /// represents a large reusable set of buffers for all socket operations
        /// </summary>
        private AsyncSocketServerEventArgsBufferManager _bufferManager;

        /// <summary>
        /// the socket used to listen for incoming connection requests
        /// </summary>
        private Socket _listenSocket;

        /// <summary>
        /// pool of reusable SocketAsyncEventArgs objects for read and accept socket operations
        /// </summary>
        private AsyncSocketServerEventArgsPool _readPool;

        /// <summary>
        /// pool of reusable SocketAsyncEventArgs objects for write and accept socket operations
        /// </summary>
        private AsyncSocketServerEventArgsPool _writePool;

        /// <summary>
        /// counter of the total bytes received by the server
        /// </summary>
        private long _totalBytesRead;

        /// <summary>
        /// counter of the total bytes sent by the server
        /// </summary>
        private long _totalBytesWrite;

        /// <summary>
        /// the total number of clients connected to the server
        /// </summary>
        private long _numConnectedSockets;

        /// <summary>
        /// the max number of accepted clients
        /// </summary>
        private Semaphore _maxNumberAcceptedClients;

        /// <summary>
        /// Constructor of AsyncSocketServer
        /// </summary>
        /// <param name="localEndPoint">local port to listen</param>
        /// <param name="numConnections">the maximum number of connections the class is designed to handle simultaneously</param>
        /// <param name="bufferSize">buffer size to use for each socket I/O operation</param>
        public AsyncSocketServer(IPEndPoint localEndPoint, int numConnections = AsyncSocketServerConstants.NumConnections, int bufferSize = AsyncSocketServerConstants.BufferSize)
        {
            this._totalBytesRead = 0;
            this._totalBytesWrite = 0;
            this._numConnectedSockets = 0;
            this._numConnections = numConnections;
            this._bufferSize = bufferSize;
            this.LocalEndPoint = localEndPoint;
        }

        /// <summary>
        /// Constructor of AsyncSocketServer
        /// </summary>
        /// <param name="localPort">local port to listen</param>
        /// <param name="numConnections">the maximum number of connections the class is designed to handle simultaneously</param>
        /// <param name="bufferSize">buffer size to use for each socket I/O operation</param>
        public AsyncSocketServer(int localPort, int numConnections = AsyncSocketServerConstants.NumConnections, int bufferSize = AsyncSocketServerConstants.BufferSize)
        {
            this._totalBytesRead = 0;
            this._totalBytesWrite = 0;
            this._numConnectedSockets = 0;
            this._numConnections = numConnections;
            this._bufferSize = bufferSize;
            this.LocalEndPoint = new IPEndPoint(IPAddress.Any, localPort);
        }

        /// <summary>
        /// Client Connected Event
        /// </summary>
        public event EventHandler<AsyncSocketUserTokenEventArgs> Connected;

        /// <summary>
        /// Client Disconnected Event
        /// </summary>
        public event EventHandler<AsyncSocketUserTokenEventArgs> Disconnected;

        /// <summary>
        /// Server Data Received Event
        /// </summary>
        public event EventHandler<AsyncSocketUserTokenEventArgs> DataReceived;

        /// <summary>
        /// Server Data Sent Event
        /// </summary>
        public event EventHandler<AsyncSocketUserTokenEventArgs> DataSent;

        /// <summary>
        /// Error Occurred Event
        /// </summary>
        public event EventHandler<AsyncSocketErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// Gets a value indicating whether socket server is listening
        /// </summary>
        public bool IsListening
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets Local EndPoint
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets numbers of connected sockets
        /// </summary>
        public long NumConnectedSockets
        {
            get { return _numConnectedSockets; }
        }

        /// <summary>
        /// Gets numbers of connected clients
        /// </summary>
        public int NumConnectedClients
        {
            get { return this._singleIPTokens.Count; }
        }

        /// <summary>
        /// Gets total bytes read
        /// </summary>
        public long TotalBytesRead
        {
            get { return _totalBytesRead; }
        }

        /// <summary>
        /// Gets total bytes write
        /// </summary>
        public long TotalBytesWrite
        {
            get { return _totalBytesWrite; }
        }

        /// <summary>
        /// Whether connected client is online or not
        /// </summary>
        /// <param name="connectionId">Connection Id</param>
        /// <returns>true if online, else false</returns>
        public bool IsOnline(Guid connectionId)
        {
            return this._tokens.ContainsKey(connectionId);
        }

        /// <summary>
        /// Whether connected client is online or not
        /// </summary>
        /// <param name="connectionIP">Connection IP</param>
        /// <returns>true if online, else false</returns>
        public bool IsOnline(IPAddress connectionIP)
        {
            return this._singleIPTokens.ContainsKey(connectionIP);
        }

        /// <summary>
        /// Start socket server
        /// </summary>
        /// <param name="useIOCP">Specifies whether the socket should only use Overlapped I/O mode.</param>
        public void Start(bool useIOCP = true)
        {
            this.Start(this.LocalEndPoint, useIOCP);
        }

        /// <summary>
        /// Start socket server to listen specific local port
        /// </summary>
        /// <param name="localEndPoint">local port to listen</param>
        /// <param name="useIOCP">Specifies whether the socket should only use Overlapped I/O mode.</param>
        public void Start(IPEndPoint localEndPoint, bool useIOCP = true)
        {
            if (!this.IsListening)
            {
                this.InitializePool();

                try
                {
                    if (null != this._listenSocket)
                    {
                        this._listenSocket.Close();
                        this._listenSocket.Dispose();
                        this._listenSocket = null;
                    }

                    this._listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    this._listenSocket.Bind(localEndPoint);
                    this._listenSocket.Listen(AsyncSocketServerConstants.Backlog);
                    this._listenSocket.UseOnlyOverlappedIO = useIOCP;
                }
                catch (ObjectDisposedException)
                {
                    this.IsListening = false;
                    throw;
                }
                catch (SocketException e)
                {
                    this.IsListening = false;
                    this.OnErrorOccurred(null, new AsyncSocketErrorEventArgs(AsyncSocketServerConstants.SocketStartException, e, AsyncSocketErrorCodeEnum.ServerStartException));
                    throw;
                }
                catch (Exception e)
                {
                    this.IsListening = false;
                    Debug.WriteLine(string.Format(AsyncSocketServerConstants.ExceptionStringFormat, "AsyncSocketServer.Start", e.Source, e.Message, e.StackTrace));
                    throw;
                }

                this.IsListening = true;
                StartAccept(null);
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.SocketStartSuccessfully));
            }
        }

        /// <summary>
        /// Send the data back to the client
        /// </summary>
        /// <param name="connectionId">Client connection Id</param>
        /// <param name="buffer">Data to send</param>
        /// <param name="operation">user defined operation</param>
        public void Send(Guid connectionId, byte[] buffer, object operation = null)
        {
            AsyncSocketUserTokenEventArgs token;

            if (!this._tokens.TryGetValue(connectionId, out token))
            {
                this.OnErrorOccurred(null, new AsyncSocketErrorEventArgs(string.Format(AsyncSocketServerConstants.ClientClosedStringFormat, connectionId), null, AsyncSocketErrorCodeEnum.SocketNoExist));
                throw new Exception();
            }

            SocketAsyncEventArgs writeEventArgs;
            writeEventArgs = _writePool.Pop();
            writeEventArgs.UserToken = token;
            token.Operation = operation;

            if (buffer.Length <= _bufferSize)
            {
                Array.Copy(buffer, 0, writeEventArgs.Buffer, writeEventArgs.Offset, buffer.Length);
            }
            else
            {
                _bufferManager.FreeBuffer(writeEventArgs);

                writeEventArgs.SetBuffer(buffer, 0, buffer.Length);
            }

            try
            {
                bool willRaiseEvent = token.Socket.SendAsync(writeEventArgs);
                if (!willRaiseEvent)
                {
                    this.ProcessSend(writeEventArgs);
                }
            }
            catch (ObjectDisposedException e)
            {
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.ExceptionStringFormat, "AsyncSocketServer.Send", e.Source, e.Message, e.StackTrace));
                this.RaiseDisconnectedEvent(token);
            }
            catch (SocketException socketException)
            {
                if (socketException.ErrorCode == (int)SocketError.ConnectionReset)
                {
                    this.RaiseDisconnectedEvent(token);
                }
                else
                {
                    this.OnErrorOccurred(token, new AsyncSocketErrorEventArgs(AsyncSocketServerConstants.SocketSendException, socketException, AsyncSocketErrorCodeEnum.ServerSendBackException));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.ExceptionStringFormat, "AsyncSocketServer.Send", e.Source, e.Message, e.StackTrace));
                throw;
            }
        }

        /// <summary>
        /// Send the data back to the client
        /// </summary>
        /// <param name="connectionIP">Client connection IP</param>
        /// <param name="buffer">Data to send</param>
        public void Send(IPAddress connectionIP, byte[] buffer, object operation = null)
        {
            AsyncSocketUserTokenEventArgs token;

            if (!this._singleIPTokens.TryGetValue(connectionIP, out token))
            {
                this.OnErrorOccurred(null, new AsyncSocketErrorEventArgs(string.Format(AsyncSocketServerConstants.ClientClosedStringFormat, connectionIP), null, AsyncSocketErrorCodeEnum.SocketNoExist));
                throw new Exception();
            }

            SocketAsyncEventArgs writeEventArgs;
            writeEventArgs = _writePool.Pop();
            writeEventArgs.UserToken = token;
            token.Operation = operation;

            if (buffer.Length <= _bufferSize)
            {
                Array.Copy(buffer, 0, writeEventArgs.Buffer, writeEventArgs.Offset, buffer.Length);
            }
            else
            {
                _bufferManager.FreeBuffer(writeEventArgs);

                writeEventArgs.SetBuffer(buffer, 0, buffer.Length);
            }

            try
            {
                bool willRaiseEvent = token.Socket.SendAsync(writeEventArgs);
                if (!willRaiseEvent)
                {
                    this.ProcessSend(writeEventArgs);
                }
            }
            catch (ObjectDisposedException e)
            {
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.ExceptionStringFormat, "AsyncSocketServer.Send", e.Source, e.Message, e.StackTrace));
                this.RaiseDisconnectedEvent(token);
            }
            catch (SocketException socketException)
            {
                if (socketException.ErrorCode == (int)SocketError.ConnectionReset)
                {
                    this.RaiseDisconnectedEvent(token);
                }
                else
                {
                    this.OnErrorOccurred(token, new AsyncSocketErrorEventArgs(AsyncSocketServerConstants.SocketSendException, socketException, AsyncSocketErrorCodeEnum.ServerSendBackException));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.ExceptionStringFormat, "AsyncSocketServer.Send", e.Source, e.Message, e.StackTrace));
                throw;
            }
        }

        /// <summary>
        /// Disconnect client
        /// </summary>
        /// <param name="connectionId">Client connection Id</param>
        public void Disconnect(Guid connectionId)
        {
            AsyncSocketUserTokenEventArgs token;

            if (!this._tokens.TryGetValue(connectionId, out token))
            {
                this.OnErrorOccurred(null, new AsyncSocketErrorEventArgs(string.Format(AsyncSocketServerConstants.ClientClosedStringFormat, connectionId), null, AsyncSocketErrorCodeEnum.SocketNoExist));
                throw new Exception();
            }

            this.RaiseDisconnectedEvent(token);
        }

        /// <summary>
        /// Stop socket server
        /// </summary>
        public void Stop()
        {
            if (this.IsListening)
            {
                try
                {
                    this._listenSocket.Close();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(string.Format(AsyncSocketServerConstants.ExceptionStringFormat, "AsyncSocketServer.Stop", e.Source, e.Message, e.StackTrace));
                    throw;
                }
                finally
                {
                    foreach (AsyncSocketUserTokenEventArgs token in this._tokens.Values)
                    {
                        try
                        {
                            this.CloseClientSocket(token);

                            if (null != token)
                            {
                                this.RaiseEvent(Disconnected, token);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(string.Format(AsyncSocketServerConstants.ExceptionStringFormat, "AsyncSocketServer.Stop", e.Source, e.Message, e.StackTrace));
                            throw;
                        }
                    }

                    this._tokens.Clear();
                    this._singleIPTokens.Clear();
                }

                this.IsListening = false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <param name="eventArgs"></param>
        protected virtual void RaiseEvent(EventHandler<AsyncSocketUserTokenEventArgs> eventHandler, AsyncSocketUserTokenEventArgs eventArgs)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<AsyncSocketUserTokenEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, eventArgs);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnErrorOccurred(object sender, AsyncSocketErrorEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<AsyncSocketErrorEventArgs> temp = Interlocked.CompareExchange(ref ErrorOccurred, null, null);

            if (temp != null)
            {
                temp(sender, e);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                if (this._listenSocket != null)
                {
                    this._listenSocket.Close();
                    this._listenSocket.Dispose();
                }

                this._maxNumberAcceptedClients.Dispose();
            }

            // free native resources
        }

        /// <summary>
        /// Initialize Read/Write Pool
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void InitializePool()
        {
            this._bufferManager = new AsyncSocketServerEventArgsBufferManager(_bufferSize * _numConnections * AsyncSocketServerConstants.OpsToPreAlloc, _bufferSize);
            this._readPool = new AsyncSocketServerEventArgsPool();
            this._writePool = new AsyncSocketServerEventArgsPool();
            this._tokens = new ConcurrentDictionary<Guid, AsyncSocketUserTokenEventArgs>();
            this._singleIPTokens = new ConcurrentDictionary<IPAddress, AsyncSocketUserTokenEventArgs>();
            this._maxNumberAcceptedClients = new Semaphore(_numConnections, _numConnections);
            this._bufferManager.InitBuffer();
            SocketAsyncEventArgs readWriteEventArg;
            AsyncSocketUserTokenEventArgs token;

            /// Initialize read Pool
            for (int i = 0; i < _numConnections; i++)
            {
                token = new AsyncSocketUserTokenEventArgs();
                token.ReadEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                this._bufferManager.SetBuffer(token.ReadEventArgs);
                token.SetBuffer(token.ReadEventArgs.Buffer, token.ReadEventArgs.Offset);
                this._readPool.Push(token.ReadEventArgs);
            }

            /// Initialize write Pool
            for (int i = 0; i < _numConnections; i++)
            {
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                readWriteEventArg.UserToken = null;
                this._bufferManager.SetBuffer(readWriteEventArg);
                this._writePool.Push(readWriteEventArg);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="acceptEventArg"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                acceptEventArg.AcceptSocket = null;
            }

            try
            {
                _maxNumberAcceptedClients.WaitOne();

                bool willRaiseEvent = _listenSocket.AcceptAsync(acceptEventArg);
                if (!willRaiseEvent)
                {
                    this.ProcessAccept(acceptEventArg);
                }
            }
            catch (ObjectDisposedException e)
            {
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.ExceptionStringFormat, "AsyncSocketServer.StartAccept", e.Source, e.Message, e.StackTrace));
            }
            catch (SocketException e)
            {
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.ExceptionStringFormat, "AsyncSocketServer.StartAccept", e.Source, e.Message, e.StackTrace));
                this.OnErrorOccurred(null, new AsyncSocketErrorEventArgs(AsyncSocketServerConstants.SocketAcceptedException, e, AsyncSocketErrorCodeEnum.ServerAcceptException));
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.ExceptionStringFormat, "AsyncSocketServer.StartAccept", e.Source, e.Message, e.StackTrace));
                throw;
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessAccept(e);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            AsyncSocketUserTokenEventArgs token;
            Interlocked.Increment(ref _numConnectedSockets);
            Debug.WriteLine(string.Format(AsyncSocketServerConstants.ClientConnectionStringFormat, _numConnectedSockets.ToString()));
            SocketAsyncEventArgs readEventArg;
            readEventArg = _readPool.Pop();
            token = (AsyncSocketUserTokenEventArgs)readEventArg.UserToken;
            token.Socket = e.AcceptSocket;
            token.ConnectionId = Guid.NewGuid();
            this._tokens.GetOrAdd(token.ConnectionId, token);
            this._singleIPTokens.AddOrUpdate(token.EndPoint.Address, token, (key, oldValue) => oldValue);
            this.RaiseEvent(Connected, token);

            try
            {
                bool willRaiseEvent = token.Socket.ReceiveAsync(readEventArg);
                if (!willRaiseEvent)
                {
                    this.ProcessReceive(readEventArg);
                }
            }
            catch (ObjectDisposedException)
            {
                this.RaiseDisconnectedEvent(token);
            }
            catch (SocketException socketException)
            {
                if (socketException.ErrorCode == (int)SocketError.ConnectionReset)
                {
                    this.RaiseDisconnectedEvent(token);
                }
                else
                {
                    this.OnErrorOccurred(token, new AsyncSocketErrorEventArgs(AsyncSocketServerConstants.SocketReceiveException, socketException));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.ExceptionStringFormat, "AsyncSocketServer.ProcessAccept", ex.Source, ex.Message, ex.StackTrace));
                this.OnErrorOccurred(token, new AsyncSocketErrorEventArgs(ex.Message, ex, AsyncSocketErrorCodeEnum.ThrowSocketException));
            }
            finally
            {
                this.StartAccept(e);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    this.ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    this.ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException(AsyncSocketServerConstants.SocketLastOperationException);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            AsyncSocketUserTokenEventArgs token = (AsyncSocketUserTokenEventArgs)e.UserToken;

            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                Interlocked.Add(ref _totalBytesRead, e.BytesTransferred);
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.ServerReceiveTotalBytesStringFormat, this._totalBytesRead.ToString()));
                token.SetBytesReceived(e.BytesTransferred);
                this.RaiseEvent(DataReceived, token);

                try
                {
                    bool willRaiseEvent = token.Socket.ReceiveAsync(e);
                    if (!willRaiseEvent)
                    {
                        this.ProcessReceive(e);
                    }
                }
                catch (ObjectDisposedException)
                {
                    this.RaiseDisconnectedEvent(token);
                }
                catch (SocketException socketException)
                {
                    if (socketException.ErrorCode == (int)SocketError.ConnectionReset)
                    {
                        this.RaiseDisconnectedEvent(token);
                    }
                    else
                    {
                        this.OnErrorOccurred(token, new AsyncSocketErrorEventArgs(AsyncSocketServerConstants.SocketReceiveException, socketException));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format(AsyncSocketServerConstants.ExceptionStringFormat, "AsyncSocketServer.ProcessReceive", ex.Source, ex.Message, ex.StackTrace));
                    throw;
                }
            }
            else
            {
                this.RaiseDisconnectedEvent(token);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            AsyncSocketUserTokenEventArgs token = (AsyncSocketUserTokenEventArgs)e.UserToken;
            Interlocked.Add(ref _totalBytesWrite, e.BytesTransferred);

            if (e.Count > _bufferSize)
            {
                _bufferManager.SetBuffer(e);
            }

            _writePool.Push(e);
            e.UserToken = null;

            if (e.SocketError == SocketError.Success)
            {
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.ServerSendTotalBytesStringFormat, e.BytesTransferred.ToString()));

                this.RaiseEvent(DataSent, token);
            }
            else
            {
                this.RaiseDisconnectedEvent(token);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="token"></param>
        private void RaiseDisconnectedEvent(AsyncSocketUserTokenEventArgs token)
        {
            if (null != token)
            {
                AsyncSocketUserTokenEventArgs outSingleIPToken;
                this._singleIPTokens.TryRemove(token.EndPoint.Address, out outSingleIPToken);

                AsyncSocketUserTokenEventArgs outToken;
                if (this._tokens.TryRemove(token.ConnectionId, out outToken))
                {
                    this.CloseClientSocket(token);

                    if (null != token)
                    {
                        this.RaiseEvent(Disconnected, token);
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="token"></param>
        private void CloseClientSocket(AsyncSocketUserTokenEventArgs token)
        {
            try
            {
                token.Socket.Shutdown(SocketShutdown.Both);
                token.Socket.Close();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException)
            {
                token.Socket.Close();
            }
            catch (Exception e)
            {
                token.Socket.Close();
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.ExceptionStringFormat, "AsyncSocketServer.CloseClientSocket", e.Source, e.Message, e.StackTrace));
                throw;
            }
            finally
            {
                Interlocked.Decrement(ref _numConnectedSockets);
                this._maxNumberAcceptedClients.Release();

                Debug.WriteLine(string.Format(AsyncSocketServerConstants.ClientConnectionStringFormat, _numConnectedSockets.ToString()));

                _readPool.Push(token.ReadEventArgs);
            }
        }
    }
}
