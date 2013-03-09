//-----------------------------------------------------------------------
// <copyright file="WcfServiceHost.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.Threading;

    /// <summary>
    /// Wcf ServiceHost.
    /// </summary>
    [Serializable]
    public sealed class WcfServiceHost : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Field _serviceHostList.
        /// </summary>
        private List<ServiceHost> _serviceHostList = new List<ServiceHost>();

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// Default constructor of WcfServiceHost, host Wcf service in current AppDomain. Use Initialize method to initialize Wcf service.
        /// </summary>
        public WcfServiceHost()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHost" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="configFile">Wcf service config file.</param>
        public WcfServiceHost(string assemblyFile, string configFile = null)
        {
            this.Initialize(assemblyFile, configFile);
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
        public event EventHandler<WcfServiceHostEventArgs> Created;

        /// <summary>
        /// Event Opening.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Opening;

        /// <summary>
        /// Event Opened.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Opened;

        /// <summary>
        /// Event Closing.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Closing;

        /// <summary>
        /// Event Closed.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Closed;

        /// <summary>
        /// Event Aborting.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Aborting;

        /// <summary>
        /// Event Aborted.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Aborted;

        /// <summary>
        /// Event Restarting.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Restarting;

        /// <summary>
        /// Event Restarted.
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Restarted;

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
        /// Initialize Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="configFile">Wcf service config file.</param>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public void Initialize(string assemblyFile, string configFile = null)
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
                this.Init(this.AssemblyFile, this.ConfigFile);
            }
            else
            {
                if (!File.Exists(configFile))
                {
                    throw new FileNotFoundException("The specified file does not exist.", configFile);
                }

                this.ConfigFile = configFile;
                string originalConfigFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

                try
                {
                    AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", this.ConfigFile);
                    this.Init(this.AssemblyFile, this.ConfigFile);
                }
                finally
                {
                    AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", originalConfigFile);
                }
            }
        }

        /// <summary>
        /// Open Service Host.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public void Open()
        {
            this.CheckDisposed();

            if (this._serviceHostList.Count > 0)
            {
                for (int i = 0; i < this._serviceHostList.Count; i++)
                {
                    if (this._serviceHostList[i].State != CommunicationState.Opening ||
                        this._serviceHostList[i].State != CommunicationState.Opened)
                    {
                        this.RaiseEvent(this.Opening, this._serviceHostList[i].Description.Name, WcfServiceHostStateEnum.Opening);
                        try
                        {
                            if (this._serviceHostList[i].State != CommunicationState.Created)
                            {
                                this._serviceHostList[i] = new ServiceHost(this._serviceHostList[i].Description.ServiceType);
                            }

                            this._serviceHostList[i].Open();
                            this.RaiseEvent(this.Opened, this._serviceHostList[i].Description.Name, WcfServiceHostStateEnum.Opened);
                            Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHost.Open", this._serviceHostList[i].Description.ServiceType.FullName, this._serviceHostList[i].BaseAddresses.Count > 0 ? this._serviceHostList[i].BaseAddresses[0].AbsoluteUri : string.Empty));
                        }
                        catch (Exception e)
                        {
                            ExceptionHandler.Log(e);
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Close Service Host.
        /// </summary>
        public void Close()
        {
            this.CheckDisposed();

            if (this._serviceHostList.Count > 0)
            {
                foreach (var serviceHost in this._serviceHostList)
                {
                    this.RaiseEvent(this.Closing, serviceHost.Description.Name, WcfServiceHostStateEnum.Closing);
                    try
                    {
                        serviceHost.Close();
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHost.Close", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                    }
                    catch (Exception e)
                    {
                        serviceHost.Abort();
                        ExceptionHandler.Log(e);
                    }

                    this.RaiseEvent(this.Closed, serviceHost.Description.Name, WcfServiceHostStateEnum.Closed);
                }
            }
        }

        /// <summary>
        /// Abort Service Host.
        /// </summary>
        public void Abort()
        {
            this.CheckDisposed();

            if (this._serviceHostList.Count > 0)
            {
                foreach (var serviceHost in this._serviceHostList)
                {
                    this.RaiseEvent(this.Aborting, serviceHost.Description.Name, WcfServiceHostStateEnum.Aborting);
                    try
                    {
                        serviceHost.Abort();
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHost.Abort", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);
                    }

                    this.RaiseEvent(this.Aborted, serviceHost.Description.Name, WcfServiceHostStateEnum.Aborted);
                }
            }
        }

        /// <summary>
        /// Restart Service Host.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public void Restart()
        {
            this.CheckDisposed();

            if (this._serviceHostList.Count > 0)
            {
                for (int i = 0; i < this._serviceHostList.Count; i++)
                {
                    this.RaiseEvent(this.Restarting, this._serviceHostList[i].Description.ServiceType.FullName, WcfServiceHostStateEnum.Restarting);
                    try
                    {
                        this._serviceHostList[i].Abort();
                        this._serviceHostList[i] = new ServiceHost(this._serviceHostList[i].Description.ServiceType);
                        this._serviceHostList[i].Open();
                        this.RaiseEvent(this.Restarted, this._serviceHostList[i].Description.Name, WcfServiceHostStateEnum.Restarted);
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHost.Restart", this._serviceHostList[i].Description.ServiceType.FullName, this._serviceHostList[i].BaseAddresses.Count > 0 ? this._serviceHostList[i].BaseAddresses[0].AbsoluteUri : string.Empty));
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Get service host list.
        /// </summary>
        /// <returns>Instance of List.</returns>
        public List<ServiceHost> GetServiceHostList()
        {
            this.CheckDisposed();

            return this._serviceHostList;
        }

        /// <summary>
        /// Get service state list.
        /// </summary>
        /// <returns>Instance of List.</returns>
        public List<WcfServiceHostInfo> GetHostInfoList()
        {
            this.CheckDisposed();

            List<WcfServiceHostInfo> result = new List<WcfServiceHostInfo>();

            foreach (var item in this._serviceHostList)
            {
                result.Add(new WcfServiceHostInfo() { ServiceType = item.Description.ServiceType.FullName, BaseAddress = item.BaseAddresses[0].AbsoluteUri, State = item.State });
            }

            return result;
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
        /// Initialize Wcf service.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="configFile">Wcf service config file.</param>
        internal void Init(string assemblyFile, string configFile)
        {
            this._serviceHostList.Clear();

            try
            {
                // To use config file to setup ServiceHost, wcf service and config file should be in a same domain.
                foreach (Type serviceType in WcfServiceHostType.LoadFile(assemblyFile, configFile))
                {
                    try
                    {
                        ServiceHost serviceHost = new ServiceHost(serviceType);
                        this._serviceHostList.Add(serviceHost);
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHost.Init", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
                throw;
            }

            this.RaiseEvent(this.Created, assemblyFile, WcfServiceHostStateEnum.Created);
        }

        /// <summary>
        /// Method RaiseEvent.
        /// </summary>
        /// <param name="eventHandler">Instance of EventHandler.</param>
        /// <param name="wcfServiceName">String of Wcf Service Name.</param>
        /// <param name="state">Instance of WcfServiceHostStateEnum.</param>
        private void RaiseEvent(EventHandler<WcfServiceHostEventArgs> eventHandler, string wcfServiceName, WcfServiceHostStateEnum state)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<WcfServiceHostEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, new WcfServiceHostEventArgs(wcfServiceName, state));
            }
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

                if (this._serviceHostList != null && this._serviceHostList.Count > 0)
                {
                    foreach (var serviceHost in this._serviceHostList)
                    {
                        if (serviceHost != null)
                        {
                            try
                            {
                                serviceHost.Close();
                            }
                            catch
                            {
                                serviceHost.Abort();
                            }
                        }
                    }

                    this._serviceHostList.Clear();
                    this._serviceHostList = null;
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
