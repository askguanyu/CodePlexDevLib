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
    using System.ServiceModel;
    using System.Threading;

    /// <summary>
    /// Wcf ServiceHost
    /// </summary>
    public class WcfServiceHost : MarshalByRefObject, IDisposable
    {
        /// <summary>
        ///
        /// </summary>
        private List<ServiceHost> _serviceHostList = new List<ServiceHost>();

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
        /// Initialize Wcf service
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file</param>
        /// <param name="configFile">Wcf service config file</param>
        public void Init(string assemblyFile, string configFile)
        {
            this._serviceHostList.Clear();

            try
            {
                foreach (Type serviceType in WcfServiceHostType.LoadFile(assemblyFile, configFile ?? string.Format("{0}.config", assemblyFile)))
                {
                    try
                    {
                        ServiceHost serviceHost = new ServiceHost(serviceType);
                        this._serviceHostList.Add(serviceHost);
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceedStringFormat, "WcfServiceHost.Init", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses[0].AbsoluteUri));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "WcfServiceHost.Init", e.Source, e.Message, e.StackTrace));
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "WcfServiceHost.Init", e.Source, e.Message, e.StackTrace));
                throw;
            }

            this.RaiseEvent(Created, assemblyFile, WcfServiceHostStateEnum.Created);
        }

        /// <summary>
        /// Open Service Host
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
                            Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceedStringFormat, "WcfServiceHost.Open", _serviceHostList[i].Description.ServiceType.FullName, _serviceHostList[i].BaseAddresses[0].AbsoluteUri));
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "WcfServiceHost.Open", e.Source, e.Message, e.StackTrace));
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Close Service Host
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
                            Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceedStringFormat, "WcfServiceHost.Close", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses[0].AbsoluteUri));
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "WcfServiceHost.Close", e.Source, e.Message, e.StackTrace));
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Abort Service Host
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
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceedStringFormat, "WcfServiceHost.Abort", serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses[0].AbsoluteUri));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "WcfServiceHost.Abort", e.Source, e.Message, e.StackTrace));
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Restart Service Host
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
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostSucceedStringFormat, "WcfServiceHost.Restart", this._serviceHostList[i].Description.ServiceType.FullName, this._serviceHostList[i].BaseAddresses[0].AbsoluteUri));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.ExceptionStringFormat, "WcfServiceHost.Restart", e.Source, e.Message, e.StackTrace));
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
        /// Get service host list
        /// </summary>
        /// <returns></returns>
        public List<ServiceHost> GetServiceHostList()
        {
            return this._serviceHostList;
        }

        /// <summary>
        /// Get service state list
        /// </summary>
        /// <returns></returns>
        public List<WcfServiceHostState> GetStateList()
        {
            List<WcfServiceHostState> result = new List<WcfServiceHostState>();

            foreach (var item in this._serviceHostList)
            {
                result.Add(new WcfServiceHostState() { ServiceType = item.Description.ServiceType.FullName, BaseAddress = item.BaseAddresses[0].AbsoluteUri, State = item.State });
            }

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
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
