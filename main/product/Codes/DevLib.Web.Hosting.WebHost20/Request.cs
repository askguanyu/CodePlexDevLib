//-----------------------------------------------------------------------
// <copyright file="Request.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Hosting.WebHost20
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Lifetime;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;
    using System.Web.Hosting;
    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// Request class.
    /// </summary>
    internal sealed class Request : SimpleWorkerRequest
    {
        /// <summary>
        /// Field MaxChunkLength.
        /// </summary>
        private const int MaxChunkLength = 65536;

        /// <summary>
        /// Field MaxHeaderBytes.
        /// </summary>
        private const int MaxHeaderBytes = 32768;

        /// <summary>
        /// Field BadPathChars.
        /// </summary>
        private static readonly char[] BadPathChars = new char[]
        {
            '%',
            '>',
            '<',
            ':',
            '\\'
        };

        /// <summary>
        /// Field DefaultFileNames.
        /// </summary>
        private static readonly string[] DefaultFileNames = new string[]
        {
            "default.aspx",
            "default.asmx",
            "default.htm",
            "default.html"
        };

        /// <summary>
        /// Field RestrictedDirs.
        /// </summary>
        private static readonly string[] RestrictedDirs = new string[]
        {
            "/bin",
            "/app_browsers",
            "/app_code",
            "/app_data",
            "/app_localresources",
            "/app_globalresources",
            "/app_webreferences"
        };

        /// <summary>
        /// Field IntToHex.
        /// </summary>
        private static readonly char[] IntToHex = new char[]
        {
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9',
            'a',
            'b',
            'c',
            'd',
            'e',
            'f'
        };

        /// <summary>
        /// Field _host.
        /// </summary>
        private Host _host;

        /// <summary>
        /// Field _connection.
        /// </summary>
        private Connection _connection;

        /// <summary>
        /// Field _connectionSponsor.
        /// </summary>
        private Sponsor _connectionSponsor;

        /// <summary>
        /// Field _connectionPermission.
        /// </summary>
        private IStackWalk _connectionPermission = new PermissionSet(PermissionState.Unrestricted);

        /// <summary>
        /// Field _headerBytes.
        /// </summary>
        private byte[] _headerBytes;

        /// <summary>
        /// Field _startHeadersOffset.
        /// </summary>
        private int _startHeadersOffset;

        /// <summary>
        /// Field _endHeadersOffset.
        /// </summary>
        private int _endHeadersOffset;

        /// <summary>
        /// Field _headerByteStrings.
        /// </summary>
        private ArrayList _headerByteStrings;

        /// <summary>
        /// Field _isClientScriptPath.
        /// </summary>
        private bool _isClientScriptPath;

        /// <summary>
        /// Field _verb.
        /// </summary>
        private string _verb;

        /// <summary>
        /// Field _url.
        /// </summary>
        private string _url;

        /// <summary>
        /// Field _port.
        /// </summary>
        private string _port;

        /// <summary>
        /// Field _path.
        /// </summary>
        private string _path;

        /// <summary>
        /// Field _filePath.
        /// </summary>
        private string _filePath;

        /// <summary>
        /// Field _pathInfo.
        /// </summary>
        private string _pathInfo;

        /// <summary>
        /// Field _pathTranslated.
        /// </summary>
        private string _pathTranslated;

        /// <summary>
        /// Field _queryString.
        /// </summary>
        private string _queryString;

        /// <summary>
        /// Field _queryStringBytes.
        /// </summary>
        private byte[] _queryStringBytes;

        /// <summary>
        /// Field _contentLength.
        /// </summary>
        private int _contentLength;

        /// <summary>
        /// Field _preloadedContentLength.
        /// </summary>
        private int _preloadedContentLength;

        /// <summary>
        /// Field _preloadedContent.
        /// </summary>
        private byte[] _preloadedContent;

        /// <summary>
        /// Field _allRawHeaders.
        /// </summary>
        private string _allRawHeaders;

        /// <summary>
        /// Field _unknownRequestHeaders.
        /// </summary>
        private string[][] _unknownRequestHeaders;

        /// <summary>
        /// Field _knownRequestHeaders.
        /// </summary>
        private string[] _knownRequestHeaders;

        /// <summary>
        /// Field _specialCaseStaticFileHeaders.
        /// </summary>
        private bool _specialCaseStaticFileHeaders;

        /// <summary>
        /// Field _headersSent.
        /// </summary>
        private bool _headersSent;

        /// <summary>
        /// Field _responseStatus.
        /// </summary>
        private int _responseStatus;

        /// <summary>
        /// Field _responseHeadersBuilder.
        /// </summary>
        private StringBuilder _responseHeadersBuilder;

        /// <summary>
        /// Field _responseBodyBytes.
        /// </summary>
        private ArrayList _responseBodyBytes;

        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="connection">The connection.</param>
        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.RemotingConfiguration)]
        public Request(Host host, Connection connection)
            : base(string.Empty, string.Empty, null)
        {
            this._host = host;
            this._connection = connection;
            ILease lease = (ILease)RemotingServices.GetLifetimeService(this._connection);
            this._connectionSponsor = new Sponsor();
            lease.Register(this._connectionSponsor);
        }

        /// <summary>
        /// Processes request.
        /// </summary>
        [AspNetHostingPermission(SecurityAction.Assert, Level = AspNetHostingPermissionLevel.Unrestricted, Unrestricted = true)]
        public void Process()
        {
            if (!this.TryParseRequest())
            {
                return;
            }

            if (this._verb == "POST" && this._contentLength > 0 && this._preloadedContentLength < this._contentLength)
            {
                this._connection.Write100Continue();
            }

            if (this._host.RequireAuthentication && !this.TryNtlmAuthenticate())
            {
                return;
            }

            if (this._isClientScriptPath)
            {
                this._connection.WriteEntireResponseFromFile(this._host.PhysicalClientScriptPath + this._path.Substring(this._host.NormalizedClientScriptPath.Length), false);
                return;
            }

            if (this.IsRequestForRestrictedDirectory())
            {
                this._connection.WriteErrorAndClose(403);
                return;
            }

            if (this.ProcessDefaultDocumentRequest())
            {
                return;
            }

            this.PrepareResponse();

            HttpRuntime.ProcessRequest(this);
        }

        /// <summary>
        /// Returns the virtual path to the requested URI.
        /// </summary>
        /// <returns>The path to the requested URI.</returns>
        public override string GetUriPath()
        {
            return this._path;
        }

        /// <summary>
        /// Returns the query string specified in the request URL.
        /// </summary>
        /// <returns>The request query string.</returns>
        public override string GetQueryString()
        {
            return this._queryString;
        }

        /// <summary>
        /// When overridden in a derived class, returns the response query string as an array of bytes.
        /// </summary>
        /// <returns>An array of bytes containing the response.</returns>
        public override byte[] GetQueryStringRawBytes()
        {
            return this._queryStringBytes;
        }

        /// <summary>
        /// Returns the URL path contained in the header with the query string appended.
        /// </summary>
        /// <returns>The raw URL path of the request header.Note:The returned URL is not normalized. Using the URL for access control, or security-sensitive decisions can expose your application to canonicalization security vulnerabilities.</returns>
        public override string GetRawUrl()
        {
            return this._url;
        }

        /// <summary>
        /// Returns the HTTP request verb.
        /// </summary>
        /// <returns>The HTTP verb for this request.</returns>
        public override string GetHttpVerbName()
        {
            return this._verb;
        }

        /// <summary>
        /// Returns the HTTP version string of the request (for example, "HTTP/1.1").
        /// </summary>
        /// <returns>The HTTP version string returned in the request header.</returns>
        public override string GetHttpVersion()
        {
            return this._port;
        }

        /// <summary>
        /// Returns the IP address of the client.
        /// </summary>
        /// <returns>The client's IP address.</returns>
        public override string GetRemoteAddress()
        {
            this._connectionPermission.Assert();

            return this._connection.RemoteIP;
        }

        /// <summary>
        /// Returns the client's port number.
        /// </summary>
        /// <returns>The client's port number.</returns>
        public override int GetRemotePort()
        {
            return 0;
        }

        /// <summary>
        /// Returns the server IP address of the interface on which the request was received.
        /// </summary>
        /// <returns>The server IP address of the interface on which the request was received.</returns>
        public override string GetLocalAddress()
        {
            this._connectionPermission.Assert();

            return this._connection.LocalIP;
        }

        /// <summary>
        /// When overridden in a derived class, returns the name of the local server.
        /// </summary>
        /// <returns>The name of the local server.</returns>
        public override string GetServerName()
        {
            string localAddress = this.GetLocalAddress();

            if (localAddress.Equals("127.0.0.1") || localAddress.Equals("::1") || localAddress.Equals("::ffff:127.0.0.1"))
            {
                return "localhost";
            }

            return localAddress;
        }

        /// <summary>
        /// Returns the port number on which the request was received.
        /// </summary>
        /// <returns>The server port number on which the request was received.</returns>
        public override int GetLocalPort()
        {
            return this._host.Port;
        }

        /// <summary>
        /// Returns the physical path to the requested URI.
        /// </summary>
        /// <returns>The physical path to the requested URI.</returns>
        public override string GetFilePath()
        {
            return this._filePath;
        }

        /// <summary>
        /// Returns the physical file path to the requested URI (and translates it from virtual path to physical path: for example, "/proj1/page.aspx" to "c:\dir\page.aspx")
        /// </summary>
        /// <returns>The translated physical file path to the requested URI.</returns>
        public override string GetFilePathTranslated()
        {
            return this._pathTranslated;
        }

        /// <summary>
        /// Returns additional path information for a resource with a URL extension. That is, for the path /virdir/page.html/tail, the return value is /tail.
        /// </summary>
        /// <returns>Additional path information for a resource.</returns>
        public override string GetPathInfo()
        {
            return this._pathInfo;
        }

        /// <summary>
        /// Returns the virtual path to the currently executing server application.
        /// </summary>
        /// <returns>The virtual path of the current application.</returns>
        public override string GetAppPath()
        {
            return this._host.VirtualPath;
        }

        /// <summary>
        /// Returns the UNC-translated path to the currently executing server application.
        /// </summary>
        /// <returns>The physical path of the current application.</returns>
        public override string GetAppPathTranslated()
        {
            return this._host.PhysicalPath;
        }

        /// <summary>
        /// Returns the portion of the HTTP request body that has already been read.
        /// </summary>
        /// <returns>The portion of the HTTP request body that has been read.</returns>
        public override byte[] GetPreloadedEntityBody()
        {
            return this._preloadedContent;
        }

        /// <summary>
        /// Returns a value indicating whether all request data is available and no further reads from the client are required.
        /// </summary>
        /// <returns>true if all request data is available; otherwise, false.</returns>
        public override bool IsEntireEntityBodyIsPreloaded()
        {
            return this._contentLength == this._preloadedContentLength;
        }

        /// <summary>
        /// Reads request data from the client (when not preloaded).
        /// </summary>
        /// <param name="buffer">The byte array to read data into.</param>
        /// <param name="size">The maximum number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        public override int ReadEntityBody(byte[] buffer, int size)
        {
            int num = 0;
            this._connectionPermission.Assert();
            byte[] array = this._connection.ReadRequestBytes(size);

            if (array != null && array.Length > 0)
            {
                num = array.Length;
                Buffer.BlockCopy(array, 0, buffer, 0, num);
            }

            return num;
        }

        /// <summary>
        /// Returns the standard HTTP request header that corresponds to the specified index.
        /// </summary>
        /// <param name="index">The index of the header. For example, the <see cref="F:System.Web.HttpWorkerRequest.HeaderAllow" /> field.</param>
        /// <returns>The HTTP request header.</returns>
        public override string GetKnownRequestHeader(int index)
        {
            return this._knownRequestHeaders[index];
        }

        /// <summary>
        /// Returns a nonstandard HTTP request header value.
        /// </summary>
        /// <param name="name">The header name.</param>
        /// <returns>The header value.</returns>
        public override string GetUnknownRequestHeader(string name)
        {
            int num = this._unknownRequestHeaders.Length;

            for (int i = 0; i < num; i++)
            {
                if (string.Compare(name, this._unknownRequestHeaders[i][0], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return this._unknownRequestHeaders[i][1];
                }
            }

            return null;
        }

        /// <summary>
        /// Get all nonstandard HTTP header name-value pairs.
        /// </summary>
        /// <returns>An array of header name-value pairs.</returns>
        public override string[][] GetUnknownRequestHeaders()
        {
            return this._unknownRequestHeaders;
        }

        /// <summary>
        /// Returns a single server variable from a dictionary of server variables associated with the request.
        /// </summary>
        /// <param name="name">The name of the requested server variable.</param>
        /// <returns>The requested server variable.</returns>
        public override string GetServerVariable(string name)
        {
            string result = string.Empty;

            if (name != null)
            {
                if (!(name == "ALL_RAW"))
                {
                    if (!(name == "SERVER_PROTOCOL"))
                    {
                        if (!(name == "LOGON_USER"))
                        {
                            if (name == "AUTH_TYPE")
                            {
                                if (this.GetUserToken() != IntPtr.Zero)
                                {
                                    result = "NTLM";
                                }
                            }
                        }
                        else
                        {
                            if (this.GetUserToken() != IntPtr.Zero)
                            {
                                result = this._host.GetProcessUser();
                            }
                        }
                    }
                    else
                    {
                        result = this._port;
                    }
                }
                else
                {
                    result = this._allRawHeaders;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the client's impersonation token.
        /// </summary>
        /// <returns>A value representing the client's impersonation token. The default is <see cref="F:System.IntPtr.Zero" />.</returns>
        public override IntPtr GetUserToken()
        {
            return this._host.GetProcessToken();
        }

        /// <summary>
        /// Returns the physical path corresponding to the specified virtual path.
        /// </summary>
        /// <param name="path">The virtual path.</param>
        /// <returns>The physical path that corresponds to the virtual path specified in the <paramref name="path" /> parameter.</returns>
        public override string MapPath(string path)
        {
            string text = string.Empty;
            bool flag = false;

            if (path == null || path.Length == 0 || path.Equals("/"))
            {
                if (this._host.VirtualPath == "/")
                {
                    text = this._host.PhysicalPath;
                }
                else
                {
                    text = string.Empty;
                }
            }
            else
            {
                if (this._host.IsVirtualPathAppPath(path))
                {
                    text = this._host.PhysicalPath;
                }
                else
                {
                    if (this._host.IsVirtualPathInApp(path, out flag))
                    {
                        if (flag)
                        {
                            text = this._host.PhysicalClientScriptPath + path.Substring(this._host.NormalizedClientScriptPath.Length);
                        }
                        else
                        {
                            text = this._host.PhysicalPath + path.Substring(this._host.NormalizedVirtualPath.Length);
                        }
                    }
                    else
                    {
                        if (path.StartsWith("/", StringComparison.Ordinal))
                        {
                            text = this._host.PhysicalPath + path.Substring(1);
                        }
                        else
                        {
                            text = this._host.PhysicalPath + path;
                        }
                    }
                }
            }

            text = text.Replace('/', '\\');

            if (text.EndsWith("\\", StringComparison.Ordinal) && !text.EndsWith(":\\", StringComparison.Ordinal))
            {
                text = text.Substring(0, text.Length - 1);
            }

            return text;
        }

        /// <summary>
        /// Specifies the HTTP status code and status description of the response; for example, SendStatus(200, "Ok").
        /// </summary>
        /// <param name="statusCode">The status code to send</param>
        /// <param name="statusDescription">The status description to send.</param>
        public override void SendStatus(int statusCode, string statusDescription)
        {
            this._responseStatus = statusCode;
        }

        /// <summary>
        /// Adds a standard HTTP header to the response.
        /// </summary>
        /// <param name="index">The header index. For example, <see cref="F:System.Web.HttpWorkerRequest.HeaderContentLength" />.</param>
        /// <param name="value">The header value.</param>
        public override void SendKnownResponseHeader(int index, string value)
        {
            if (this._headersSent)
            {
                return;
            }

            switch (index)
            {
                case 1:
                case 2:
                    break;

                default:
                    switch (index)
                    {
                        case 18:
                        case 19:
                            if (this._specialCaseStaticFileHeaders)
                            {
                                return;
                            }

                            break;

                        case 20:
                            if (value == "bytes")
                            {
                                this._specialCaseStaticFileHeaders = true;
                                return;
                            }

                            break;

                        default:
                            if (index == 26)
                            {
                                return;
                            }

                            break;
                    }

                    this._responseHeadersBuilder.Append(HttpWorkerRequest.GetKnownResponseHeaderName(index));
                    this._responseHeadersBuilder.Append(": ");
                    this._responseHeadersBuilder.Append(value);
                    this._responseHeadersBuilder.Append("\r\n");

                    return;
            }
        }

        /// <summary>
        /// Adds a nonstandard HTTP header to the response.
        /// </summary>
        /// <param name="name">The name of the header to send.</param>
        /// <param name="value">The value of the header.</param>
        public override void SendUnknownResponseHeader(string name, string value)
        {
            if (this._headersSent)
            {
                return;
            }

            this._responseHeadersBuilder.Append(name);
            this._responseHeadersBuilder.Append(": ");
            this._responseHeadersBuilder.Append(value);
            this._responseHeadersBuilder.Append("\r\n");
        }

        /// <summary>
        /// Adds a Content-Length HTTP header to the response for message bodies that are less than or equal to 2 GB.
        /// </summary>
        /// <param name="contentLength">The length of the response, in bytes.</param>
        public override void SendCalculatedContentLength(int contentLength)
        {
            if (!this._headersSent)
            {
                this._responseHeadersBuilder.Append("Content-Length: ");
                this._responseHeadersBuilder.Append(contentLength.ToString(CultureInfo.InvariantCulture));
                this._responseHeadersBuilder.Append("\r\n");
            }
        }

        /// <summary>
        /// Returns a value indicating whether HTTP response headers have been sent to the client for the current request.
        /// </summary>
        /// <returns>true if HTTP response headers have been sent to the client; otherwise, false.</returns>
        public override bool HeadersSent()
        {
            return this._headersSent;
        }

        /// <summary>
        /// Returns a value indicating whether the client connection is still active.
        /// </summary>
        /// <returns>true if the client connection is still active; otherwise, false.</returns>
        public override bool IsClientConnected()
        {
            this._connectionPermission.Assert();

            return this._connection.Connected;
        }

        /// <summary>
        /// Terminates the connection with the client.
        /// </summary>
        public override void CloseConnection()
        {
            this._connectionPermission.Assert();

            this.CloseConnectionInternal();
        }

        /// <summary>
        /// Adds the contents of a byte array to the response and specifies the number of bytes to send.
        /// </summary>
        /// <param name="data">The byte array to send.</param>
        /// <param name="length">The number of bytes to send.</param>
        public override void SendResponseFromMemory(byte[] data, int length)
        {
            if (length > 0)
            {
                byte[] array = new byte[length];
                Buffer.BlockCopy(data, 0, array, 0, length);
                this._responseBodyBytes.Add(array);
            }
        }

        /// <summary>
        /// Adds the contents of the file with the specified name to the response and specifies the starting position in the file and the number of bytes to send.
        /// </summary>
        /// <param name="filename">The name of the file to send.</param>
        /// <param name="offset">The starting position in the file.</param>
        /// <param name="length">The number of bytes to send.</param>
        public override void SendResponseFromFile(string filename, long offset, long length)
        {
            if (length == 0L)
            {
                return;
            }

            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                this.SendResponseFromFileStream(fileStream, offset, length);
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }
        }

        /// <summary>
        /// Adds the contents of the file with the specified handle to the response and specifies the starting position in the file and the number of bytes to send.
        /// </summary>
        /// <param name="handle">The handle of the file to send.</param>
        /// <param name="offset">The starting position in the file.</param>
        /// <param name="length">The number of bytes to send.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public override void SendResponseFromFile(IntPtr handle, long offset, long length)
        {
            if (length == 0L)
            {
                return;
            }

            FileStream fileStream = null;

            try
            {
                SafeFileHandle handle2 = new SafeFileHandle(handle, false);
                fileStream = new FileStream(handle2, FileAccess.Read);
                this.SendResponseFromFileStream(fileStream, offset, length);
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Dispose();
                    fileStream = null;
                }
            }
        }

        /// <summary>
        /// Sends all pending response data to the client.
        /// </summary>
        /// <param name="finalFlush">true if this is the last time response data will be flushed; otherwise, false.</param>
        public override void FlushResponse(bool finalFlush)
        {
            if (this._responseStatus == 404 && !this._headersSent && finalFlush && this._verb == "GET" && this.ProcessDirectoryListingRequest())
            {
                return;
            }

            this._connectionPermission.Assert();

            if (!this._headersSent)
            {
                this._connection.WriteHeaders(this._responseStatus, this._responseHeadersBuilder.ToString());
                this._headersSent = true;
            }

            for (int i = 0; i < this._responseBodyBytes.Count; i++)
            {
                byte[] array = (byte[])this._responseBodyBytes[i];
                this._connection.WriteBody(array, 0, array.Length);
            }

            this._responseBodyBytes = new ArrayList();

            if (finalFlush)
            {
                this.CloseConnectionInternal();
            }
        }

        /// <summary>
        /// Notifies the <see cref="T:System.Web.HttpWorkerRequest" /> that request processing for the current request is complete.
        /// </summary>
        public override void EndOfRequest()
        {
        }

        /// <summary>
        /// Gets the URL encode redirect.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The URL encode redirect string.</returns>
        private static string GetUrlEncodeRedirect(string path)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(path);

            int num = bytes.Length;

            int num2 = 0;

            for (int i = 0; i < num; i++)
            {
                if ((bytes[i] & 128) != 0)
                {
                    num2++;
                }
            }

            if (num2 > 0)
            {
                byte[] array = new byte[num + (num2 * 2)];

                int num3 = 0;

                for (int j = 0; j < num; j++)
                {
                    byte b = bytes[j];

                    if ((b & 128) == 0)
                    {
                        array[num3++] = b;
                    }
                    else
                    {
                        array[num3++] = 37;
                        array[num3++] = (byte)Request.IntToHex[b >> 4 & 15];
                        array[num3++] = (byte)Request.IntToHex[(int)(b & 15)];
                    }
                }

                path = Encoding.ASCII.GetString(array);
            }

            if (path.IndexOf(' ') >= 0)
            {
                path = path.Replace(" ", "%20");
            }

            return path;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        private void Reset()
        {
            this._headerBytes = null;
            this._startHeadersOffset = 0;
            this._endHeadersOffset = 0;
            this._headerByteStrings = null;
            this._isClientScriptPath = false;
            this._verb = null;
            this._url = null;
            this._port = null;
            this._path = null;
            this._filePath = null;
            this._pathInfo = null;
            this._pathTranslated = null;
            this._queryString = null;
            this._queryStringBytes = null;
            this._contentLength = 0;
            this._preloadedContentLength = 0;
            this._preloadedContent = null;
            this._allRawHeaders = null;
            this._unknownRequestHeaders = null;
            this._knownRequestHeaders = null;
            this._specialCaseStaticFileHeaders = false;
        }

        /// <summary>
        /// Tries to parse request.
        /// </summary>
        /// <returns>true if succeeded; otherwise, false.</returns>
        private bool TryParseRequest()
        {
            this.Reset();

            this.ReadAllHeaders();

            if (this._headerBytes == null || this._endHeadersOffset < 0 || this._headerByteStrings == null || this._headerByteStrings.Count == 0)
            {
                this._connection.WriteErrorAndClose(400);
                return false;
            }

            this.ParseRequestLine();

            if (this.IsBadPath())
            {
                this._connection.WriteErrorAndClose(400);
                return false;
            }

            if (!this._host.IsVirtualPathInApp(this._path, out this._isClientScriptPath))
            {
                this._connection.WriteErrorAndClose(404);
                return false;
            }

            this.ParseHeaders();

            this.ParsePostedContent();

            return true;
        }

        /// <summary>
        /// Tries to check NTLM authenticate.
        /// </summary>
        /// <returns>true if succeeded; otherwise, false.</returns>
        private bool TryNtlmAuthenticate()
        {
            return true;
        }

        /// <summary>
        /// Tries to read all headers.
        /// </summary>
        /// <returns>true if succeeded; otherwise, false.</returns>
        private bool TryReadAllHeaders()
        {
            byte[] array = this._connection.ReadRequestBytes(32768);

            if (array == null || array.Length == 0)
            {
                return false;
            }

            if (this._headerBytes != null)
            {
                int num = array.Length + this._headerBytes.Length;

                if (num > 32768)
                {
                    return false;
                }

                byte[] array2 = new byte[num];

                Buffer.BlockCopy(this._headerBytes, 0, array2, 0, this._headerBytes.Length);

                Buffer.BlockCopy(array, 0, array2, this._headerBytes.Length, array.Length);

                this._headerBytes = array2;
            }
            else
            {
                this._headerBytes = array;
            }

            this._startHeadersOffset = -1;
            this._endHeadersOffset = -1;
            this._headerByteStrings = new ArrayList();
            ByteParser byteParser = new ByteParser(this._headerBytes);

            while (true)
            {
                ByteString byteString = byteParser.ReadLine();

                if (byteString == null)
                {
                    return true;
                }

                if (this._startHeadersOffset < 0)
                {
                    this._startHeadersOffset = byteParser.CurrentOffset;
                }

                if (byteString.IsEmpty)
                {
                    break;
                }

                this._headerByteStrings.Add(byteString);
            }

            this._endHeadersOffset = byteParser.CurrentOffset;

            return true;
        }

        /// <summary>
        /// Reads all headers.
        /// </summary>
        private void ReadAllHeaders()
        {
            this._headerBytes = null;

            while (this.TryReadAllHeaders())
            {
                if (this._endHeadersOffset >= 0)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Parses the request line.
        /// </summary>
        private void ParseRequestLine()
        {
            ByteString byteString = (ByteString)this._headerByteStrings[0];

            ByteString[] array = byteString.Split(' ');

            if (array == null || array.Length < 2 || array.Length > 3)
            {
                this._connection.WriteErrorAndClose(400);
                return;
            }

            this._verb = array[0].GetString();

            ByteString byteString2 = array[1];

            this._url = byteString2.GetString();

            if (this._url.IndexOf('�') >= 0)
            {
                this._url = byteString2.GetString(Encoding.Default);
            }

            if (array.Length == 3)
            {
                this._port = array[2].GetString();
            }
            else
            {
                this._port = "HTTP/1.0";
            }

            int num = byteString2.IndexOf('?');

            if (num > 0)
            {
                this._queryStringBytes = byteString2.Substring(num + 1).GetBytes();
            }
            else
            {
                this._queryStringBytes = new byte[0];
            }

            num = this._url.IndexOf('?');

            if (num > 0)
            {
                this._path = this._url.Substring(0, num);
                this._queryString = this._url.Substring(num + 1);
            }
            else
            {
                this._path = this._url;
                this._queryString = string.Empty;
            }

            if (this._path.IndexOf('%') >= 0)
            {
                this._path = HttpUtility.UrlDecode(this._path, Encoding.UTF8);

                num = this._url.IndexOf('?');

                if (num >= 0)
                {
                    this._url = this._path + this._url.Substring(num);
                }
                else
                {
                    this._url = this._path;
                }
            }

            int num2 = this._path.LastIndexOf('.');

            int num3 = this._path.LastIndexOf('/');

            if (num2 >= 0 && num3 >= 0 && num2 < num3)
            {
                int num4 = this._path.IndexOf('/', num2);

                this._filePath = this._path.Substring(0, num4);

                this._pathInfo = this._path.Substring(num4);
            }
            else
            {
                this._filePath = this._path;

                this._pathInfo = string.Empty;
            }

            this._pathTranslated = this.MapPath(this._filePath);
        }

        /// <summary>
        /// Determines whether the path is bad path.
        /// </summary>
        /// <returns>true if the path is bad path; otherwise, false.</returns>
        private bool IsBadPath()
        {
            return this._path.IndexOfAny(Request.BadPathChars) >= 0 || CultureInfo.InvariantCulture.CompareInfo.IndexOf(this._path, "..", CompareOptions.Ordinal) >= 0 || CultureInfo.InvariantCulture.CompareInfo.IndexOf(this._path, "//", CompareOptions.Ordinal) >= 0;
        }

        /// <summary>
        /// Parses the headers.
        /// </summary>
        private void ParseHeaders()
        {
            this._knownRequestHeaders = new string[40];

            ArrayList arrayList = new ArrayList();

            for (int i = 1; i < this._headerByteStrings.Count; i++)
            {
                string string1 = ((ByteString)this._headerByteStrings[i]).GetString();

                int num = string1.IndexOf(':');

                if (num >= 0)
                {
                    string text = string1.Substring(0, num).Trim();

                    string text2 = string1.Substring(num + 1).Trim();

                    int knownRequestHeaderIndex = HttpWorkerRequest.GetKnownRequestHeaderIndex(text);

                    if (knownRequestHeaderIndex >= 0)
                    {
                        this._knownRequestHeaders[knownRequestHeaderIndex] = text2;
                    }
                    else
                    {
                        arrayList.Add(text);
                        arrayList.Add(text2);
                    }
                }
            }

            int num2 = arrayList.Count / 2;

            this._unknownRequestHeaders = new string[num2][];

            int num3 = 0;

            for (int j = 0; j < num2; j++)
            {
                this._unknownRequestHeaders[j] = new string[2];
                this._unknownRequestHeaders[j][0] = (string)arrayList[num3++];
                this._unknownRequestHeaders[j][1] = (string)arrayList[num3++];
            }

            if (this._headerByteStrings.Count > 1)
            {
                this._allRawHeaders = Encoding.UTF8.GetString(this._headerBytes, this._startHeadersOffset, this._endHeadersOffset - this._startHeadersOffset);
                return;
            }

            this._allRawHeaders = string.Empty;
        }

        /// <summary>
        /// Parses the posted content.
        /// </summary>
        private void ParsePostedContent()
        {
            this._contentLength = 0;

            this._preloadedContentLength = 0;

            string text = this._knownRequestHeaders[11];

            if (text != null)
            {
                try
                {
                    this._contentLength = int.Parse(text, CultureInfo.InvariantCulture);
                }
                catch
                {
                }
            }

            if (this._headerBytes.Length > this._endHeadersOffset)
            {
                this._preloadedContentLength = this._headerBytes.Length - this._endHeadersOffset;

                if (this._preloadedContentLength > this._contentLength)
                {
                    this._preloadedContentLength = this._contentLength;
                }

                if (this._preloadedContentLength > 0)
                {
                    this._preloadedContent = new byte[this._preloadedContentLength];
                    Buffer.BlockCopy(this._headerBytes, this._endHeadersOffset, this._preloadedContent, 0, this._preloadedContentLength);
                }
            }
        }

        /// <summary>
        /// Skips the all posted content.
        /// </summary>
        private void SkipAllPostedContent()
        {
            if (this._contentLength > 0 && this._preloadedContentLength < this._contentLength)
            {
                byte[] array;

                for (int i = this._contentLength - this._preloadedContentLength; i > 0; i -= array.Length)
                {
                    array = this._connection.ReadRequestBytes(i);

                    if (array == null || array.Length == 0)
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the request is for restricted directory.
        /// </summary>
        /// <returns>true if the request is for restricted directory; otherwise, false.</returns>
        private bool IsRequestForRestrictedDirectory()
        {
            string text = CultureInfo.InvariantCulture.TextInfo.ToLower(this._path);

            if (this._host.VirtualPath != "/")
            {
                text = text.Substring(this._host.VirtualPath.Length);
            }

            string[] array = Request.RestrictedDirs;

            for (int i = 0; i < array.Length; i++)
            {
                string text2 = array[i];

                if (text.StartsWith(text2, StringComparison.Ordinal) && (text.Length == text2.Length || text[text2.Length] == '/'))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Processes the default document request.
        /// </summary>
        /// <returns>true if succeeded; otherwise, false.</returns>
        private bool ProcessDefaultDocumentRequest()
        {
            if (this._verb != "GET")
            {
                return false;
            }

            string text = this._pathTranslated;

            if (this._pathInfo.Length > 0)
            {
                text = this.MapPath(this._path);
            }

            if (!Directory.Exists(text))
            {
                return false;
            }

            if (!this._path.EndsWith("/", StringComparison.Ordinal))
            {
                string text2 = this._path + "/";
                string extraHeaders = "Location: " + Request.GetUrlEncodeRedirect(text2) + "\r\n";
                string body = "<html><head><title>Object moved</title></head><body>\r\n<h2>Object moved to <a href='" + text2 + "'>here</a>.</h2>\r\n</body></html>\r\n";
                this._connection.WriteEntireResponseFromString(302, extraHeaders, body, false);

                return true;
            }

            string[] array = Request.DefaultFileNames;

            for (int i = 0; i < array.Length; i++)
            {
                string text3 = array[i];
                string text4 = text + "\\" + text3;

                if (File.Exists(text4))
                {
                    this._path += text3;
                    this._filePath = this._path;
                    this._url = (this._queryString != null) ? (this._path + "?" + this._queryString) : this._path;
                    this._pathTranslated = text4;

                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Processes the directory listing request.
        /// </summary>
        /// <returns>true if succeeded; otherwise, false.</returns>
        private bool ProcessDirectoryListingRequest()
        {
            if (this._verb != "GET")
            {
                return false;
            }

            string path = this._pathTranslated;

            if (this._pathInfo.Length > 0)
            {
                path = this.MapPath(this._path);
            }

            if (!Directory.Exists(path))
            {
                return false;
            }

            if (this._host.DisableDirectoryListing)
            {
                return false;
            }

            FileSystemInfo[] elements = null;

            try
            {
                elements = new DirectoryInfo(path).GetFileSystemInfos();
            }
            catch
            {
            }

            string text = null;

            if (this._path.Length > 1)
            {
                int num = this._path.LastIndexOf('/', this._path.Length - 2);
                text = (num > 0) ? this._path.Substring(0, num) : "/";

                if (!this._host.IsVirtualPathInApp(text))
                {
                    text = null;
                }
            }

            this._connection.WriteEntireResponseFromString(200, "Content-type: text/html; charset=utf-8\r\n", Messages.FormatDirectoryListing(this._path, text, elements), false);

            return true;
        }

        /// <summary>
        /// Prepares the response.
        /// </summary>
        private void PrepareResponse()
        {
            this._headersSent = false;
            this._responseStatus = 200;
            this._responseHeadersBuilder = new StringBuilder();
            this._responseBodyBytes = new ArrayList();
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        private void CloseConnectionInternal()
        {
            if (this._connection != null)
            {
                this._connection.Close();
                ILease lease = (ILease)RemotingServices.GetLifetimeService(this._connection);
                lease.Unregister(this._connectionSponsor);
                this._connection = null;
                this._connectionSponsor = null;
            }
        }

        /// <summary>
        /// Sends the response from file stream.
        /// </summary>
        /// <param name="fileStream">The file stream.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        private void SendResponseFromFileStream(FileStream fileStream, long offset, long length)
        {
            long length2 = fileStream.Length;

            if (length == -1L)
            {
                length = length2 - offset;
            }

            if (length == 0L || offset < 0L || length > length2 - offset)
            {
                return;
            }

            if (offset > 0L)
            {
                fileStream.Seek(offset, SeekOrigin.Begin);
            }

            if (length <= 65536L)
            {
                byte[] array = new byte[(int)length];
                int length3 = fileStream.Read(array, 0, (int)length);
                this.SendResponseFromMemory(array, length3);

                return;
            }

            byte[] array2 = new byte[65536];
            int i = (int)length;

            while (i > 0)
            {
                int count = (i < 65536) ? i : 65536;
                int num = fileStream.Read(array2, 0, count);
                this.SendResponseFromMemory(array2, num);
                i -= num;

                if (i > 0 && num > 0)
                {
                    this.FlushResponse(false);
                }
            }
        }
    }
}
