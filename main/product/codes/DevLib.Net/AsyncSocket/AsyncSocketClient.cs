//-----------------------------------------------------------------------
// <copyright file="AsyncSocketClient.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.AsyncSocket
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// AsyncSocket Client
    /// </summary>
    public class AsyncSocketClient : IDisposable
    {
        /// <summary>
        ///
        /// </summary>
        private int _bufferSize;

        /// <summary>
        ///
        /// </summary>
        private Socket _clientSocket = null;

        /// <summary>
        ///
        /// </summary>
        private AsyncSocketUserTokenEventArgs _token;

        /// <summary>
        ///
        /// </summary>
        private byte[] _dataBuffer;

        /// <summary>
        /// Constructor of AsyncSocketClient
        /// </summary>
        /// <param name="bufferSize">Buffer size of data to be sent</param>
        public AsyncSocketClient(int bufferSize = AsyncSocketClientConstants.BufferSize)
        {
            this._bufferSize = bufferSize;
            this._dataBuffer = new byte[this._bufferSize];
        }

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<AsyncSocketUserTokenEventArgs> Connected;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<AsyncSocketUserTokenEventArgs> Disconnected;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<AsyncSocketErrorEventArgs> ErrorOccurred;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<AsyncSocketUserTokenEventArgs> DataReceived;

        /// <summary>
        /// Client Data Sent Event
        /// </summary>
        public event EventHandler<AsyncSocketUserTokenEventArgs> DataSent;

        /// <summary>
        /// Connect to remote endpoint
        /// </summary>
        /// <param name="remoteEndPoint">Remote IPEndPoint</param>
        /// <param name="useIOCP">Specifies whether the socket should only use Overlapped I/O mode.</param>
        public void Connect(IPEndPoint remoteEndPoint, bool useIOCP = true)
        {
            this._clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                this._token = new AsyncSocketUserTokenEventArgs(this._clientSocket);
                this._token.ConnectionId = Guid.NewGuid();
                this._token.SetBuffer(this._token.ReadEventArgs.Buffer, this._token.ReadEventArgs.Offset);

                this._clientSocket.UseOnlyOverlappedIO = useIOCP;
                this._clientSocket.BeginConnect(remoteEndPoint, new AsyncCallback(this.ProcessConnect), this._clientSocket);

                Debug.WriteLine(AsyncSocketClientConstants.ClientConnectSuccessfully);
            }
            catch (ObjectDisposedException e)
            {
                Debug.WriteLine(string.Format(AsyncSocketClientConstants.ClientConnectExceptionStringFormat, e.Message));
                this.OnDisconnected(new AsyncSocketUserTokenEventArgs(this._clientSocket));
            }
            catch (SocketException e)
            {
                Debug.WriteLine(string.Format(AsyncSocketClientConstants.ClientConnectExceptionStringFormat, e.Message));

                if (e.ErrorCode == (int)SocketError.ConnectionReset)
                {
                    this.OnDisconnected(this._token);
                }

                Debug.WriteLine(string.Format(AsyncSocketClientConstants.DebugStringFormat, e.Message));
                this.OnErrorOccurred(this._token.Socket, new AsyncSocketErrorEventArgs(e.Message, e, AsyncSocketErrorCodeEnum.ClientConnectException));
            }
        }

        /// <summary>
        /// Connect to remote endpoint
        /// </summary>
        /// <param name="ip">Remote IP</param>
        /// <param name="port">Remote port</param>
        /// <param name="useIOCP">Specifies whether the socket should only use Overlapped I/O mode.</param>
        public void Connect(string ip, int port, bool useIOCP = true)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            this.Connect(remoteEndPoint, useIOCP);
        }

        /// <summary>
        /// Send binary data, call Connect method before using this method
        /// </summary>
        /// <param name="data">Data to be sent</param>
        public void Send(byte[] data)
        {
            try
            {
                this._clientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(this.ProcessSendFinished), this._clientSocket);
                this._token.SetBuffer(data, 0);
                this._token.SetBytesReceived(data.Length);
                this.OnDataSent(this._token);
                Debug.WriteLine(string.Format(AsyncSocketClientConstants.ClientSendBytesStringFormat, data.Length));
            }
            catch (ObjectDisposedException e)
            {
                Debug.WriteLine(string.Format(AsyncSocketClientConstants.ClientSendExceptionStringFormat, e.Message));
                this.OnDisconnected(new AsyncSocketUserTokenEventArgs(this._clientSocket));
            }
            catch (SocketException e)
            {
                Debug.WriteLine(string.Format(AsyncSocketClientConstants.ClientSendExceptionStringFormat, e.Message));

                if (e.ErrorCode == (int)SocketError.ConnectionReset)
                {
                    this.OnDisconnected(this._token);
                }

                Debug.WriteLine(string.Format(AsyncSocketClientConstants.DebugStringFormat, e.Message));
                this.OnErrorOccurred(this._token.Socket, new AsyncSocketErrorEventArgs(e.Message, e, AsyncSocketErrorCodeEnum.ClientSendException));
            }
        }

        /// <summary>
        /// Send strings, call Connect method before using this method
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <param name="encoding">Character encoding</param>
        public void Send(string message, Encoding encoding)
        {
            byte[] data = encoding.GetBytes(message);
            this.Send(data);
        }

        /// <summary>
        /// Send binary data once
        /// </summary>
        /// <param name="remoteEndPoint">Remote endpoint</param>
        /// <param name="data">Data to be sent</param>
        public void SendOnce(IPEndPoint remoteEndPoint, byte[] data)
        {
            this.Connect(remoteEndPoint);
            this.Send(data);
            this.Disconnect();
        }

        /// <summary>
        /// Send strings once
        /// </summary>
        /// <param name="remoteEndPoint">Remote endpoint</param>
        /// <param name="message">Message to be sent</param>
        /// <param name="encoding">Character encoding</param>
        public void SendOnce(IPEndPoint remoteEndPoint, string message, Encoding encoding)
        {
            this.Connect(remoteEndPoint);
            this.Send(message, encoding);
            this.Disconnect();
        }

        /// <summary>
        /// Send binary data once
        /// </summary>
        /// <param name="ip">Remote IP</param>
        /// <param name="port">Remote port</param>
        /// <param name="data">Data to be sent</param>
        public void SendOnce(string ip, int port, byte[] data)
        {
            this.Connect(ip, port);
            this.Send(data);
            this.Disconnect();
        }

        /// <summary>
        /// Send strings once
        /// </summary>
        /// <param name="ip">Remote IP</param>
        /// <param name="port">Remote port</param>
        /// <param name="message">Message to be sent</param>
        /// <param name="encoding">Character encoding</param>
        public void SendOnce(string ip, int port, string message, Encoding encoding)
        {
            this.Connect(ip, port);
            this.Send(message, encoding);
            this.Disconnect();
        }

        /// <summary>
        /// Disconnect client
        /// </summary>
        public void Disconnect()
        {
            try
            {
                this._clientSocket.Shutdown(SocketShutdown.Both);
                this._clientSocket.Close();
                this.OnDisconnected(this._token);
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format(AsyncSocketClientConstants.ClientDisconnectExceptionStringFormat, e.Message));

                this.OnErrorOccurred(this._token.Socket, new AsyncSocketErrorEventArgs(e.Message, e, AsyncSocketErrorCodeEnum.ClientDisconnectException));
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
        /// <param name="e"></param>
        protected virtual void OnConnected(AsyncSocketUserTokenEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<AsyncSocketUserTokenEventArgs> temp = Interlocked.CompareExchange(ref Connected, null, null);

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

            if ((temp != null) && (e.Socket != null))
            {
                temp(this, e);
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

            if (temp != null)
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
                if (this._clientSocket != null)
                {
                    this._clientSocket.Shutdown(SocketShutdown.Both);
                    this._clientSocket.Close();
                    this._clientSocket.Dispose();
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ProcessConnect(IAsyncResult asyncResult)
        {
            Socket asyncState = (Socket)asyncResult.AsyncState;
            try
            {
                asyncState.EndConnect(asyncResult);
                this.OnConnected(this._token);
                this.ProcessWaitForData(asyncState);
            }
            catch (ObjectDisposedException)
            {
                this.OnDisconnected(new AsyncSocketUserTokenEventArgs(this._clientSocket));
            }
            catch (SocketException e)
            {
                this.HandleSocketException(e);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="socket"></param>
        private void ProcessWaitForData(Socket socket)
        {
            try
            {
                socket.BeginReceive(this._dataBuffer, 0, this._bufferSize, SocketFlags.None, new AsyncCallback(this.ProcessIncomingData), socket);
            }
            catch (ObjectDisposedException)
            {
                this.OnDisconnected(this._token);
            }
            catch (SocketException e)
            {
                this.HandleSocketException(e);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ProcessIncomingData(IAsyncResult asyncResult)
        {
            Socket asyncState = (Socket)asyncResult.AsyncState;
            try
            {
                int length = asyncState.EndReceive(asyncResult);
                if (0 == length)
                {
                    this.OnDisconnected(this._token);
                }
                else
                {
                    this._token.SetBuffer(this._dataBuffer, 0);
                    this._token.SetBytesReceived(length);

                    this.OnDataReceived(this._token);
                    this.ProcessWaitForData(asyncState);
                }
            }
            catch (ObjectDisposedException)
            {
                this.OnDisconnected(this._token);
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == (int)SocketError.ConnectionReset)
                {
                    this.OnDisconnected(this._token);
                }

                Debug.WriteLine(string.Format(AsyncSocketClientConstants.DebugStringFormat, e.Message));
                this.OnErrorOccurred(this._token.Socket, new AsyncSocketErrorEventArgs(e.Message, e, AsyncSocketErrorCodeEnum.ClientReceiveException));
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ProcessSendFinished(IAsyncResult asyncResult)
        {
            try
            {
                ((Socket)asyncResult.AsyncState).EndSend(asyncResult);
            }
            catch (ObjectDisposedException)
            {
                this.OnDisconnected(this._token);
            }
            catch (SocketException e)
            {
                this.HandleSocketException(e);
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format(AsyncSocketClientConstants.DebugStringFormat, e.Message));
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        private void HandleSocketException(SocketException e)
        {
            if (e.ErrorCode == (int)SocketError.ConnectionReset)
            {
                this.OnDisconnected(this._token);
            }

            Debug.WriteLine(string.Format(AsyncSocketClientConstants.DebugStringFormat, e.Message));
            this.OnErrorOccurred(this._token.Socket, new AsyncSocketErrorEventArgs(e.Message, e));
        }
    }
}
