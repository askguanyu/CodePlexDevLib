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
        private Dictionary<string, WcfServiceHostState> _stateList = new Dictionary<string, WcfServiceHostState>();

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
        /// Open Service Host
        /// </summary>
        public void Open()
        {
            if (this._serviceHostList.Count > 0)
            {
                foreach (var serviceHost in this._serviceHostList)
                {
                    if (serviceHost.State == CommunicationState.Created ||
                        serviceHost.State == CommunicationState.Closed)
                    {
                        this.RaiseEvent(Opening, serviceHost.Description.Name, WcfServiceHostState.Opening);

                        try
                        {
                            serviceHost.Open();
                            Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostOpenStringFormat, serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses[0].AbsoluteUri));
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostOpenExceptionStringFormat, e.Source, e.Message));
                        }

                        this.RaiseEvent(Opened, serviceHost.Description.Name, WcfServiceHostState.Opened);
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
                        this.RaiseEvent(Closing, serviceHost.Description.Name, WcfServiceHostState.Closing);

                        try
                        {
                            serviceHost.Close();
                            Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostCloseStringFormat, serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses[0].AbsoluteUri));
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostCloseExceptionStringFormat, e.Source, e.Message));
                        }

                        this.RaiseEvent(Closed, serviceHost.Description.Name, WcfServiceHostState.Closed);
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
                    this.RaiseEvent(Aborting, serviceHost.Description.Name, WcfServiceHostState.Aborting);

                    try
                    {
                        serviceHost.Abort();
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostAbortStringFormat, serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses[0].AbsoluteUri));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostAbortExceptionStringFormat, e.Source, e.Message));
                    }

                    this.RaiseEvent(Aborted, serviceHost.Description.Name, WcfServiceHostState.Aborted);
                }
            }
        }

        /// <summary>
        /// Restart Service Host
        /// </summary>
        public void Restart()
        {
            if (this._serviceHostList.Count > 0)
            {
                foreach (var serviceHost in this._serviceHostList)
                {
                    this.RaiseEvent(Restarting, serviceHost.Description.Name, WcfServiceHostState.Restarting);

                    try
                    {
                        serviceHost.Abort();
                        serviceHost.Open();
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostRestartStringFormat, serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses[0].AbsoluteUri));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostRestartExceptionStringFormat, e.Source, e.Message));
                    }

                    this.RaiseEvent(Restarted, serviceHost.Description.Name, WcfServiceHostState.Restarted);
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
        ///
        /// </summary>
        /// <returns></returns>
        public List<ServiceHost> GetServiceHostList()
        {
            return this._serviceHostList;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, WcfServiceHostState> GetStateList()
        {
            return this._stateList;
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
        /// <param name="assemblyFile"></param>
        /// <param name="configFile"></param>
        public void Init(string assemblyFile, string configFile = null)
        {
            this._serviceHostList.Clear();
            this._stateList.Clear();

            foreach (Type serviceType in WcfServiceHostType.LoadFile(assemblyFile, configFile ?? string.Format("{0}.config", assemblyFile)))
            {
                try
                {
                    ServiceHost serviceHost = new ServiceHost(serviceType);
                    this._serviceHostList.Add(serviceHost);
                    this._stateList.Add(serviceHost.Description.ServiceType.FullName, WcfServiceHostState.Closed);
                    Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostInitStringFormat, serviceHost.Description.ServiceType.FullName, serviceHost.BaseAddresses[0].AbsoluteUri));
                }
                catch (Exception e)
                {
                    Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostInitExceptionStringFormat, e.Source, e.Message));
                }
            }

            this.RaiseEvent(Created, assemblyFile, WcfServiceHostState.Created);
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
                    this.Close();
                    foreach (IDisposable item in this._serviceHostList)
                    {
                        item.Dispose();
                    }

                    this._serviceHostList.Clear();
                    this._stateList.Clear();
                }
            }

            // free native resources if there are any
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <param name="e"></param>
        private void RaiseEvent(EventHandler<WcfServiceHostEventArgs> eventHandler, string wcfServiceName, WcfServiceHostState state)
        {
            this._stateList[wcfServiceName] = state;

            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<WcfServiceHostEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, new WcfServiceHostEventArgs(wcfServiceName, state));
            }
        }
    }
}
