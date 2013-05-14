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
    using System.ServiceModel.Channels;
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
        /// Field _serviceType.
        /// </summary>
        private Type _serviceType;

        /// <summary>
        /// Field _baseAddress.
        /// </summary>
        private string _baseAddress;

        /// <summary>
        /// Field _bindingType.
        /// </summary>
        private Type _bindingType;

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
        /// <param name="autoOpen">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(string assemblyFile, bool autoOpen = false)
        {
            this.Initialize(assemblyFile, string.Empty, string.Empty);

            if (autoOpen)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="autoOpen">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(string assemblyFile, string configFile, bool autoOpen = false)
        {
            this.Initialize(assemblyFile, configFile, string.Empty);

            if (autoOpen)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        /// <param name="autoOpen">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(string assemblyFile, string configFile, string baseAddress, bool autoOpen = false)
        {
            this.Initialize(assemblyFile, configFile, baseAddress);

            if (autoOpen)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="address">Wcf service address.</param>
        /// <param name="autoOpen">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(string assemblyFile, Type bindingType, string address, bool autoOpen = false)
        {
            this.Initialize(assemblyFile, bindingType, address);

            if (autoOpen)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="address">Wcf service address.</param>
        /// <param name="autoOpen">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(Type serviceType, Type bindingType, string address, bool autoOpen = false)
        {
            this.Initialize(serviceType, bindingType, address);

            if (autoOpen)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="address">Wcf service address.</param>
        /// <param name="autoOpen">true if immediately open wcf service; otherwise, false.</param>
        public WcfServiceHost(Type serviceType, Binding binding, string address, bool autoOpen = false)
        {
            this.Initialize(serviceType, binding, address);

            if (autoOpen)
            {
                this.Open();
            }
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
        public void Initialize(string assemblyFile, string configFile, string baseAddress)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("The specified file does not exist.", assemblyFile);
            }

            if (!string.IsNullOrEmpty(configFile) && !File.Exists(configFile))
            {
                throw new FileNotFoundException("The specified file does not exist.", configFile);
            }

            if (!string.IsNullOrEmpty(baseAddress) && !Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
            {
                throw new UriFormatException(baseAddress);
            }

            this.CleanServiceConfig();

            this.AssemblyFile = assemblyFile;

            this._serviceType = null;

            this.ConfigFile = string.IsNullOrEmpty(configFile) ? AppDomain.CurrentDomain.SetupInformation.ConfigurationFile : configFile;

            this._baseAddress = baseAddress;

            this._bindingType = null;

            this.CreateDomain();
        }

        /// <summary>
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="address">The address for the endpoint added.</param>
        public void Initialize(string assemblyFile, Type bindingType, string address)
        {
            if (string.IsNullOrEmpty(assemblyFile))
            {
                throw new ArgumentNullException("assemblyFile");
            }

            if (!File.Exists(assemblyFile))
            {
                throw new FileNotFoundException("The specified file does not exist.", assemblyFile);
            }

            if (bindingType == null)
            {
                throw new ArgumentNullException("bindingType");
            }

            if (!bindingType.IsSubclassOf(typeof(Binding)))
            {
                throw new ArgumentException("The parameter bindingType is not a Binding type.", "bindingType");
            }

            if (string.IsNullOrEmpty(address) || !Uri.IsWellFormedUriString(address, UriKind.Absolute))
            {
                throw new UriFormatException(address ?? string.Empty);
            }

            this.CleanServiceConfig();

            this.AssemblyFile = assemblyFile;

            this._serviceType = null;

            this.ConfigFile = null;

            this._baseAddress = address;

            this._bindingType = bindingType;

            this.CreateDomain();
        }

        /// <summary>
        /// Create an isolated AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="address">The address for the endpoint added.</param>
        public void Initialize(Type serviceType, Type bindingType, string address)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (!WcfServiceHostType.IsWcfServiceClass(serviceType))
            {
                throw new ArgumentException("The parameter serviceType is not a Wcf service.", "serviceType");
            }

            if (bindingType == null)
            {
                throw new ArgumentNullException("bindingType");
            }

            if (!bindingType.IsSubclassOf(typeof(Binding)))
            {
                throw new ArgumentException("The parameter bindingType is not a Binding type.", "bindingType");
            }

            if (string.IsNullOrEmpty(address) || !Uri.IsWellFormedUriString(address, UriKind.Absolute))
            {
                throw new UriFormatException(address ?? string.Empty);
            }

            this.CleanServiceConfig();

            this.AssemblyFile = null;

            this._serviceType = serviceType;

            this.ConfigFile = null;

            this._baseAddress = address;

            this._bindingType = bindingType;

            this.CreateDomain();
        }

        /// <summary>
        /// Use current AppDomain to host Wcf service.
        /// </summary>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="address">The address for the endpoint added.</param>
        public void Initialize(Type serviceType, Binding binding, string address)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (!WcfServiceHostType.IsWcfServiceClass(serviceType))
            {
                throw new ArgumentException("The parameter serviceType is not a Wcf service.", "serviceType");
            }

            if (binding == null)
            {
                throw new ArgumentNullException("binding");
            }

            if (string.IsNullOrEmpty(address) || !Uri.IsWellFormedUriString(address, UriKind.Absolute))
            {
                throw new UriFormatException(address ?? string.Empty);
            }

            this.CleanServiceConfig();

            this.AssemblyFile = null;

            this._serviceType = serviceType;

            this.ConfigFile = null;

            this._baseAddress = address;

            this._bindingType = binding.GetType();

            try
            {
                this.CleanTempWcfConfigFile();

                this._wcfServiceHostProxy = new WcfServiceHostProxy();

                this.SubscribeAllWcfServiceHostProxyEvent();

                this._wcfServiceHostProxy.Initialize(this._serviceType, binding, this._baseAddress);
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
                throw;
            }
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
                this.UnSubscribeDomainExitEvent();
                AppDomain.Unload(this._appDomain);
                this.IsAppDomainLoaded = false;
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

            if (string.IsNullOrEmpty(sourceFileName))
            {
                return result;
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
                    this.UnSubscribeDomainExitEvent();
                    AppDomain.Unload(this._appDomain);
                    this.IsAppDomainLoaded = false;
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
                this.CleanTempWcfConfigFile();

                AppDomainSetup appDomainSetup = new AppDomainSetup();
                appDomainSetup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
                appDomainSetup.ApplicationName = Path.GetFileNameWithoutExtension(this.AssemblyFile) ?? this._serviceType.FullName;
                appDomainSetup.ConfigurationFile = this._bindingType == null ? this._tempConfigFile = this.GetTempWcfConfigFile(this.ConfigFile, this._baseAddress) : string.Empty;
                appDomainSetup.LoaderOptimization = LoaderOptimization.MultiDomainHost;
                appDomainSetup.ShadowCopyFiles = "true";
                appDomainSetup.ShadowCopyDirectories = appDomainSetup.ApplicationBase;

                this._appDomain = AppDomain.CreateDomain(appDomainSetup.ApplicationName, AppDomain.CurrentDomain.Evidence, appDomainSetup);

                this.SubscribeDomainExitEvent();

                this._wcfServiceHostProxy = this._appDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(WcfServiceHostProxy).FullName) as WcfServiceHostProxy;
                this.IsAppDomainLoaded = true;
                this.SubscribeAllWcfServiceHostProxyEvent();

                if (this._serviceType == null)
                {
                    this._wcfServiceHostProxy.Initialize(this.AssemblyFile, this._tempConfigFile, this._bindingType, this._baseAddress);
                }
                else
                {
                    this._wcfServiceHostProxy.Initialize(this._serviceType, this._bindingType, this._baseAddress);
                }

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
        /// Clean up wcf service config.
        /// </summary>
        private void CleanServiceConfig()
        {
            this._serviceType = null;
            this._baseAddress = string.Empty;
            this._bindingType = null;
            this.AssemblyFile = string.Empty;
            this.ConfigFile = string.Empty;
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
