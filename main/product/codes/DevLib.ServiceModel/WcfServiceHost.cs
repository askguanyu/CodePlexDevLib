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
        ///
        /// </summary>
        private List<ServiceHost> _serviceHostList = new List<ServiceHost>();

        /// <summary>
        /// Constructor of WcfServiceHost, host Wcf service in current AppDomain. Use Initialize method to initialize wcf service.
        /// </summary>
        public WcfServiceHost()
        {
        }

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Created;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Opening;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Opened;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Closing;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Closed;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Aborting;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Aborted;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<WcfServiceHostEventArgs> Restarting;

        /// <summary>
        ///
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
                throw new ArgumentException("The file does not exist.", assemblyFile);
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
                    throw new ArgumentException("The file does not exist.", configFile);
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void Open()
        {
            if (this._serviceHostList.Count > 0)
            {
                for (int i = 0; i < _serviceHostList.Count; i++)
                {
                    if (_serviceHostList[i].State != CommunicationState.Opening ||
                        _serviceHostList[i].State != CommunicationState.Opened)
                    {
                        this.RaiseEvent(Opening, _serviceHostList[i].Description.Name, WcfServiceHostStateEnum.Opening);
                        try
                        {
                            if (_serviceHostList[i].State != CommunicationState.Created)
                            {
                                this._serviceHostList[i] = new ServiceHost(_serviceHostList[i].Description.ServiceType);
                            }

                            _serviceHostList[i].Open();
                            this.RaiseEvent(Opened, _serviceHostList[i].Description.Name, WcfServiceHostStateEnum.Opened);
                            Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceedStringFormat, "DevLib.ServiceModel.WcfServiceHost.Open", this._serviceHostList[i].Description.ServiceType.FullName, this._serviceHostList[i].BaseAddresses.Count > 0 ? this._serviceHostList[i].BaseAddresses[0].AbsoluteUri : string.Empty));
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "DevLib.ServiceModel.WcfServiceHost.Open", e.Source, e.Message, e.StackTrace));
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
            if (this._serviceHostList.Count > 0)
            {
                foreach (var serviceHost in this._serviceHostList)
                {
                    if (serviceHost.State == CommunicationState.Opened)
                    {
                        this.RaiseEvent(Closing, serviceHost.Description.Name, WcfServiceHostStateEnum.Closing);
                        try
                        {
                            serviceHost.Close();
                            this.RaiseEvent(Closed, serviceHost.Description.Name, WcfServiceHostStateEnum.Closed);
                            Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceedStringFormat, "DevLib.ServiceModel.WcfServiceHost.Close", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "DevLib.ServiceModel.WcfServiceHost.Close", e.Source, e.Message, e.StackTrace));
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Abort Service Host.
        /// </summary>
        public void Abort()
        {
            if (this._serviceHostList.Count > 0)
            {
                foreach (var serviceHost in this._serviceHostList)
                {
                    this.RaiseEvent(Aborting, serviceHost.Description.Name, WcfServiceHostStateEnum.Aborting);
                    try
                    {
                        serviceHost.Abort();
                        this.RaiseEvent(Aborted, serviceHost.Description.Name, WcfServiceHostStateEnum.Aborted);
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceedStringFormat, "DevLib.ServiceModel.WcfServiceHost.Abort", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "DevLib.ServiceModel.WcfServiceHost.Abort", e.Source, e.Message, e.StackTrace));
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Restart Service Host.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void Restart()
        {
            if (this._serviceHostList.Count > 0)
            {
                for (int i = 0; i < _serviceHostList.Count; i++)
                {
                    this.RaiseEvent(Restarting, _serviceHostList[i].Description.ServiceType.FullName, WcfServiceHostStateEnum.Restarting);
                    try
                    {
                        this._serviceHostList[i].Abort();
                        this._serviceHostList[i] = new ServiceHost(_serviceHostList[i].Description.ServiceType);
                        this._serviceHostList[i].Open();
                        this.RaiseEvent(Restarted, this._serviceHostList[i].Description.Name, WcfServiceHostStateEnum.Restarted);
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceedStringFormat, "DevLib.ServiceModel.WcfServiceHost.Restart", this._serviceHostList[i].Description.ServiceType.FullName, this._serviceHostList[i].BaseAddresses.Count > 0 ? this._serviceHostList[i].BaseAddresses[0].AbsoluteUri : string.Empty));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "DevLib.ServiceModel.WcfServiceHost.Restart", e.Source, e.Message, e.StackTrace));
                        throw;
                    }
                }
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
        /// Get service host list.
        /// </summary>
        /// <returns></returns>
        public List<ServiceHost> GetServiceHostList()
        {
            return this._serviceHostList;
        }

        /// <summary>
        /// Get service state list.
        /// </summary>
        /// <returns></returns>
        public List<WcfServiceHostInfo> GetHostInfoList()
        {
            List<WcfServiceHostInfo> result = new List<WcfServiceHostInfo>();

            foreach (var item in this._serviceHostList)
            {
                result.Add(new WcfServiceHostInfo() { ServiceType = item.Description.ServiceType.FullName, BaseAddress = item.BaseAddresses[0].AbsoluteUri, State = item.State });
            }

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
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
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceedStringFormat, "DevLib.ServiceModel.WcfServiceHost.Init", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "DevLib.ServiceModel.WcfServiceHost.Init", e.Source, e.Message, e.StackTrace));
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "DevLib.ServiceModel.WcfServiceHost.Init", e.Source, e.Message, e.StackTrace));
                throw;
            }

            this.RaiseEvent(Created, assemblyFile, WcfServiceHostStateEnum.Created);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (this._serviceHostList != null)
                {
                    this.Abort();
                    foreach (IDisposable item in this._serviceHostList)
                    {
                        item.Dispose();
                    }

                    this._serviceHostList.Clear();
                }
            }

            // free native resources if there are any
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <param name="e"></param>
        private void RaiseEvent(EventHandler<WcfServiceHostEventArgs> eventHandler, string wcfServiceName, WcfServiceHostStateEnum state)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<WcfServiceHostEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, new WcfServiceHostEventArgs(wcfServiceName, state));
            }
        }
    }
}
