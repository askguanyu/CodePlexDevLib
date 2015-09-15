//-----------------------------------------------------------------------
// <copyright file="Connection.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Hosting.WebHost20
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.Remoting.Lifetime;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;

    /// <summary>
    /// Connection class.
    /// </summary>
    internal sealed class Connection : MarshalByRefObject
    {
        /// <summary>
        /// Field _defaultLocalhostIP.
        /// </summary>
        private static string _defaultLocalhostIP;

        /// <summary>
        /// Field _localServerIP.
        /// </summary>
        private static string _localServerIP;

        /// <summary>
        /// Field _server.
        /// </summary>
        private WebServer _server;

        /// <summary>
        /// Field _socket.
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="socket">The socket.</param>
        internal Connection(WebServer server, Socket socket)
        {
            this._server = server;
            this._socket = socket;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Connection"/> is connected.
        /// </summary>
        internal bool Connected
        {
            get
            {
                return this._socket.Connected;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is local address.
        /// </summary>
        internal bool IsLocal
        {
            get
            {
                string remoteIP = this.RemoteIP;

                return !string.IsNullOrEmpty(remoteIP) && (remoteIP.Equals("127.0.0.1") || remoteIP.Equals("::1") || remoteIP.Equals("::ffff:127.0.0.1") || Connection.LocalServerIP.Equals(remoteIP));
            }
        }

        /// <summary>
        /// Gets the local ip.
        /// </summary>
        internal string LocalIP
        {
            get
            {
                IPEndPoint ipEndPoint = (IPEndPoint)this._socket.LocalEndPoint;

                if (ipEndPoint != null && ipEndPoint.Address != null)
                {
                    return ipEndPoint.Address.ToString();
                }

                return this.DefaultLocalHostIP;
            }
        }

        /// <summary>
        /// Gets the remote ip.
        /// </summary>
        internal string RemoteIP
        {
            get
            {
                IPEndPoint ipEndPoint = (IPEndPoint)this._socket.RemoteEndPoint;

                if (ipEndPoint != null && ipEndPoint.Address != null)
                {
                    return ipEndPoint.Address.ToString();
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the local server ip.
        /// </summary>
        private static string LocalServerIP
        {
            get
            {
                if (Connection._localServerIP == null)
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(Environment.MachineName);
                    IPAddress ipAddress = hostEntry.AddressList[0];
                    Connection._localServerIP = ipAddress.ToString();
                }

                return Connection._localServerIP;
            }
        }

        /// <summary>
        /// Gets the default local host ip.
        /// </summary>
        private string DefaultLocalHostIP
        {
            get
            {
                if (string.IsNullOrEmpty(Connection._defaultLocalhostIP))
                {
                    bool flag = !Socket.SupportsIPv4 && Socket.OSSupportsIPv6;

                    if (flag)
                    {
                        Connection._defaultLocalhostIP = "::1";
                    }
                    else
                    {
                        Connection._defaultLocalhostIP = "127.0.0.1";
                    }
                }

                return Connection._defaultLocalhostIP;
            }
        }

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns>An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease" /> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime" /> property.</returns>
        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.RemotingConfiguration)]
        public override object InitializeLifetimeService()
        {
            ILease lease = (ILease)base.InitializeLifetimeService();

            if (lease.CurrentState == LeaseState.Initial)
            {
                lease.InitialLeaseTime = TimeSpan.FromMinutes(1.0);
                lease.RenewOnCallTime = TimeSpan.FromSeconds(10.0);
                lease.SponsorshipTimeout = TimeSpan.FromSeconds(30.0);
            }

            return lease;
        }

        /// <summary>
        /// Closes current connection.
        /// </summary>
        internal void Close()
        {
            try
            {
                this._socket.Shutdown(SocketShutdown.Both);
                this._socket.Close();
            }
            catch
            {
            }
            finally
            {
                this._socket = null;
                this._server = null;
            }
        }

        /// <summary>
        /// Reads the request bytes.
        /// </summary>
        /// <param name="maxBytes">The maximum bytes.</param>
        /// <returns>Bytes array.</returns>
        internal byte[] ReadRequestBytes(int maxBytes)
        {
            byte[] result;

            try
            {
                if (this.WaitForRequestBytes() == 0)
                {
                    result = null;
                }
                else
                {
                    int num = this._socket.Available;

                    if (num > maxBytes)
                    {
                        num = maxBytes;
                    }

                    int num2 = 0;

                    byte[] array = new byte[num];

                    if (num > 0)
                    {
                        num2 = this._socket.Receive(array, 0, num, SocketFlags.None);
                    }

                    if (num2 < num)
                    {
                        byte[] array2 = new byte[num2];

                        if (num2 > 0)
                        {
                            Buffer.BlockCopy(array, 0, array2, 0, num2);
                        }

                        array = array2;
                    }

                    result = array;
                }
            }
            catch
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Method Write100Continue.
        /// </summary>
        internal void Write100Continue()
        {
            this.WriteEntireResponseFromString(100, null, null, true);
        }

        /// <summary>
        /// Writes the body.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        internal void WriteBody(byte[] data, int offset, int length)
        {
            try
            {
                this._socket.Send(data, offset, length, SocketFlags.None);
            }
            catch (SocketException)
            {
            }
        }

        /// <summary>
        /// Writes the entire response from string.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="extraHeaders">The extra headers.</param>
        /// <param name="body">The body.</param>
        /// <param name="keepAlive">true to keep alive; otherwise, false.</param>
        internal void WriteEntireResponseFromString(int statusCode, string extraHeaders, string body, bool keepAlive)
        {
            try
            {
                int contentLength = (body != null) ? Encoding.UTF8.GetByteCount(body) : 0;

                this._socket.Send(Encoding.UTF8.GetBytes(Connection.MakeResponseHeaders(statusCode, extraHeaders, contentLength, keepAlive) + body));
            }
            catch (SocketException)
            {
            }
            finally
            {
                if (!keepAlive)
                {
                    this.Close();
                }
            }
        }

        /// <summary>
        /// Writes the entire response from file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="keepAlive">true to keep alive; otherwise, false.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        internal void WriteEntireResponseFromFile(string filename, bool keepAlive)
        {
            if (!File.Exists(filename))
            {
                this.WriteErrorAndClose(404);
                return;
            }

            string text = Connection.MakeContentTypeHeader(filename);

            if (text == null)
            {
                this.WriteErrorAndClose(403);
                return;
            }

            bool flag = false;

            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

                int num = (int)fileStream.Length;

                byte[] buffer = new byte[num];

                int num2 = fileStream.Read(buffer, 0, num);

                this._socket.Send(Encoding.UTF8.GetBytes(Connection.MakeResponseHeaders(200, text, num2, keepAlive)));

                this._socket.Send(buffer, 0, num2, SocketFlags.None);

                flag = true;
            }
            catch (SocketException)
            {
            }
            finally
            {
                if (!keepAlive || !flag)
                {
                    this.Close();
                }

                if (fileStream != null)
                {
                    fileStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Writes the error and close.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="message">The message.</param>
        internal void WriteErrorAndClose(int statusCode, string message)
        {
            this.WriteEntireResponseFromString(statusCode, "Content-type:text/html;charset=utf-8\r\n", this.GetErrorResponseBody(statusCode, message), false);
        }

        /// <summary>
        /// Writes the error and close.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        internal void WriteErrorAndClose(int statusCode)
        {
            this.WriteErrorAndClose(statusCode, null);
        }

        /// <summary>
        /// Writes the error with extra headers and keep alive.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="extraHeaders">The extra headers.</param>
        internal void WriteErrorWithExtraHeadersAndKeepAlive(int statusCode, string extraHeaders)
        {
            this.WriteEntireResponseFromString(statusCode, extraHeaders, this.GetErrorResponseBody(statusCode, null), true);
        }

        /// <summary>
        /// Waits for request bytes.
        /// </summary>
        /// <returns>The number of bytes of data received from the network and available to be read.</returns>
        internal int WaitForRequestBytes()
        {
            int result = 0;

            try
            {
                if (this._socket.Available == 0)
                {
                    this._socket.Poll(100000, SelectMode.SelectRead);

                    if (this._socket.Available == 0 && this._socket.Connected)
                    {
                        this._socket.Poll(30000000, SelectMode.SelectRead);
                    }
                }

                result = this._socket.Available;
            }
            catch
            {
            }

            return result;
        }

        /// <summary>
        /// Writes the headers.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="extraHeaders">The extra headers.</param>
        internal void WriteHeaders(int statusCode, string extraHeaders)
        {
            string text = Connection.MakeResponseHeaders(statusCode, extraHeaders, -1, false);

            try
            {
                this._socket.Send(Encoding.UTF8.GetBytes(text));
            }
            catch (SocketException)
            {
            }
        }

        /// <summary>
        /// Makes the response headers.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="moreHeaders">The more headers.</param>
        /// <param name="contentLength">Length of the content.</param>
        /// <param name="keepAlive">true to keep alive; otherwise, false.</param>
        /// <returns>Response header string.</returns>
        private static string MakeResponseHeaders(int statusCode, string moreHeaders, int contentLength, bool keepAlive)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("HTTP/1.1 ");
            stringBuilder.Append(statusCode);
            stringBuilder.Append(" ");
            stringBuilder.Append(HttpWorkerRequest.GetStatusDescription(statusCode));
            stringBuilder.Append("\r\n");
            stringBuilder.Append("Server: ");
            stringBuilder.Append(Constants.VWDName);
            stringBuilder.Append("/");
            stringBuilder.Append(Constants.VersionString);
            stringBuilder.Append("\r\n");
            stringBuilder.Append("Date: ");
            stringBuilder.Append(DateTime.Now.ToUniversalTime().ToString("R", DateTimeFormatInfo.InvariantInfo));
            stringBuilder.Append("\r\n");

            if (contentLength >= 0)
            {
                stringBuilder.Append("Content-Length: ");
                stringBuilder.Append(contentLength);
                stringBuilder.Append("\r\n");
            }

            if (moreHeaders != null)
            {
                stringBuilder.Append(moreHeaders);
            }

            if (!keepAlive)
            {
                stringBuilder.Append("Connection: Close\r\n");
            }

            stringBuilder.Append("\r\n");

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Makes the content type header.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>Content type header string.</returns>
        private static string MakeContentTypeHeader(string filename)
        {
            string text = null;

            string key = new FileInfo(filename).Extension.ToLowerInvariant();

            switch (key)
            {
                case ".bmp":
                    text = "image/bmp";
                    break;

                case ".css":
                    text = "text/css";
                    break;

                case ".gif":
                    text = "image/gif";
                    break;

                case ".ico":
                    text = "image/x-icon";
                    break;

                case ".htm":
                case ".html":
                    text = "text/html";
                    break;

                case ".jpe":
                case ".jpeg":
                case ".jpg":
                    text = "image/jpeg";
                    break;

                case ".js":
                    text = "application/x-javascript";
                    break;
            }

            if (text == null)
            {
                return null;
            }

            return "Content-Type: " + text + "\r\n";
        }

        /// <summary>
        /// Gets the error response body.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="message">The message.</param>
        /// <returns>Error string.</returns>
        private string GetErrorResponseBody(int statusCode, string message)
        {
            string text = Messages.FormatErrorMessageBody(statusCode, this._server.VirtualPath);

            if (!string.IsNullOrEmpty(message))
            {
                text = text + "\r\n<!--\r\n" + message + "\r\n-->";
            }

            return text;
        }
    }
}
