//-----------------------------------------------------------------------
// <copyright file="WebServer.cs" company="YuGuan Corporation">
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
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;
    using System.Web.Hosting;

    /// <summary>
    /// Web service or Asp.net hosting.
    /// </summary>
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class WebServer : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Field TOKEN_ALL_ACCESS.
        /// </summary>
        private const int TOKEN_ALL_ACCESS = 983551;

        /// <summary>
        /// Field TOKEN_EXECUTE.
        /// </summary>
        private const int TOKEN_EXECUTE = 131072;

        /// <summary>
        /// Field TOKEN_READ.
        /// </summary>
        private const int TOKEN_READ = 131080;

        /// <summary>
        /// Field TOKEN_IMPERSONATE.
        /// </summary>
        private const int TOKEN_IMPERSONATE = 4;

        /// <summary>
        /// Field SecurityImpersonation.
        /// </summary>
        private const int SecurityImpersonation = 2;

        /// <summary>
        /// Field CurrentAssemblyFullPath.
        /// </summary>
        private static readonly string CurrentAssemblyFullPath = Path.GetFullPath(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

        /// <summary>
        /// Field CurrentAssemblyFilename.
        /// </summary>
        private static readonly string CurrentAssemblyFilename = Path.GetFileName(CurrentAssemblyFullPath);

        /// <summary>
        /// Field _syncRoot.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Field _requireAuthentication.
        /// </summary>
        private bool _requireAuthentication;

        /// <summary>
        /// Field _disableDirectoryListing.
        /// </summary>
        private bool _disableDirectoryListing;

        /// <summary>
        /// Field _onStart.
        /// </summary>
        private WaitCallback _onStart;

        /// <summary>
        /// Field _onSocketAccept.
        /// </summary>
        private WaitCallback _onSocketAccept;

        /// <summary>
        /// Field _shutdownInProgress.
        /// </summary>
        private bool _shutdownInProgress;

        /// <summary>
        /// Field _appManager.
        /// </summary>
        private ApplicationManager _appManager;

        /// <summary>
        /// Field _socketIPv4.
        /// </summary>
        private Socket _socketIPv4;

        /// <summary>
        /// Field _socketIPv6.
        /// </summary>
        private Socket _socketIPv6;

        /// <summary>
        /// Field _host.
        /// </summary>
        private Host _host;

        /// <summary>
        /// Field _processToken.
        /// </summary>
        private IntPtr _processToken;

        /// <summary>
        /// Field _processUser.
        /// </summary>
        private string _processUser;

        /// <summary>
        /// Field _isBinFolderExists.
        /// </summary>
        private bool _isBinFolderExists;

        /// <summary>
        /// Field _binFolder.
        /// </summary>
        private string _binFolder;

        /// <summary>
        /// Field _binFolderReferenceFile.
        /// </summary>
        private string _binFolderReferenceFile;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServer"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="startNow">true if immediately start service; otherwise, false.</param>
        public WebServer(int port, bool startNow = false)
            : this(port, string.Empty, Path.GetDirectoryName(Path.GetFullPath(new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath)), false, false, startNow)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServer"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="virtualPath">The virtual path.</param>
        /// <param name="startNow">true if immediately start service; otherwise, false.</param>
        public WebServer(int port, string virtualPath, bool startNow = false)
            : this(port, virtualPath, Path.GetDirectoryName(Path.GetFullPath(new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath)), false, false, startNow)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServer"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="virtualPath">The virtual path.</param>
        /// <param name="physicalPath">The physical path.</param>
        /// <param name="startNow">true if immediately start service; otherwise, false.</param>
        public WebServer(int port, string virtualPath, string physicalPath, bool startNow = false)
            : this(port, virtualPath, physicalPath, false, false, startNow)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServer"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="virtualPath">The virtual path.</param>
        /// <param name="physicalPath">The physical path.</param>
        /// <param name="requireAuthentication">true if require authentication; otherwise, false.</param>
        /// <param name="startNow">true if immediately start service; otherwise, false.</param>
        public WebServer(int port, string virtualPath, string physicalPath, bool requireAuthentication, bool startNow)
            : this(port, virtualPath, physicalPath, requireAuthentication, false, startNow)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServer"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="virtualPath">The virtual path.</param>
        /// <param name="physicalPath">The physical path.</param>
        /// <param name="requireAuthentication">true if require authentication; otherwise, false.</param>
        /// <param name="disableDirectoryListing">true if disable directory listing; otherwise, false.</param>
        /// <param name="startNow">true if immediately start service; otherwise, false.</param>
        public WebServer(int port, string virtualPath, string physicalPath, bool requireAuthentication, bool disableDirectoryListing, bool startNow)
        {
            this.Port = port;
            this.VirtualPath = physicalPath == null || string.IsNullOrEmpty(virtualPath.Trim()) ? "/" : "/" + virtualPath.Trim('/');
            this.PhysicalPath = Path.GetFullPath(physicalPath == null || string.IsNullOrEmpty(physicalPath.Trim()) ? "." : physicalPath).TrimEnd('\\') + "\\";
            this._binFolder = Path.Combine(this.PhysicalPath, "bin");
            this._binFolderReferenceFile = Path.Combine(this._binFolder, CurrentAssemblyFilename);
            this._requireAuthentication = requireAuthentication;
            this._disableDirectoryListing = disableDirectoryListing;
            this._onSocketAccept = new WaitCallback(this.OnSocketAccept);
            this._onStart = new WaitCallback(this.OnStart);
            this._appManager = ApplicationManager.GetApplicationManager();
            this.ObtainProcessToken();

            if (startNow)
            {
                this.Start();
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="WebServer" /> class.
        /// </summary>
        ~WebServer()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the virtual path.
        /// </summary>
        public string VirtualPath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the physical path.
        /// </summary>
        public string PhysicalPath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the port.
        /// </summary>
        public int Port
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the root URL.
        /// </summary>
        public string RootUrl
        {
            get
            {
                if (this.Port != 80)
                {
                    return "http://localhost:" + this.Port + "/" + this.VirtualPath.TrimStart('/');
                }

                return "http://localhost/" + this.VirtualPath.TrimStart('/');
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="WebServer" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="WebServer" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns>An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease" /> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime" /> property.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// Gets the process token.
        /// </summary>
        /// <returns>IntPtr instance.</returns>
        public IntPtr GetProcessToken()
        {
            this.CheckDisposed();

            return this._processToken;
        }

        /// <summary>
        /// Gets the process user.
        /// </summary>
        /// <returns>User string.</returns>
        public string GetProcessUser()
        {
            this.CheckDisposed();

            return this._processUser;
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        public void Start()
        {
            this.CheckDisposed();

            bool flag = false;

            flag = Socket.SupportsIPv4;

            if (Socket.OSSupportsIPv6)
            {
                try
                {
                    this._socketIPv6 = this.CreateSocketBindAndListen(AddressFamily.InterNetworkV6, IPAddress.IPv6Any, this.Port);
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.AddressAlreadyInUse || !flag)
                    {
                        throw;
                    }
                }
            }

            if (flag)
            {
                try
                {
                    this._socketIPv4 = this.CreateSocketBindAndListen(AddressFamily.InterNetwork, IPAddress.Any, this.Port);
                }
                catch (SocketException)
                {
                    if (this._socketIPv6 == null)
                    {
                        throw;
                    }
                }
            }

            this.CopyReferenceFile();

            if (this._socketIPv6 != null)
            {
                ThreadPool.QueueUserWorkItem(this._onStart, this._socketIPv6);
            }

            if (this._socketIPv4 != null)
            {
                ThreadPool.QueueUserWorkItem(this._onStart, this._socketIPv4);
            }
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop()
        {
            this.CheckDisposed();

            this._shutdownInProgress = true;

            try
            {
                if (this._socketIPv4 != null)
                {
                    this._socketIPv4.Close();
                }

                if (this._socketIPv6 != null)
                {
                    this._socketIPv6.Close();
                }
            }
            catch
            {
            }
            finally
            {
                this._socketIPv4 = null;
                this._socketIPv6 = null;
            }

            try
            {
                if (this._host != null)
                {
                    this._host.Shutdown();
                }

                while (this._host != null)
                {
                    Thread.Sleep(100);
                }
            }
            catch
            {
            }
            finally
            {
                this._host = null;
            }

            this.RemoveReferenceFile();
        }

        /// <summary>
        /// Stopped host.
        /// </summary>
        internal void HostStopped()
        {
            this._host = null;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="WebServer" /> class.
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

                this._shutdownInProgress = true;

                try
                {
                    if (this._socketIPv4 != null)
                    {
                        this._socketIPv4.Close();
                    }

                    if (this._socketIPv6 != null)
                    {
                        this._socketIPv6.Close();
                    }
                }
                catch
                {
                }
                finally
                {
                    this._socketIPv4 = null;
                    this._socketIPv6 = null;
                }

                try
                {
                    if (this._host != null)
                    {
                        this._host.Shutdown();
                    }
                }
                catch
                {
                }
                finally
                {
                    this._host = null;
                }

                this.RemoveReferenceFile();
            }

            // free native resources
            ////if (nativeResource != IntPtr.Zero)
            ////{
            ////    Marshal.FreeHGlobal(nativeResource);
            ////    nativeResource = IntPtr.Zero;
            ////}
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.Web.Hosting.WebHost20.WebServer");
            }
        }

        /// <summary>
        /// Obtains the process token.
        /// </summary>
        private void ObtainProcessToken()
        {
            if (NativeMethods.ImpersonateSelf(2))
            {
                NativeMethods.OpenThreadToken(NativeMethods.GetCurrentThread(), 983551, true, ref this._processToken);
                NativeMethods.RevertToSelf();
                this._processUser = WindowsIdentity.GetCurrent().Name;
            }
        }

        /// <summary>
        /// Creates the socket bind and listen.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="port">The port.</param>
        /// <returns>Socket instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        private Socket CreateSocketBindAndListen(AddressFamily family, IPAddress ipAddress, int port)
        {
            Socket socket = null;

            try
            {
                socket = new Socket(family, SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.Bind(new IPEndPoint(ipAddress, port));
            }
            catch
            {
                if (socket != null)
                {
                    socket.Close();
                    socket = null;
                }

                throw;
            }

            socket.Listen(int.MaxValue);

            return socket;
        }

        /// <summary>
        /// Called when socket accept.
        /// </summary>
        /// <param name="acceptedSocket">The accepted socket.</param>
        private void OnSocketAccept(object acceptedSocket)
        {
            if (!this._shutdownInProgress)
            {
                Connection connection = new Connection(this, (Socket)acceptedSocket);

                if (connection.WaitForRequestBytes() == 0)
                {
                    connection.WriteErrorAndClose(400);
                    return;
                }

                Host host = null;

                try
                {
                    host = this.GetHost();
                }
                catch (Exception e)
                {
                    connection.WriteErrorAndClose(500, e.ToString());
                    return;
                }

                if (host == null)
                {
                    connection.WriteErrorAndClose(500);
                    return;
                }

                try
                {
                    host.ProcessRequest(connection);
                }
                catch (Exception e)
                {
                    string exception = string.Format(Constants.UnhandledException, e.GetType().ToString());
                    connection.WriteEntireResponseFromString(500, "Content-type:text/html;charset=utf-8\r\n", Messages.FormatExceptionMessageBody(exception, e.Message, e.StackTrace), true);
                }
            }
        }

        /// <summary>
        /// Called when start.
        /// </summary>
        /// <param name="listeningSocket">The listening socket.</param>
        private void OnStart(object listeningSocket)
        {
            while (!this._shutdownInProgress)
            {
                try
                {
                    if (listeningSocket != null)
                    {
                        Socket state = ((Socket)listeningSocket).Accept();

                        ThreadPool.QueueUserWorkItem(this._onSocketAccept, state);
                    }
                }
                catch
                {
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// Gets the host.
        /// </summary>
        /// <returns>Host instance.</returns>
        private Host GetHost()
        {
            if (this._shutdownInProgress)
            {
                return null;
            }

            Host host = this._host;

            if (host == null)
            {
                lock (this._syncRoot)
                {
                    host = this._host;

                    if (host == null)
                    {
                        string text = (this.VirtualPath + this.PhysicalPath).ToLowerInvariant();
                        string appId = text.GetHashCode().ToString("x", CultureInfo.InvariantCulture);

                        this._host = (Host)this._appManager.CreateObject(appId, typeof(Host), this.VirtualPath, this.PhysicalPath, false);
                        this._host.Configure(this, this.Port, this.VirtualPath, this.PhysicalPath, this._requireAuthentication, this._disableDirectoryListing);

                        host = this._host;
                    }
                }
            }

            return host;
        }

        /// <summary>
        /// Copies the reference file.
        /// </summary>
        private void CopyReferenceFile()
        {
            this._isBinFolderExists = Directory.Exists(this._binFolder);

            if (!this._isBinFolderExists)
            {
                try
                {
                    Directory.CreateDirectory(this._binFolder);
                }
                catch
                {
                }
            }

            try
            {
                File.Copy(CurrentAssemblyFullPath, this._binFolderReferenceFile, true);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Removes the reference file.
        /// </summary>
        private void RemoveReferenceFile()
        {
            try
            {
                File.Delete(this._binFolderReferenceFile);
            }
            catch
            {
            }

            if (!this._isBinFolderExists)
            {
                try
                {
                    Directory.Delete(this._binFolder, true);
                }
                catch
                {
                }
            }
        }
    }
}
