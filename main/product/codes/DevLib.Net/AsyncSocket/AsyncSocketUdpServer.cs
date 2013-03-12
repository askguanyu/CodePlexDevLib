//-----------------------------------------------------------------------
// <copyright file="AsyncSocketUdpServer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.AsyncSocket
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// Async Socket Udp Server Class.
    /// </summary>
    public class AsyncSocketUdpServer : MarshalByRefObject, IDisposable
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
        /// Counter of the total bytes received by AsyncSocketUdpServer.
        /// </summary>
        private long _totalBytesReceived;

        /// <summary>
        /// Field _receiveSocketAsyncEventArgs.
        /// </summary>
        private SocketAsyncEventArgs _receiveSocketAsyncEventArgs;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSocketUdpServer" /> class.
        /// </summary>
        public AsyncSocketUdpServer()
            : this(IPEndPoint.MinPort, 8192, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSocketUdpServer" /> class.
        /// </summary>
        /// <param name="port">Local port to listen.</param>
        /// <param name="bufferSize">Buffer size to use with receive data.</param>
        /// <param name="useIPv6">if set to <c>true</c> use IPv6; otherwise, use IPv4.</param>
        public AsyncSocketUdpServer(int port, int bufferSize = 8192, bool useIPv6 = false)
        {
            this.LocalPort = port;
            this.BufferSize = bufferSize;
            this.UseIPv6 = useIPv6;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AsyncSocketUdpServer" /> class.
        /// </summary>
        ~AsyncSocketUdpServer()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Server Data Received Event.
        /// </summary>
        public event EventHandler<AsyncSocketSessionEventArgs> DataReceived;

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
        /// Gets current session Id.
        /// </summary>
        public int SessionId { get; private set; }

        /// <summary>
        /// Gets total bytes received by AsyncSocketUdpServer.
        /// </summary>
        public long TotalBytesReceived
        {
            get
            {
                return Interlocked.Read(ref this._totalBytesReceived);
            }
        }

        /// <summary>
        /// Gets a value indicating whether AsyncSocketUdpServer is working or not.
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
        /// Start AsyncSocketUdpServer.
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

                    this.CloseListenSocket();
                    this.CloseReceiveSocketAsyncEventArgs();

                    this._listenSocket = new Socket(localIPEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                    this._listenSocket.Bind(localIPEndPoint);

                    this.SessionId = this._listenSocket.GetHashCode();

                    this._receiveSocketAsyncEventArgs = new SocketAsyncEventArgs();
                    this._receiveSocketAsyncEventArgs.SetBuffer(new byte[this.BufferSize], 0, this.BufferSize);
                    this._receiveSocketAsyncEventArgs.Completed += this.ReceiveSocketAsyncEventArgsCompleted;

                    try
                    {
                        bool willRaiseEvent = this._listenSocket.ReceiveAsync(this._receiveSocketAsyncEventArgs);
                        if (!willRaiseEvent)
                        {
                            this.ProcessReceive(this._receiveSocketAsyncEventArgs);
                        }

                        Debug.WriteLine(AsyncSocketUdpServerConstants.UdpServerStartSucceeded);

                        this.RaiseEvent(this.Started);

                        return true;
                    }
                    catch
                    {
                        throw;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(AsyncSocketUdpServerConstants.UdpServerStartException);

                    ExceptionHandler.Log(e);

                    this.RaiseEvent(
                                    this.ErrorOccurred,
                                    new AsyncSocketErrorEventArgs(
                                                                  AsyncSocketUdpServerConstants.UdpServerStartException,
                                                                  e,
                                                                  AsyncSocketErrorCodeEnum.UdpServerStartException));

                    if (throwOnError)
                    {
                        throw;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Stop AsyncSocketUdpServer.
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
                    this.CloseListenSocket();
                    this.CloseReceiveSocketAsyncEventArgs();

                    Debug.WriteLine(AsyncSocketUdpServerConstants.UdpServerStopSucceeded);

                    this.RaiseEvent(this.Stopped);

                    return true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(AsyncSocketUdpServerConstants.UdpServerStopException);

                    ExceptionHandler.Log(e);

                    this.RaiseEvent(
                                    this.ErrorOccurred,
                                    new AsyncSocketErrorEventArgs(
                                                                  AsyncSocketUdpServerConstants.UdpServerStopException,
                                                                  e,
                                                                  AsyncSocketErrorCodeEnum.UdpServerStopException));

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
        /// Releases all resources used by the current instance of the <see cref="AsyncSocketUdpServer" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AsyncSocketUdpServer" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AsyncSocketUdpServer" /> class.
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
                    this.CloseListenSocket();
                    this.CloseReceiveSocketAsyncEventArgs();
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                }
                finally
                {
                    this._receiveSocketAsyncEventArgs = null;
                    this._listenSocket = null;
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
                try
                {
                    Interlocked.Add(ref this._totalBytesReceived, receiveSocketAsyncEventArgs.BytesTransferred);

                    this.RaiseEvent(
                                    this.DataReceived,
                                    new AsyncSocketSessionEventArgs(
                                                                    this.SessionId,
                                                                    this._listenSocket.LocalEndPoint as IPEndPoint,
                                                                    receiveSocketAsyncEventArgs.Buffer,
                                                                    receiveSocketAsyncEventArgs.BytesTransferred,
                                                                    receiveSocketAsyncEventArgs.Offset));
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);

                    this.RaiseEvent(
                                    this.ErrorOccurred,
                                    new AsyncSocketErrorEventArgs(
                                                                  AsyncSocketUdpServerConstants.UdpServerReceiveException,
                                                                  e,
                                                                  AsyncSocketErrorCodeEnum.UdpServerReceiveException));
                }
            }

            try
            {
                bool willRaiseEvent = this._listenSocket.ReceiveAsync(receiveSocketAsyncEventArgs);
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
                                                              AsyncSocketUdpServerConstants.UdpServerReceiveException,
                                                              e,
                                                              AsyncSocketErrorCodeEnum.UdpServerReceiveException));
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
        /// Method CloseReceiveSocketAsyncEventArgs.
        /// </summary>
        private void CloseReceiveSocketAsyncEventArgs()
        {
            if (this._receiveSocketAsyncEventArgs != null)
            {
                this._receiveSocketAsyncEventArgs.Completed -= this.ReceiveSocketAsyncEventArgsCompleted;

                Socket sessionSocket = this._receiveSocketAsyncEventArgs.UserToken as Socket;

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

                if (this._receiveSocketAsyncEventArgs.AcceptSocket != null)
                {
                    try
                    {
                        this._receiveSocketAsyncEventArgs.AcceptSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        this._receiveSocketAsyncEventArgs.AcceptSocket.Close();
                        this._receiveSocketAsyncEventArgs.AcceptSocket = null;
                    }
                }

                this._receiveSocketAsyncEventArgs.UserToken = null;
                this._receiveSocketAsyncEventArgs.Dispose();
                this._receiveSocketAsyncEventArgs = null;
            }
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.Net.AsyncSocket.AsyncSocketUdpServer");
            }
        }
    }
}
