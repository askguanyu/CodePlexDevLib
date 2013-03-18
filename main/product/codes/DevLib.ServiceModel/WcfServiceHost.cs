//-----------------------------------------------------------------------
// <copyright file="WcfServiceHost.cs" company="YuGuan Corporation">
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
    using System.Xml;

    /// <summary>
    /// Class WcfServiceHost.
    /// </summary>
    [Serializable]
    public sealed class WcfServiceHost : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Field _appDomain.
        /// </summary>
        [NonSerialized]
        private AppDomain _appDomain;

        /// <summary>
        /// Field _wcfServiceHostProxy.
        /// </summary>
        private WcfServiceHostProxy _wcfServiceHostProxy;

        /// <summary>
        /// Field _baseAddress.
        /// </summary>
        private string _baseAddress;

        /// <summary>
        /// Field _tempConfigFile.
        /// </summary>
        private string _tempConfigFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// Default constructor of WcfServiceHost, create an isolated AppDomain to host Wcf service. Use Initialize method to initialize Wcf service.
        /// </summary>
        public WcfServiceHost()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        public WcfServiceHost(string assemblyFile)
        {
            this.Initialize(assemblyFile);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="configFile">Wcf service config file.</param>
        public WcfServiceHost(string assemblyFile, string configFile)
        {
            this.Initialize(assemblyFile, configFile);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public WcfServiceHost(string assemblyFile, string configFile, string baseAddress)
        {
            this.Initialize(assemblyFile, configFile, baseAddress);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        ~WcfServiceHost()
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
        /// <param name="baseAddress">Wcf service base address.</param>
        public void Initialize(string assemblyFile, string configFile = null, string baseAddress = null)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("The specified file does not exist.", assemblyFile);
            }

            this.AssemblyFile = assemblyFile;

            if (string.IsNullOrEmpty(configFile))
            {
                this.ConfigFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            }
            else
            {
                if (!File.Exists(configFile))
                {
                    throw new FileNotFoundException("The specified file does not exist.", configFile);
                }
                else
                {
                    this.ConfigFile = configFile;
                }
            }

            this._baseAddress = baseAddress;

            this.CleanTempWcfConfigFile();

            this._tempConfigFile = this.GetTempWcfConfigFile(this.ConfigFile, this._baseAddress);

            this.CreateDomain();
        }

        /// <summary>
        /// Open Wcf service.
        /// </summary>
        public void Open()
        {
            this.CheckDisposed();

            if (this._wcfServiceHostProxy != null)
            {
                try
                {
                    this._wcfServiceHostProxy.Open();
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

            if (this._wcfServiceHostProxy != null)
            {
                try
                {
                    this._wcfServiceHostProxy.Close();
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

            if (this._wcfServiceHostProxy != null)
            {
                try
                {
                    this._wcfServiceHostProxy.Abort();
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

            if (this._wcfServiceHostProxy != null)
            {
                try
                {
                    this._wcfServiceHostProxy.Restart();
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

            if (this._wcfServiceHostProxy != null)
            {
                this._wcfServiceHostProxy.Dispose();
                this.UnSubscribeAllWcfServiceHostProxyEvent();
                this._wcfServiceHostProxy = null;
            }

            if (this._appDomain != null)
            {
                AppDomain.Unload(this._appDomain);
                this.IsAppDomainLoaded = false;
                this.UnSubscribeDomainExitEvent();
                this._appDomain = null;
                this.RaiseEvent(this.Unloaded, null);
            }

            this.CleanTempWcfConfigFile();
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

            return this._wcfServiceHostProxy.GetHostInfoList();
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
        /// Releases all resources used by the current instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Method RemoveWcfConfigFileBaseAddressNode.
        /// </summary>
        /// <param name="sourceFileName">Source wcf config file name.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <returns>Temp Wcf config file full path.</returns>
        private string GetTempWcfConfigFile(string sourceFileName, string baseAddress)
        {
            string result = null;

            try
            {
                result = Path.GetTempFileName();
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
                throw;
            }

            if (string.IsNullOrEmpty(baseAddress))
            {
                try
                {
                    File.Copy(sourceFileName, result, true);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    throw;
                }
            }
            else
            {
                try
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(sourceFileName);

                    var nodeList = xmlDocument.SelectNodes(@"configuration/system.serviceModel/services/service/host");

                    if (nodeList != null && nodeList.Count > 0)
                    {
                        foreach (XmlNode item in nodeList)
                        {
                            item.RemoveAll();
                        }
                    }

                    xmlDocument.Save(result);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    throw;
                }
            }

            return result;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="WcfServiceHost" /> class.
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

                if (this._wcfServiceHostProxy != null)
                {
                    this._wcfServiceHostProxy.Dispose();
                    this.UnSubscribeAllWcfServiceHostProxyEvent();
                    this._wcfServiceHostProxy = null;
                }

                if (this._appDomain != null)
                {
                    AppDomain.Unload(this._appDomain);
                    this.IsAppDomainLoaded = false;
                    this.UnSubscribeDomainExitEvent();
                    this._appDomain = null;
                }

                this.CleanTempWcfConfigFile();
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
                appDomainSetup.ConfigurationFile = this._tempConfigFile;
                appDomainSetup.LoaderOptimization = LoaderOptimization.MultiDomainHost;
                appDomainSetup.ShadowCopyFiles = "true";
                appDomainSetup.ShadowCopyDirectories = appDomainSetup.ApplicationBase;

                this._appDomain = AppDomain.CreateDomain(appDomainSetup.ApplicationName, AppDomain.CurrentDomain.Evidence, appDomainSetup);

                this.SubscribeDomainExitEvent();

                this._wcfServiceHostProxy = this._appDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(WcfServiceHostProxy).FullName) as WcfServiceHostProxy;
                this.IsAppDomainLoaded = true;
                this.SubscribeAllWcfServiceHostProxyEvent();
                this._wcfServiceHostProxy.Initialize(this.AssemblyFile, this._tempConfigFile, this._baseAddress);
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
        /// Method SubscribeDomainExitEvent.
        /// </summary>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        private void SubscribeDomainExitEvent()
        {
            this._appDomain.DomainUnload += this.DomainExit;
            this._appDomain.ProcessExit += this.DomainExit;
            this._appDomain.UnhandledException += this.DomainExit;
        }

        /// <summary>
        /// Method UnSubscribeDomainExitEvent.
        /// </summary>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        private void UnSubscribeDomainExitEvent()
        {
            this._appDomain.DomainUnload -= this.DomainExit;
            this._appDomain.ProcessExit -= this.DomainExit;
            this._appDomain.UnhandledException -= this.DomainExit;
        }

        /// <summary>
        /// Method DomainExit.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void DomainExit(object sender, EventArgs e)
        {
            this.CleanTempWcfConfigFile();
        }

        /// <summary>
        /// Clean up temp wcf config file.
        /// </summary>
        private void CleanTempWcfConfigFile()
        {
            if (File.Exists(this._tempConfigFile))
            {
                try
                {
                    File.Delete(this._tempConfigFile);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                }
            }
        }

        /// <summary>
        /// Method SubscribeAllWcfServiceHostProxyEvent.
        /// </summary>
        private void SubscribeAllWcfServiceHostProxyEvent()
        {
            this._wcfServiceHostProxy.Created += (s, e) => this.RaiseEvent(this.Created, e);
            this._wcfServiceHostProxy.Opening += (s, e) => this.RaiseEvent(this.Opening, e);
            this._wcfServiceHostProxy.Opened += (s, e) => this.RaiseEvent(this.Opened, e);
            this._wcfServiceHostProxy.Closing += (s, e) => this.RaiseEvent(this.Closing, e);
            this._wcfServiceHostProxy.Closed += (s, e) => this.RaiseEvent(this.Closed, e);
            this._wcfServiceHostProxy.Aborting += (s, e) => this.RaiseEvent(this.Aborting, e);
            this._wcfServiceHostProxy.Aborted += (s, e) => this.RaiseEvent(this.Aborted, e);
            this._wcfServiceHostProxy.Restarting += (s, e) => this.RaiseEvent(this.Restarting, e);
            this._wcfServiceHostProxy.Restarted += (s, e) => this.RaiseEvent(this.Restarted, e);
        }

        /// <summary>
        /// Method UnSubscribeAllWcfServiceHostProxyEvent.
        /// </summary>
        private void UnSubscribeAllWcfServiceHostProxyEvent()
        {
            this._wcfServiceHostProxy.Created -= (s, e) => this.RaiseEvent(this.Created, e);
            this._wcfServiceHostProxy.Opening -= (s, e) => this.RaiseEvent(this.Opening, e);
            this._wcfServiceHostProxy.Opened -= (s, e) => this.RaiseEvent(this.Opened, e);
            this._wcfServiceHostProxy.Closing -= (s, e) => this.RaiseEvent(this.Closing, e);
            this._wcfServiceHostProxy.Closed -= (s, e) => this.RaiseEvent(this.Closed, e);
            this._wcfServiceHostProxy.Aborting -= (s, e) => this.RaiseEvent(this.Aborting, e);
            this._wcfServiceHostProxy.Aborted -= (s, e) => this.RaiseEvent(this.Aborted, e);
            this._wcfServiceHostProxy.Restarting -= (s, e) => this.RaiseEvent(this.Restarting, e);
            this._wcfServiceHostProxy.Restarted -= (s, e) => this.RaiseEvent(this.Restarted, e);
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.ServiceModel.WcfServiceHost");
            }
        }
    }
}
