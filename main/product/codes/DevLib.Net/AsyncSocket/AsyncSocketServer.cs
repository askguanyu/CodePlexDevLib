//-----------------------------------------------------------------------
// <copyright file="AsyncSocketServer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.AsyncSocket
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
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
        ///
        /// </summary>
        private Object _bufferLock = new Object();

        /// <summary>
        ///
        /// </summary>
        private Object _readPoolLock = new Object();

        /// <summary>
        ///
        /// </summary>
        private Object _writePoolLock = new Object();

        /// <summary>
        ///
        /// </summary>
        private Dictionary<Guid, AsyncSocketUserTokenEventArgs> _tokens;

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
        /// Client Connected Event
        /// </summary>
        public event EventHandler<AsyncSocketUserTokenEventArgs> Connected;

        /// <summary>
        /// Client Disconnected Event
        /// </summary>
        public event EventHandler<AsyncSocketUserTokenEventArgs> Disconnected;

        /// <summary>
        /// Error Occurred Event
        /// </summary>
        public event EventHandler<AsyncSocketErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// Server Data Received Event
        /// </summary>
        public event EventHandler<AsyncSocketUserTokenEventArgs> DataReceived;

        /// <summary>
        /// Server Data Sent Event
        /// </summary>
        public event EventHandler<AsyncSocketUserTokenEventArgs> DataSent;

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
            lock (((ICollection)this._tokens).SyncRoot)
            {
                return this._tokens.ContainsKey(connectionId);
            }
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
                catch (SocketException ex)
                {
                    this.IsListening = false;
                    this.OnErrorOccurred(null, new AsyncSocketErrorEventArgs(AsyncSocketServerConstants.SocketStartException, ex, AsyncSocketErrorCodeEnum.ServerStartException));
                    throw;
                }
                catch (Exception ex)
                {
                    this.IsListening = false;
                    Debug.WriteLine(string.Format(AsyncSocketServerConstants.DebugStringFormat, ex.Message));
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
        public void Send(Guid connectionId, byte[] buffer)
        {
            AsyncSocketUserTokenEventArgs token;

            lock (((ICollection)this._tokens).SyncRoot)
            {
                if (!this._tokens.TryGetValue(connectionId, out token))
                {
                    this.OnErrorOccurred(null, new AsyncSocketErrorEventArgs(string.Format(AsyncSocketServerConstants.ClientClosedStringFormat, connectionId), null, AsyncSocketErrorCodeEnum.SocketNoExist));
                    throw new Exception();
                }
            }

            SocketAsyncEventArgs writeEventArgs;

            lock (_writePool)
            {
                writeEventArgs = _writePool.Pop();
            }

            writeEventArgs.UserToken = token;

            if (buffer.Length <= _bufferSize)
            {
                Array.Copy(buffer, 0, writeEventArgs.Buffer, writeEventArgs.Offset, buffer.Length);
                writeEventArgs.SetBuffer(writeEventArgs.Buffer, writeEventArgs.Offset, buffer.Length);
            }
            else
            {
                lock (_bufferLock)
                {
                    _bufferManager.FreeBuffer(writeEventArgs);
                }

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
                    this.OnErrorOccurred(token, new AsyncSocketErrorEventArgs(AsyncSocketServerConstants.SocketSendException, socketException, AsyncSocketErrorCodeEnum.ServerSendBackException));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.DebugStringFormat, ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Send the data back to the client
        /// </summary>
        /// <param name="connectionId">Client connection Id</param>
        /// <param name="buffer">Data to send</param>
        /// <param name="operation">user defined operation</param>
        public void Send(Guid connectionId, byte[] buffer, object operation)
        {
            AsyncSocketUserTokenEventArgs token;

            lock (((ICollection)this._tokens).SyncRoot)
            {
                if (!this._tokens.TryGetValue(connectionId, out token))
                {
                    this.OnErrorOccurred(null, new AsyncSocketErrorEventArgs(string.Format(AsyncSocketServerConstants.ClientClosedStringFormat, connectionId), null, AsyncSocketErrorCodeEnum.SocketNoExist));
                    throw new Exception();
                }
            }

            SocketAsyncEventArgs writeEventArgs;

            lock (_writePool)
            {
                writeEventArgs = _writePool.Pop();
            }

            writeEventArgs.UserToken = token;
            token.Operation = operation;

            if (buffer.Length <= _bufferSize)
            {
                Array.Copy(buffer, 0, writeEventArgs.Buffer, writeEventArgs.Offset, buffer.Length);
            }
            else
            {
                lock (_bufferLock)
                {
                    _bufferManager.FreeBuffer(writeEventArgs);
                }

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
                    this.OnErrorOccurred(token, new AsyncSocketErrorEventArgs(AsyncSocketServerConstants.SocketSendException, socketException, AsyncSocketErrorCodeEnum.ServerSendBackException));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.DebugStringFormat, ex.Message));
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

            lock (((ICollection)this._tokens).SyncRoot)
            {
                if (!this._tokens.TryGetValue(connectionId, out token))
                {
                    this.OnErrorOccurred(null, new AsyncSocketErrorEventArgs(string.Format(AsyncSocketServerConstants.ClientClosedStringFormat, connectionId), null, AsyncSocketErrorCodeEnum.SocketNoExist));
                    throw new Exception();
                }
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
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format(AsyncSocketServerConstants.DebugStringFormat, ex.Message));
                    throw;
                }
                finally
                {
                    lock (((ICollection)this._tokens).SyncRoot)
                    {
                        foreach (AsyncSocketUserTokenEventArgs token in this._tokens.Values)
                        {
                            try
                            {
                                this.CloseClientSocket(token);

                                if (null != token)
                                {
                                    this.OnDisconnected(token);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(string.Format(AsyncSocketServerConstants.ClientClosedStringFormat, ex.Message));
                                throw;
                            }
                        }

                        this._tokens.Clear();
                    }
                }

                this.IsListening = false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConnected(AsyncSocketUserTokenEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<AsyncSocketUserTokenEventArgs> temp = Interlocked.CompareExchange(ref Connected, null, null);

            if ((temp != null) && (e.EndPoint != null))
            {
                temp(this, e);
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
        /// <param name="e"></param>
        protected virtual void OnDataReceived(AsyncSocketUserTokenEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<AsyncSocketUserTokenEventArgs> temp = Interlocked.CompareExchange(ref DataReceived, null, null);

            if ((temp != null) && (e.EndPoint != null))
            {
                temp(this, e);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataSent(AsyncSocketUserTokenEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<AsyncSocketUserTokenEventArgs> temp = Interlocked.CompareExchange(ref DataSent, null, null);

            if (temp != null)
            {
                temp(this, e);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(AsyncSocketUserTokenEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<AsyncSocketUserTokenEventArgs> temp = Interlocked.CompareExchange(ref Disconnected, null, null);

            if ((temp != null) && (e.EndPoint != null))
            {
                temp(this, e);
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
            this._readPool = new AsyncSocketServerEventArgsPool(_numConnections);
            this._writePool = new AsyncSocketServerEventArgsPool(_numConnections);
            this._tokens = new Dictionary<Guid, AsyncSocketUserTokenEventArgs>();
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
            catch (ObjectDisposedException ex)
            {
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.DebugStringFormat, ex.Message));
            }
            catch (SocketException ex)
            {
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.DebugStringFormat, ex.Message));
                this.OnErrorOccurred(null, new AsyncSocketErrorEventArgs(AsyncSocketServerConstants.SocketAcceptedException, ex, AsyncSocketErrorCodeEnum.ServerAcceptException));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.DebugStringFormat, ex.Message));
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

            lock (_readPool)
            {
                readEventArg = _readPool.Pop();
            }

            token = (AsyncSocketUserTokenEventArgs)readEventArg.UserToken;

            token.Socket = e.AcceptSocket;

            token.ConnectionId = Guid.NewGuid();

            lock (((ICollection)this._tokens).SyncRoot)
            {
                this._tokens.Add(token.ConnectionId, token);
            }

            this.OnConnected(token);

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
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.DebugStringFormat, ex.Message));
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

                this.OnDataReceived(token);

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
                    Debug.WriteLine(string.Format(AsyncSocketServerConstants.DebugStringFormat, ex.Message));
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
                lock (_bufferLock)
                {
                    _bufferManager.SetBuffer(e);
                }
            }

            lock (_writePool)
            {
                _writePool.Push(e);
            }

            e.UserToken = null;

            if (e.SocketError == SocketError.Success)
            {
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.ServerSendTotalBytesStringFormat, e.BytesTransferred.ToString()));

                this.OnDataSent(token);
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
                lock (((ICollection)this._tokens).SyncRoot)
                {
                    if (this._tokens.ContainsValue(token))
                    {
                        this._tokens.Remove(token.ConnectionId);
                        this.CloseClientSocket(token);

                        if (null != token)
                        {
                            this.OnDisconnected(token);
                        }
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
            catch (Exception ex)
            {
                token.Socket.Close();
                Debug.WriteLine(string.Format(AsyncSocketServerConstants.DebugStringFormat, ex.Message));
                throw;
            }
            finally
            {
                Interlocked.Decrement(ref _numConnectedSockets);
                this._maxNumberAcceptedClients.Release();

                Debug.WriteLine(string.Format(AsyncSocketServerConstants.ClientConnectionStringFormat, _numConnectedSockets.ToString()));

                lock (_readPool)
                {
                    _readPool.Push(token.ReadEventArgs);
                }
            }
        }
    }
}
