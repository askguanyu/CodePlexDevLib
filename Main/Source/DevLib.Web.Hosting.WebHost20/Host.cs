//-----------------------------------------------------------------------
// <copyright file="Host.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Hosting.WebHost20
{
    using System;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;

    /// <summary>
    /// Host class.
    /// </summary>
    internal sealed class Host : MarshalByRefObject, IRegisteredObject
    {
        /// <summary>
        /// Field _server.
        /// </summary>
        private WebServer _server;

        /// <summary>
        /// Field _pendingCallsCount.
        /// </summary>
        private long _pendingCallsCount;

        /// <summary>
        /// Field _lowerCasedVirtualPath.
        /// </summary>
        private string _lowerCasedVirtualPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="Host"/> class.
        /// </summary>
        public Host()
        {
            HostingEnvironment.RegisterObject(this);
        }

        /// <summary>
        /// Gets the install path.
        /// </summary>
        public string InstallPath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the normalized client script path.
        /// </summary>
        public string NormalizedClientScriptPath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the normalized virtual path.
        /// </summary>
        public string NormalizedVirtualPath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the physical client script path.
        /// </summary>
        public string PhysicalClientScriptPath
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
        /// Gets the virtual path.
        /// </summary>
        public string VirtualPath
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
        /// Gets a value indicating whether disable directory listing.
        /// </summary>
        public bool DisableDirectoryListing
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns>An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease" /> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime" /> property.</returns>
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// Configures the specified server.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="port">The port.</param>
        /// <param name="virtualPath">The virtual path.</param>
        /// <param name="physicalPath">The physical path.</param>
        /// <param name="requireAuthentication">true if require authentication; otherwise, false.</param>
        public void Configure(WebServer server, int port, string virtualPath, string physicalPath, bool requireAuthentication)
        {
            this.Configure(server, port, virtualPath, physicalPath, requireAuthentication, false);
        }

        /// <summary>
        /// Configures the specified server.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="port">The port.</param>
        /// <param name="virtualPath">The virtual path.</param>
        /// <param name="physicalPath">The physical path.</param>
        /// <param name="requireAuthentication">true if require authentication; otherwise, false.</param>
        /// <param name="disableDirectoryListing">true if disable directory listing; otherwise, false.</param>
        public void Configure(WebServer server, int port, string virtualPath, string physicalPath, bool requireAuthentication, bool disableDirectoryListing)
        {
            this._server = server;
            this.Port = port;
            this.InstallPath = null;
            this.VirtualPath = virtualPath;
            this.RequireAuthentication = requireAuthentication;
            this.DisableDirectoryListing = disableDirectoryListing;
            this._lowerCasedVirtualPath = CultureInfo.InvariantCulture.TextInfo.ToLower(this.VirtualPath);
            this.NormalizedVirtualPath = CultureInfo.InvariantCulture.TextInfo.ToLower(virtualPath.EndsWith("/", StringComparison.Ordinal) ? virtualPath : (virtualPath + "/"));
            this.PhysicalPath = physicalPath;
            this.PhysicalClientScriptPath = HttpRuntime.AspClientScriptPhysicalPath + "\\";
            this.NormalizedClientScriptPath = CultureInfo.InvariantCulture.TextInfo.ToLower(HttpRuntime.AspClientScriptVirtualPath + "/");
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public void ProcessRequest(Connection connection)
        {
            this.AddPendingCall();

            try
            {
                Request request = new Request(this, connection);
                request.Process();
            }
            finally
            {
                this.RemovePendingCall();
            }
        }

        /// <summary>
        /// Shutdowns this instance.
        /// </summary>
        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        public void Shutdown()
        {
            HostingEnvironment.InitiateShutdown();
        }

        /// <summary>
        /// Determines whether the virtual path is in application.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>true if the virtual path is in application; otherwise, false.</returns>
        public bool IsVirtualPathInApp(string path)
        {
            bool flag;

            return this.IsVirtualPathInApp(path, out flag);
        }

        /// <summary>
        /// Determines whether the virtual path is in application.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="isClientScriptPath">true if is the client script path; otherwise, false.</param>
        /// <returns>true if the virtual path is in application; otherwise, false.</returns>
        public bool IsVirtualPathInApp(string path, out bool isClientScriptPath)
        {
            isClientScriptPath = false;

            if (path == null)
            {
                return false;
            }

            path = CultureInfo.InvariantCulture.TextInfo.ToLower(path);

            if (this.VirtualPath == "/" && path.StartsWith("/", StringComparison.Ordinal))
            {
                if (path.StartsWith(this.NormalizedClientScriptPath, StringComparison.Ordinal))
                {
                    isClientScriptPath = true;
                }

                return true;
            }

            if (path.StartsWith(this.NormalizedVirtualPath, StringComparison.Ordinal))
            {
                return true;
            }

            if (path == this._lowerCasedVirtualPath)
            {
                return true;
            }

            if (path.StartsWith(this.NormalizedClientScriptPath, StringComparison.Ordinal))
            {
                isClientScriptPath = true;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the virtual path is application path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>true if the virtual path is application path; otherwise, false.</returns>
        public bool IsVirtualPathAppPath(string path)
        {
            if (path == null)
            {
                return false;
            }

            path = CultureInfo.InvariantCulture.TextInfo.ToLower(path);

            return path == this._lowerCasedVirtualPath || path == this.NormalizedVirtualPath;
        }

        /// <summary>
        /// Gets the process token.
        /// </summary>
        /// <returns>IntPtr instance.</returns>
        public IntPtr GetProcessToken()
        {
            new SecurityPermission(PermissionState.Unrestricted).Assert();

            return this._server.GetProcessToken();
        }

        /// <summary>
        /// Gets the process user.
        /// </summary>
        /// <returns>User string.</returns>
        public string GetProcessUser()
        {
            return this._server.GetProcessUser();
        }

        /// <summary>
        /// Gets the process sid.
        /// </summary>
        /// <returns>SecurityIdentifier instance.</returns>
        public SecurityIdentifier GetProcessSID()
        {
            SecurityIdentifier result = null;

            using (WindowsIdentity windowsIdentity = new WindowsIdentity(this._server.GetProcessToken()))
            {
                result = windowsIdentity.User;
            }

            return result;
        }

        /// <summary>
        /// Requests a registered object to unregister.
        /// </summary>
        /// <param name="immediate">true to indicate the registered object should unregister from the hosting environment before returning; otherwise, false.</param>
        void IRegisteredObject.Stop(bool immediate)
        {
            if (this._server != null)
            {
                this._server.HostStopped();
            }

            this.WaitForPendingCallsToFinish();

            HostingEnvironment.UnregisterObject(this);
        }

        /// <summary>
        /// Waits for pending calls to finish.
        /// </summary>
        private void WaitForPendingCallsToFinish()
        {
            while (this._pendingCallsCount > 0)
            {
                Thread.Sleep(250);
            }
        }

        /// <summary>
        /// Adds the pending call.
        /// </summary>
        private void AddPendingCall()
        {
            Interlocked.Increment(ref this._pendingCallsCount);
        }

        /// <summary>
        /// Removes the pending call.
        /// </summary>
        private void RemovePendingCall()
        {
            Interlocked.Decrement(ref this._pendingCallsCount);
        }
    }
}
