//-----------------------------------------------------------------------
// <copyright file="AsyncSocketUdpClient.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.AsyncSocket
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// Async Socket Udp Client Class.
    /// </summary>
    public class AsyncSocketUdpClient : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Counter of the total bytes sent by AsyncSocketUdpClient.
        /// </summary>
        private long _totalBytesSent;

        /// <summary>
        /// The socket used to send to server.
        /// </summary>
        private Socket _clientSocket;

        /// <summary>
        /// Field _remoteIPEndPoint.
        /// </summary>
        private IPEndPoint _remoteIPEndPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSocketUdpClient" /> class.
        /// </summary>
        public AsyncSocketUdpClient()
            : this(IPAddress.Loopback.ToString(), IPEndPoint.MinPort)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSocketUdpClient" /> class.
        /// </summary>
        /// <param name="remoteIP">The IP address of the remote host.</param>
        /// <param name="remotePort">The port number of the remote host.</param>
        public AsyncSocketUdpClient(string remoteIP, int remotePort)
        {
            this.RemoteIP = remoteIP;
            this.RemotePort = remotePort;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AsyncSocketUdpClient" /> class.
        /// </summary>
        ~AsyncSocketUdpClient()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Client Data Sent Event.
        /// </summary>
        public event EventHandler<AsyncSocketSessionEventArgs> DataSent;

        /// <summary>
        /// Error Occurred Event.
        /// </summary>
        public event EventHandler<AsyncSocketErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// Client Started Event.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Client Stopped Event.
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Gets total bytes sent by AsyncSocketUdpClient.
        /// </summary>
        public long TotalBytesSent
        {
            get
            {
                return this._totalBytesSent;
            }
        }

        /// <summary>
        /// Gets or sets the IP address of the remote host.
        /// </summary>
        public string RemoteIP
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the port number of the remote host.
        /// </summary>
        public int RemotePort
        {
            get;
            set;
        }

        /// <summary>
        /// Gets current session Id.
        /// </summary>
        public int SessionId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether udp client is working or not.
        /// </summary>
        public bool IsRunning
        {
            get;
            private set;
        }

        /// <summary>
        /// Sends data synchronously to the specified endpoint.
        /// </summary>
        /// <param name="remoteIP">The IP address of the remote host.</param>
        /// <param name="remotePort">The port number of the remote host.</param>
        /// <param name="buffer">Data to send.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public static bool SendTo(string remoteIP, int remotePort, byte[] buffer)
        {
            try
            {
                IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);

                using (Socket clientSocket = new Socket(remoteIPEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
                {
                    clientSocket.SendTo(buffer, remoteIPEndPoint);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(AsyncSocketUdpClientConstants.UdpClientSendException);

                ExceptionHandler.Log(e);
            }

            return false;
        }

        /// <summary>
        /// Establishes a socket to send to a remote host.
        /// </summary>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Start(bool throwOnError = false)
        {
            this.CheckDisposed();

            if (!this.IsRunning)
            {
                try
                {
                    this._remoteIPEndPoint = new IPEndPoint(IPAddress.Parse(this.RemoteIP), this.RemotePort);

                    this.CloseClientSocket();

                    this._clientSocket = new Socket(this._remoteIPEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

                    this.SessionId = this._clientSocket.GetHashCode();

                    Debug.WriteLine(AsyncSocketUdpClientConstants.UdpClientStartSucceeded);

                    this.RaiseEvent(this.Started);

                    this.IsRunning = true;

                    return true;
                }
                catch (Exception e)
                {
                    this.IsRunning = false;

                    Debug.WriteLine(AsyncSocketUdpClientConstants.UdpClientStartException);

                    ExceptionHandler.Log(e);

                    this.RaiseEvent(
                                    this.ErrorOccurred,
                                    new AsyncSocketErrorEventArgs(
                                                                  AsyncSocketUdpClientConstants.UdpClientStartException,
                                                                  e,
                                                                  AsyncSocketErrorCodeEnum.UdpClientStartException));

                    if (throwOnError)
                    {
                        throw;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Closes the socket.
        /// </summary>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Stop(bool throwOnError = false)
        {
            this.CheckDisposed();

            if (this.IsRunning)
            {
                try
                {
                    this.CloseClientSocket();

                    Debug.WriteLine(AsyncSocketUdpClientConstants.UdpClientStopSucceeded);

                    this.RaiseEvent(this.Stopped);

                    return true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(AsyncSocketUdpClientConstants.UdpClientStopException);

                    ExceptionHandler.Log(e);

                    this.RaiseEvent(
                                    this.ErrorOccurred,
                                    new AsyncSocketErrorEventArgs(
                                                                  AsyncSocketUdpClientConstants.UdpClientStopException,
                                                                  e,
                                                                  AsyncSocketErrorCodeEnum.UdpClientStopException));

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

                this.IsRunning = false;
            }

            return false;
        }

        /// <summary>
        /// Sends data asynchronously to a specific remote host.
        /// </summary>
        /// <param name="buffer">Data to send.</param>
        /// <param name="userToken">A user or application object associated with this asynchronous socket operation.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public bool Send(byte[] buffer, object userToken = null)
        {
            this.CheckDisposed();

            if (!this.IsRunning)
            {
                return false;
            }

            if (this._clientSocket != null)
            {
                SocketAsyncEventArgs sendSocketAsyncEventArgs = null;

                try
                {
                    sendSocketAsyncEventArgs = new SocketAsyncEventArgs();
                    sendSocketAsyncEventArgs.UserToken = userToken;
                    sendSocketAsyncEventArgs.RemoteEndPoint = this._remoteIPEndPoint;
                    sendSocketAsyncEventArgs.SetBuffer(buffer, 0, buffer.Length);
                    sendSocketAsyncEventArgs.Completed += this.SendSocketAsyncEventArgsCompleted;

                    try
                    {
                        bool willRaiseEvent = this._clientSocket.SendToAsync(sendSocketAsyncEventArgs);
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
                                                                  AsyncSocketUdpClientConstants.UdpClientSendException,
                                                                  e,
                                                                  AsyncSocketErrorCodeEnum.UdpClientSendException));
                }
            }

            return false;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AsyncSocketUdpClient" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AsyncSocketUdpClient" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AsyncSocketUdpClient" /> class.
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
                    this.CloseClientSocket();
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                }
                finally
                {
                    this._clientSocket = null;
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
                    case SocketAsyncOperation.SendTo:
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
                try
                {
                    Interlocked.Add(ref this._totalBytesSent, sendSocketAsyncEventArgs.BytesTransferred);

                    this.RaiseEvent(
                                    this.DataSent,
                                    new AsyncSocketSessionEventArgs(
                                                                    this.SessionId,
                                                                    sendSocketAsyncEventArgs.RemoteEndPoint as IPEndPoint,
                                                                    sendSocketAsyncEventArgs.Buffer,
                                                                    sendSocketAsyncEventArgs.BytesTransferred,
                                                                    sendSocketAsyncEventArgs.Offset,
                                                                    sendSocketAsyncEventArgs.UserToken));
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
        /// Method CloseClientSocket.
        /// </summary>
        private void CloseClientSocket()
        {
            if (this._clientSocket != null)
            {
                try
                {
                    this._clientSocket.Disconnect(false);
                }
                catch
                {
                }

                try
                {
                    this._clientSocket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                }
                finally
                {
                    this._clientSocket.Close();
                    this._clientSocket = null;
                }
            }
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.Net.AsyncSocket.AsyncSocketUdpClient");
            }
        }
    }
}
