//-----------------------------------------------------------------------
// <copyright file="WcfIsolatedServiceHost.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Threading;

    /// <summary>
    /// Wcf Isolated ServiceHost.
    /// </summary>
    [Serializable]
    public sealed class WcfIsolatedServiceHost : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Field _appDomain.
        /// </summary>
        [NonSerialized]
        private AppDomain _appDomain;

        /// <summary>
        /// Field _wcfServiceHost.
        /// </summary>
        private WcfServiceHost _wcfServiceHost;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfIsolatedServiceHost" /> class.
        /// Default constructor of WcfIsolatedServiceHost, create an isolated AppDomain to host Wcf service. Use Initialize method to initialize Wcf service.
        /// </summary>
        public WcfIsolatedServiceHost()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfIsolatedServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="configFile">Wcf service config file.</param>
        public WcfIsolatedServiceHost(string assemblyFile, string configFile = null)
        {
            this.Initialize(assemblyFile, configFile);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="WcfIsolatedServiceHost" /> class.
        /// </summary>
        ~WcfIsolatedServiceHost()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Event Created.
        /// </summary>
        public event EventHandler<EventArgs> Created;

        /// <summary>
        /// Event Opening.
        /// </summary>
        public event EventHandler<EventArgs> Opening;

        /// <summary>
        /// Event Opened.
        /// </summary>
        public event EventHandler<EventArgs> Opened;

        /// <summary>
        /// Event Closing.
        /// </summary>
        public event EventHandler<EventArgs> Closing;

        /// <summary>
        /// Event Closed.
        /// </summary>
        public event EventHandler<EventArgs> Closed;

        /// <summary>
        /// Event Aborting.
        /// </summary>
        public event EventHandler<EventArgs> Aborting;

        /// <summary>
        /// Event Aborted.
        /// </summary>
        public event EventHandler<EventArgs> Aborted;

        /// <summary>
        /// Event Restarting.
        /// </summary>
        public event EventHandler<EventArgs> Restarting;

        /// <summary>
        /// Event Restarted.
        /// </summary>
        public event EventHandler<EventArgs> Restarted;

        /// <summary>
        /// Event Loaded.
        /// </summary>
        public event EventHandler<EventArgs> Loaded;

        /// <summary>
        /// Event Unloaded.
        /// </summary>
        public event EventHandler<EventArgs> Unloaded;

        /// <summary>
        /// Event Reloaded.
        /// </summary>
        public event EventHandler<EventArgs> Reloaded;

        /// <summary>
        /// Gets a value indicating whether isolated AppDomain is loaded.
        /// </summary>
        public bool IsAppDomainLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets current Wcf service assembly file.
        /// </summary>
        public string AssemblyFile
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets current Wcf service config file.
        /// </summary>
        public string ConfigFile
        {
            get;
            private set;
        }

        /// <summary>
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="configFile">Wcf service config file.</param>
        public void Initialize(string assemblyFile, string configFile = null)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new ArgumentException("The file does not exist.", assemblyFile);
            }

            this.AssemblyFile = assemblyFile;

            this.ConfigFile = configFile ?? AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            if (!File.Exists(this.ConfigFile))
            {
                throw new ArgumentException("The file does not exist.", this.ConfigFile);
            }

            this.CreateDomain();
        }

        /// <summary>
        /// Open Wcf service.
        /// </summary>
        public void Open()
        {
            this.CheckDisposed();

            if (this._wcfServiceHost != null)
            {
                try
                {
                    this._wcfServiceHost.Open();
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Close Wcf service.
        /// </summary>
        public void Close()
        {
            this.CheckDisposed();

            if (this._wcfServiceHost != null)
            {
                try
                {
                    this._wcfServiceHost.Close();
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Abort Wcf service.
        /// </summary>
        public void Abort()
        {
            this.CheckDisposed();

            if (this._wcfServiceHost != null)
            {
                try
                {
                    this._wcfServiceHost.Abort();
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Restart Wcf service.
        /// </summary>
        public void Restart()
        {
            this.CheckDisposed();

            if (this._wcfServiceHost != null)
            {
                try
                {
                    this._wcfServiceHost.Restart();
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Unload current isolated AppDomain.
        /// </summary>
        public void Unload()
        {
            this.CheckDisposed();

            if (this._wcfServiceHost != null)
            {
                this._wcfServiceHost.Dispose();
                this.UnSubscribeAllWcfServiceHostEvent();
                this._wcfServiceHost = null;
            }

            if (this._appDomain != null)
            {
                AppDomain.Unload(this._appDomain);
                this.IsAppDomainLoaded = false;
                this._appDomain = null;
                this.RaiseEvent(this.Unloaded, null);
            }
        }

        /// <summary>
        /// Reload current isolated AppDomain.
        /// </summary>
        public void Reload()
        {
            this.CheckDisposed();

            this.Unload();
            this.CreateDomain();
            this.RaiseEvent(this.Reloaded, null);
        }

        /// <summary>
        /// Get current isolated AppDomain.
        /// </summary>
        /// <returns>Instance of AppDomain.</returns>
        public AppDomain GetAppDomain()
        {
            this.CheckDisposed();

            return this._appDomain;
        }

        /// <summary>
        /// Get Wcf service state list.
        /// </summary>
        /// <returns>Instance of List.</returns>
        public List<WcfServiceHostInfo> GetHostInfoList()
        {
            this.CheckDisposed();

            return this._wcfServiceHost.GetHostInfoList();
        }

        /// <summary>
        /// Gives the <see cref="T:System.AppDomain" /> an infinite lifetime by preventing a lease from being created.
        /// </summary>
        /// <exception cref="T:System.AppDomainUnloadedException">The operation is attempted on an unloaded application domain.</exception>
        /// <returns>Always null.</returns>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="WcfIsolatedServiceHost" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="WcfIsolatedServiceHost" /> class.
        /// protected virtual for non-sealed class; private for sealed class.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
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

                if (this._wcfServiceHost != null)
                {
                    this._wcfServiceHost.Dispose();
                    this._wcfServiceHost = null;
                }

                if (this._appDomain != null)
                {
                    AppDomain.Unload(this._appDomain);
                    this.IsAppDomainLoaded = false;
                    this._appDomain = null;
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
        /// <param name="e">Instance of EventArgs.</param>
        private void RaiseEvent(EventHandler<EventArgs> eventHandler, EventArgs e)
        {
            if (this._disposed)
            {
                return;
            }

            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<EventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, e);
            }
        }

        /// <summary>
        /// Method CreateDomain.
        /// </summary>
        private void CreateDomain()
        {
            try
            {
                AppDomainSetup appDomainSetup = new AppDomainSetup();
                appDomainSetup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
                appDomainSetup.ApplicationName = Path.GetFileNameWithoutExtension(this.AssemblyFile);
                appDomainSetup.ConfigurationFile = this.ConfigFile;
                appDomainSetup.LoaderOptimization = LoaderOptimization.MultiDomainHost;
                appDomainSetup.ShadowCopyFiles = "true";
                appDomainSetup.ShadowCopyDirectories = appDomainSetup.ApplicationBase;

                this._appDomain = AppDomain.CreateDomain(appDomainSetup.ApplicationName, AppDomain.CurrentDomain.Evidence, appDomainSetup);
                this._wcfServiceHost = this._appDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(WcfServiceHost).FullName) as WcfServiceHost;
                this.IsAppDomainLoaded = true;
                this.SubscribeAllWcfServiceHostEvent();
                this._wcfServiceHost.Init(this.AssemblyFile, this.ConfigFile);
                this.RaiseEvent(this.Loaded, null);
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
                this.Unload();
                throw;
            }
        }

        /// <summary>
        /// Method SubscribeAllWcfServiceHostEvent.
        /// </summary>
        private void SubscribeAllWcfServiceHostEvent()
        {
            this._wcfServiceHost.Created += (s, e) => this.RaiseEvent(this.Created, e);
            this._wcfServiceHost.Opening += (s, e) => this.RaiseEvent(this.Opening, e);
            this._wcfServiceHost.Opened += (s, e) => this.RaiseEvent(this.Opened, e);
            this._wcfServiceHost.Closing += (s, e) => this.RaiseEvent(this.Closing, e);
            this._wcfServiceHost.Closed += (s, e) => this.RaiseEvent(this.Closed, e);
            this._wcfServiceHost.Aborting += (s, e) => this.RaiseEvent(this.Aborting, e);
            this._wcfServiceHost.Aborted += (s, e) => this.RaiseEvent(this.Aborted, e);
            this._wcfServiceHost.Restarting += (s, e) => this.RaiseEvent(this.Restarting, e);
            this._wcfServiceHost.Restarted += (s, e) => this.RaiseEvent(this.Restarted, e);
        }

        /// <summary>
        /// Method UnSubscribeAllWcfServiceHostEvent.
        /// </summary>
        private void UnSubscribeAllWcfServiceHostEvent()
        {
            this._wcfServiceHost.Created -= (s, e) => this.RaiseEvent(this.Created, e);
            this._wcfServiceHost.Opening -= (s, e) => this.RaiseEvent(this.Opening, e);
            this._wcfServiceHost.Opened -= (s, e) => this.RaiseEvent(this.Opened, e);
            this._wcfServiceHost.Closing -= (s, e) => this.RaiseEvent(this.Closing, e);
            this._wcfServiceHost.Closed -= (s, e) => this.RaiseEvent(this.Closed, e);
            this._wcfServiceHost.Aborting -= (s, e) => this.RaiseEvent(this.Aborting, e);
            this._wcfServiceHost.Aborted -= (s, e) => this.RaiseEvent(this.Aborted, e);
            this._wcfServiceHost.Restarting -= (s, e) => this.RaiseEvent(this.Restarting, e);
            this._wcfServiceHost.Restarted -= (s, e) => this.RaiseEvent(this.Restarted, e);
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.ServiceModel.WcfIsolatedServiceHost");
            }
        }
    }
}
