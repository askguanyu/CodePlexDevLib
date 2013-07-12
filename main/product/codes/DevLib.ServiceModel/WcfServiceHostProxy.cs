//-----------------------------------------------------------------------
// <copyright file="WcfServiceHostProxy.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Threading;

    /// <summary>
    /// Class WcfServiceHostProxy.
    /// </summary>
    [Serializable]
    internal sealed class WcfServiceHostProxy : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Field _baseAddress.
        /// </summary>
        private readonly string _assemblyFile;

        /// <summary>
        /// Field _serviceType.
        /// </summary>
        private readonly Type _serviceType;

        /// <summary>
        /// Field _binding.
        /// </summary>
        [NonSerialized]
        private readonly Binding _binding;

        /// <summary>
        /// Field _bindingType.
        /// </summary>
        private readonly Type _bindingType;

        /// <summary>
        /// Field _configFile.
        /// </summary>
        private readonly string _configFile;

        /// <summary>
        /// Field _baseAddress.
        /// </summary>
        private readonly string _baseAddress;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Field _serviceHostList.
        /// </summary>
        private List<ServiceHost> _serviceHostList = new List<ServiceHost>();

        /// <summary>
        /// Field _isOpened.
        /// </summary>
        private bool _isOpened = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHostProxy" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public WcfServiceHostProxy(string assemblyFile, Type serviceType, Type bindingType, string configFile, string baseAddress)
            : this(assemblyFile, serviceType, null, bindingType, configFile, baseAddress)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHostProxy" /> class.
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file.</param>
        /// <param name="serviceType">Wcf service type.</param>
        /// <param name="binding">The <see cref="T:System.ServiceModel.Channels.Binding" /> for the endpoint.</param>
        /// <param name="bindingType">The type of <see cref="T:System.ServiceModel.Channels.Binding" /> for the service.</param>
        /// <param name="configFile">Wcf service config file.</param>
        /// <param name="baseAddress">Wcf service base address.</param>
        public WcfServiceHostProxy(string assemblyFile, Type serviceType, Binding binding, Type bindingType, string configFile, string baseAddress)
        {
            this._assemblyFile = assemblyFile;
            this._serviceType = serviceType;
            this._binding = binding;
            this._bindingType = bindingType;
            this._configFile = configFile;
            this._baseAddress = baseAddress;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="WcfServiceHostProxy" /> class.
        /// </summary>
        ~WcfServiceHostProxy()
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
        /// Open Service Host.
        /// </summary>
        public void Open()
        {
            this.CheckDisposed();

            if (this._isOpened)
            {
                return;
            }

            this.Initialize();

            if (this._serviceHostList.Count > 0)
            {
                try
                {
                    foreach (ServiceHost serviceHost in this._serviceHostList)
                    {
                        if (!(serviceHost.State == CommunicationState.Opening || serviceHost.State == CommunicationState.Opened))
                        {
                            this.RaiseEvent(this.Opening, serviceHost.Description.Name, WcfServiceHostStateEnum.Opening);
                            serviceHost.Open();
                            this.RaiseEvent(this.Opened, serviceHost.Description.Name, WcfServiceHostStateEnum.Opened);
                            Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHostProxy.Open", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                        }
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    throw;
                }
            }

            this._isOpened = true;
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
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHostProxy.Close", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                    }
                    catch (Exception e)
                    {
                        serviceHost.Abort();
                        ExceptionHandler.Log(e);
                    }

                    this.RaiseEvent(this.Closed, serviceHost.Description.Name, WcfServiceHostStateEnum.Closed);
                }
            }

            this._isOpened = false;
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
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHostProxy.Abort", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);
                    }

                    this.RaiseEvent(this.Aborted, serviceHost.Description.Name, WcfServiceHostStateEnum.Aborted);
                }
            }

            this._isOpened = false;
        }

        /// <summary>
        /// Restart Service Host.
        /// </summary>
        public void Restart()
        {
            this.CheckDisposed();

            this.Initialize();

            if (this._serviceHostList.Count > 0)
            {
                try
                {
                    foreach (ServiceHost serviceHost in this._serviceHostList)
                    {
                        if (!(serviceHost.State == CommunicationState.Opening || serviceHost.State == CommunicationState.Opened))
                        {
                            this.RaiseEvent(this.Restarting, serviceHost.Description.Name, WcfServiceHostStateEnum.Restarting);
                            serviceHost.Open();
                            this.RaiseEvent(this.Restarted, serviceHost.Description.Name, WcfServiceHostStateEnum.Restarted);
                            Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHostProxy.Restart", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                        }
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    throw;
                }
            }

            this._isOpened = true;
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
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="WcfServiceHostProxy" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initialize Wcf service.
        /// </summary>
        private void Initialize()
        {
            if (this._serviceHostList.Count > 0)
            {
                foreach (ServiceHost serviceHost in this._serviceHostList)
                {
                    try
                    {
                        serviceHost.Abort();
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);
                    }
                }
            }

            this._serviceHostList.Clear();

            if (File.Exists(this._assemblyFile))
            {
                if (File.Exists(this._configFile))
                {
                    try
                    {
                        foreach (Type serviceType in WcfServiceType.LoadFile(this._assemblyFile, this._configFile))
                        {
                            ServiceHost serviceHost = string.IsNullOrEmpty(this._baseAddress) ? new ServiceHost(serviceType) : new ServiceHost(serviceType, new Uri(this._baseAddress));
                            this._serviceHostList.Add(serviceHost);
                            Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHostProxy.Initialize", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);
                        throw;
                    }
                }
                else
                {
                    Uri baseAddressUri = new Uri(this._baseAddress);
                    Binding binding = this._binding ?? WcfServiceType.GetBinding(this._bindingType);

                    try
                    {
                        foreach (Type serviceType in WcfServiceType.LoadFile(this._assemblyFile))
                        {
                            foreach (Type serviceContract in WcfServiceType.GetServiceContract(serviceType))
                            {
                                ServiceHost serviceHost = new ServiceHost(serviceType, baseAddressUri);
                                serviceHost.Description.Endpoints.Clear();
                                serviceHost.AddServiceEndpoint(serviceContract, binding, baseAddressUri);

                                if (baseAddressUri.Scheme.Equals(Uri.UriSchemeHttp))
                                {
                                    ServiceMetadataBehavior serviceMetadataBehavior = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();

                                    if (serviceMetadataBehavior == null)
                                    {
                                        serviceMetadataBehavior = new ServiceMetadataBehavior();

                                        serviceHost.Description.Behaviors.Add(serviceMetadataBehavior);
                                    }

                                    serviceMetadataBehavior.HttpGetEnabled = true;
                                }

                                foreach (var endpoint in serviceHost.Description.Endpoints)
                                {
                                    ContractDescription contractDescription = endpoint.Contract;

                                    foreach (var operationDescription in contractDescription.Operations)
                                    {
                                        DataContractSerializerOperationBehavior serializerBehavior = operationDescription.Behaviors.Find<DataContractSerializerOperationBehavior>();

                                        if (serializerBehavior == null)
                                        {
                                            serializerBehavior = new DataContractSerializerOperationBehavior(operationDescription);

                                            operationDescription.Behaviors.Add(serializerBehavior);
                                        }

                                        serializerBehavior.MaxItemsInObjectGraph = int.MaxValue;
                                        serializerBehavior.IgnoreExtensionDataObject = true;
                                    }
                                }

                                this._serviceHostList.Add(serviceHost);
                                Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHostProxy.Initialize", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);
                        throw;
                    }
                }
            }
            else
            {
                if (File.Exists(this._configFile))
                {
                    try
                    {
                        ServiceHost serviceHost = string.IsNullOrEmpty(this._baseAddress) ? new ServiceHost(this._serviceType) : new ServiceHost(this._serviceType, new Uri(this._baseAddress));
                        this._serviceHostList.Add(serviceHost);
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHostProxy.Initialize", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
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
                        Uri baseAddressUri = new Uri(this._baseAddress);
                        Binding binding = this._binding ?? WcfServiceType.GetBinding(this._bindingType);

                        foreach (Type serviceContract in WcfServiceType.GetServiceContract(this._serviceType))
                        {
                            ServiceHost serviceHost = new ServiceHost(this._serviceType, baseAddressUri);
                            serviceHost.Description.Endpoints.Clear();
                            serviceHost.AddServiceEndpoint(serviceContract, binding, baseAddressUri);

                            if (baseAddressUri.Scheme.Equals(Uri.UriSchemeHttp))
                            {
                                ServiceMetadataBehavior serviceMetadataBehavior = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();

                                if (serviceMetadataBehavior == null)
                                {
                                    serviceMetadataBehavior = new ServiceMetadataBehavior();

                                    serviceHost.Description.Behaviors.Add(serviceMetadataBehavior);
                                }

                                serviceMetadataBehavior.HttpGetEnabled = true;
                            }

                            foreach (var endpoint in serviceHost.Description.Endpoints)
                            {
                                ContractDescription contractDescription = endpoint.Contract;

                                foreach (var operationDescription in contractDescription.Operations)
                                {
                                    DataContractSerializerOperationBehavior serializerBehavior = operationDescription.Behaviors.Find<DataContractSerializerOperationBehavior>();

                                    if (serializerBehavior == null)
                                    {
                                        serializerBehavior = new DataContractSerializerOperationBehavior(operationDescription);

                                        operationDescription.Behaviors.Add(serializerBehavior);
                                    }

                                    serializerBehavior.MaxItemsInObjectGraph = int.MaxValue;
                                    serializerBehavior.IgnoreExtensionDataObject = true;
                                }
                            }

                            this._serviceHostList.Add(serviceHost);
                            Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceededStringFormat, "DevLib.ServiceModel.WcfServiceHostProxy.Initialize", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses.Count > 0 ? serviceHost.BaseAddresses[0].AbsoluteUri : string.Empty));
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);
                        throw;
                    }
                }
            }

            this.RaiseEvent(this.Created, this._assemblyFile ?? this._serviceType.Name, WcfServiceHostStateEnum.Created);
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
        /// Releases all resources used by the current instance of the <see cref="WcfServiceHostProxy" /> class.
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
                throw new ObjectDisposedException("DevLib.ServiceModel.WcfServiceHostProxy");
            }
        }
    }
}
