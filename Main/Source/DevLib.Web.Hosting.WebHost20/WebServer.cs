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
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;
    using System.Web.Hosting;
    using System.Xml;
    using DevLib.Web.Hosting.WebHost20.NativeAPI;
    using DevLib.Web.Hosting.WebHost20.Properties;

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
        /// Field SecurityImpersonation.
        /// </summary>
        private const int SecurityImpersonation = 2;

        /// <summary>
        /// The application host node name.
        /// </summary>
        private const string ApplicationHostNodeName = @"configuration/system.applicationHost";

        /// <summary>
        /// The web server node name.
        /// </summary>
        private const string WebServerNodeName = @"configuration/system.webServer";

        /// <summary>
        /// The application pools node name.
        /// </summary>
        private const string ApplicationPoolsNodeName = @"configuration/system.applicationHost/applicationPools";

        /// <summary>
        /// The site node name.
        /// </summary>
        private const string SiteNodeName = @"configuration/system.applicationHost/sites/site";

        /// <summary>
        /// The directory browse node name.
        /// </summary>
        private const string DirectoryBrowseNodeName = @"configuration/system.webServer/directoryBrowse";

        /// <summary>
        /// Field CurrentAssemblyFullPath.
        /// </summary>
        private static readonly string CurrentAssemblyFullPath = Path.GetFullPath(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

        /// <summary>
        /// Field CurrentAssemblyFilename.
        /// </summary>
        private static readonly string CurrentAssemblyFilename = Path.GetFileName(CurrentAssemblyFullPath);

        /// <summary>
        /// The root web configuration path.
        /// </summary>
        private static readonly string RootWebConfigPath = Environment.ExpandEnvironmentVariables(@"%windir%\Microsoft.Net\Framework\v2.0.50727\config\web.config");

        /// <summary>
        /// Field HostedWebCoreDllDirectory.
        /// </summary>
        private static readonly string HostedWebCoreDllDirectory = Environment.ExpandEnvironmentVariables(@"%windir%\system32\inetsrv");

        /// <summary>
        /// The local application host configuration file.
        /// </summary>
        private static readonly string LocalAppHostConfigFile = Path.Combine(Path.GetFullPath(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "applicationHost.config");

        /// <summary>
        /// Field _syncRoot.
        /// </summary>
        private readonly object _syncRoot = new object();

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
        /// Field _appHostConfigFile.
        /// </summary>
        private string _appHostConfigFile;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServer" /> class.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <param name="port">The port.</param>
        /// <param name="useIIS">true to use IIS Hosted Web Core hosting; false to use managed code web server hosting.</param>
        /// <param name="startNow">true if immediately start service; otherwise, false.</param>
        public WebServer(int siteId, int port = 80, bool useIIS = false, bool startNow = false)
            : this(siteId, Path.GetDirectoryName(Path.GetFullPath(new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath)), null, port, null, false, false, useIIS, startNow)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServer" /> class.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <param name="physicalPath">The physical path.</param>
        /// <param name="port">The port.</param>
        /// <param name="useIIS">true to use IIS Hosted Web Core hosting; false to use managed code web server hosting.</param>
        /// <param name="startNow">true if immediately start service; otherwise, false.</param>
        public WebServer(int siteId, string physicalPath, int port = 80, bool useIIS = false, bool startNow = false)
            : this(siteId, physicalPath, null, port, null, false, false, useIIS, startNow)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServer" /> class.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <param name="physicalPath">The physical path.</param>
        /// <param name="virtualPath">The virtual path.</param>
        /// <param name="port">The port.</param>
        /// <param name="useIIS">true to use IIS Hosted Web Core hosting; false to use managed code web server hosting.</param>
        /// <param name="startNow">true if immediately start service; otherwise, false.</param>
        public WebServer(int siteId, string physicalPath, string virtualPath = null, int port = 80, bool useIIS = false, bool startNow = false)
            : this(siteId, physicalPath, virtualPath, port, null, false, false, useIIS, startNow)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServer" /> class.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <param name="physicalPath">The physical path.</param>
        /// <param name="virtualPath">The virtual path.</param>
        /// <param name="port">The port.</param>
        /// <param name="requireAuthentication">true if require authentication; otherwise, false.</param>
        /// <param name="useIIS">true to use IIS Hosted Web Core hosting; false to use managed code web server hosting.</param>
        /// <param name="startNow">true if immediately start service; otherwise, false.</param>
        public WebServer(int siteId, string physicalPath, string virtualPath, int port, bool requireAuthentication, bool useIIS = false, bool startNow = false)
            : this(siteId, physicalPath, virtualPath, port, null, requireAuthentication, false, useIIS, startNow)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServer" /> class.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <param name="physicalPath">The physical path.</param>
        /// <param name="virtualPath">The virtual path.</param>
        /// <param name="port">The port.</param>
        /// <param name="siteName">The site name.</param>
        /// <param name="requireAuthentication">true if require authentication; otherwise, false.</param>
        /// <param name="enableDirectoryBrowse">true to enable directory browsing; otherwise, false.</param>
        /// <param name="useIIS">true to use IIS Hosted Web Core hosting; false to use managed code web server hosting.</param>
        /// <param name="startNow">true if immediately start service; otherwise, false.</param>
        public WebServer(int siteId, string physicalPath, string virtualPath, int port, string siteName, bool requireAuthentication, bool enableDirectoryBrowse, bool useIIS, bool startNow)
        {
            this.SiteId = siteId;
            this.PhysicalPath = Path.GetFullPath(this.IsNullOrWhiteSpace(physicalPath) ? "." : physicalPath).TrimEnd('\\') + "\\";
            this.VirtualPath = this.IsNullOrWhiteSpace(virtualPath) ? "/" : "/" + virtualPath.Trim('/');
            this.Port = port;
            this.SiteName = siteName ?? Guid.NewGuid().ToString();
            this.RequireAuthentication = requireAuthentication;
            this.EnableDirectoryBrowse = enableDirectoryBrowse;
            this.IsUsingIIS = useIIS;

            if (this.IsUsingIIS)
            {
                this._appHostConfigFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", string.Empty) + ".config");
            }
            else
            {
                this._binFolder = Path.Combine(this.PhysicalPath, "bin");
                this._binFolderReferenceFile = Path.Combine(this._binFolder, CurrentAssemblyFilename);
                this._onSocketAccept = new WaitCallback(this.OnSocketAccept);
                this._onStart = new WaitCallback(this.OnStart);
                this._appManager = ApplicationManager.GetApplicationManager();
                this.ObtainProcessToken();
            }

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
        /// Gets the site name.
        /// </summary>
        public string SiteName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the site id.
        /// </summary>
        public int SiteId
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
        /// Gets a value indicating whether require authentication.
        /// </summary>
        public bool RequireAuthentication
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether directory browsing is enabled or disabled on the Web server.
        /// </summary>
        public bool EnableDirectoryBrowse
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
        /// Gets a value indicating whether use IIS hosting.
        /// </summary>
        public bool IsUsingIIS
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this web server is running.
        /// </summary>
        public bool IsRunning
        {
            get;
            private set;
        }

        /// <summary>
        /// Starts web server.
        /// </summary>
        public void Start()
        {
            this.CheckDisposed();

            if (!this.IsRunning)
            {
                if (this.IsUsingIIS)
                {
                    this.StartHostedWebCore();
                }
                else
                {
                    this.StartManagedCodeWebServer();
                }

                this.IsRunning = true;
            }
        }

        /// <summary>
        /// Stops web server.
        /// </summary>
        public void Stop()
        {
            this.CheckDisposed();

            if (this.IsRunning)
            {
                if (this.IsUsingIIS)
                {
                    this.StopHostedWebCore();
                }
                else
                {
                    this.StopManagedCodeWebServer();
                }

                this.IsRunning = false;
            }
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
        /// Releases all resources used by the current instance of the <see cref="WebServer" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
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

                if (this.IsUsingIIS)
                {
                    this.RemoveAppHostConfigFile();
                }
                else
                {
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
            }

            // free native resources
            ////if (nativeResource != IntPtr.Zero)
            ////{
            ////    Marshal.FreeHGlobal(nativeResource);
            ////    nativeResource = IntPtr.Zero;
            ////}
        }

        /// <summary>
        /// Starts web server with managed code approach.
        /// </summary>
        private void StartManagedCodeWebServer()
        {
            bool isSupportsIPv4 = Socket.SupportsIPv4;

            if (Socket.OSSupportsIPv6)
            {
                try
                {
                    this._socketIPv6 = this.CreateSocketBindAndListen(AddressFamily.InterNetworkV6, IPAddress.IPv6Any, this.Port);
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.AddressAlreadyInUse || !isSupportsIPv4)
                    {
                        throw;
                    }
                }
            }

            if (isSupportsIPv4)
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
        /// Starts web server with IIS approach.
        /// </summary>
        private void StartHostedWebCore()
        {
            this.CreateAppHostConfigFile();

            NativeMethods.SetDllDirectory(HostedWebCoreDllDirectory);

            int result = NativeMethods.WebCoreActivate(this._appHostConfigFile, RootWebConfigPath, Guid.NewGuid().ToString());

            if (result != 0)
            {
                Marshal.ThrowExceptionForHR(result);
            }
        }

        /// <summary>
        /// Stops web server with managed code approach.
        /// </summary>
        private void StopManagedCodeWebServer()
        {
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
        /// Stops web server with IIS approach.
        /// </summary>
        private void StopHostedWebCore()
        {
            int result = NativeMethods.WebCoreShutdown(false);

            this.RemoveAppHostConfigFile();

            if (result != 0)
            {
                Marshal.ThrowExceptionForHR(result);
            }
        }

        /// <summary>
        /// Obtains the process token.
        /// </summary>
        private void ObtainProcessToken()
        {
            if (NativeMethods.ImpersonateSelf(SecurityImpersonation))
            {
                NativeMethods.OpenThreadToken(NativeMethods.GetCurrentThread(), TOKEN_ALL_ACCESS, true, ref this._processToken);
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
                        this._host.Configure(this, this.Port, this.VirtualPath, this.PhysicalPath, this.RequireAuthentication, this.EnableDirectoryBrowse);

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
                if (this.IsDirectoryEmpty(this._binFolder))
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

        /// <summary>
        /// Creates the application host configuration file.
        /// </summary>
        private void CreateAppHostConfigFile()
        {
            try
            {
                XmlDocument appHostConfigXml = new XmlDocument();

                if (File.Exists(LocalAppHostConfigFile))
                {
                    appHostConfigXml.Load(LocalAppHostConfigFile);
                }
                else
                {
                    appHostConfigXml.LoadXml(Resources.ApplicationHost);
                }

                string appHostConfigTemplate = Resources.AppHostTemplate;

                string configContent = appHostConfigTemplate
                    .Replace("$[SiteName]", this.SiteName)
                    .Replace("$[SiteId]", this.SiteId.ToString())
                    .Replace("$[Port]", this.Port.ToString())
                    .Replace("$[VirtualPath]", this.VirtualPath)
                    .Replace("$[PhysicalPath]", this.PhysicalPath)
                    .Replace("$[AppPool]", string.Format("{0}_{1}_{2}", this.SiteName, this.SiteId.ToString(), this.Port.ToString()))
                    .Replace("$[DirectoryBrowse]", this.EnableDirectoryBrowse.ToString());

                XmlDocument configContentXml = new XmlDocument();
                configContentXml.LoadXml(configContent);

                XmlNode applicationHostNode = appHostConfigXml.SelectSingleNode(ApplicationHostNodeName);

                if (applicationHostNode != null)
                {
                    XmlNodeList applicationPoolsNodes = appHostConfigXml.SelectNodes(ApplicationPoolsNodeName);

                    if (applicationPoolsNodes != null && applicationPoolsNodes.Count > 0)
                    {
                        foreach (XmlNode item in applicationPoolsNodes)
                        {
                            applicationHostNode.RemoveChild(item);
                        }
                    }

                    XmlNode applicationPoolsNode = appHostConfigXml.ImportNode(configContentXml.SelectSingleNode(ApplicationPoolsNodeName), true);
                    applicationHostNode.AppendChild(applicationPoolsNode);

                    XmlNodeList siteNodes = appHostConfigXml.SelectNodes(SiteNodeName);

                    if (siteNodes != null && siteNodes.Count > 0)
                    {
                        foreach (XmlNode item in siteNodes)
                        {
                            applicationHostNode.RemoveChild(item);
                        }
                    }

                    XmlNode siteNode = appHostConfigXml.ImportNode(configContentXml.SelectSingleNode(SiteNodeName), true);
                    applicationHostNode.AppendChild(siteNode);
                }
                else
                {
                    applicationHostNode = appHostConfigXml.ImportNode(configContentXml.SelectSingleNode(ApplicationHostNodeName), true);
                    appHostConfigXml.DocumentElement.AppendChild(applicationHostNode);
                }

                XmlNode webServerNode = appHostConfigXml.SelectSingleNode(WebServerNodeName);

                if (webServerNode != null)
                {
                    XmlNodeList directoryBrowseNodeNameNodes = appHostConfigXml.SelectNodes(DirectoryBrowseNodeName);

                    if (directoryBrowseNodeNameNodes != null && directoryBrowseNodeNameNodes.Count > 0)
                    {
                        foreach (XmlNode item in directoryBrowseNodeNameNodes)
                        {
                            webServerNode.RemoveChild(item);
                        }
                    }

                    XmlNode directoryBrowseNodeNameNode = appHostConfigXml.ImportNode(configContentXml.SelectSingleNode(DirectoryBrowseNodeName), true);
                    webServerNode.AppendChild(directoryBrowseNodeNameNode);
                }
                else
                {
                    webServerNode = appHostConfigXml.ImportNode(configContentXml.SelectSingleNode(WebServerNodeName), true);
                    appHostConfigXml.DocumentElement.AppendChild(webServerNode);
                }

                appHostConfigXml.Save(this._appHostConfigFile);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Removes the application host configuration file.
        /// </summary>
        private void RemoveAppHostConfigFile()
        {
            try
            {
                File.Delete(this._appHostConfigFile);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Determines whether the specified path is empty directory.
        /// </summary>
        /// <param name="sourcePath">The path to check.</param>
        /// <returns>true if the specified path is empty directory; otherwise, false.</returns>
        private bool IsDirectoryEmpty(string sourcePath)
        {
            string[] dirs = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories);

            if (dirs.Length == 0)
            {
                string[] files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);

                return files.Length == 0;
            }

            return false;
        }

        /// <summary>
        /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the value parameter is null or String.Empty, or if value consists exclusively of white-space characters.</returns>
        private bool IsNullOrWhiteSpace(string value)
        {
            if (value == null)
            {
                return true;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }

            return true;
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
    }
}
