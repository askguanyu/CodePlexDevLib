//-----------------------------------------------------------------------
// <copyright file="WcfIsolatedServiceHost.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;

    /// <summary>
    /// Wcf Isolated ServiceHost
    /// </summary>
    public class WcfIsolatedServiceHost : MarshalByRefObject, IDisposable
    {
        /// <summary>
        ///
        /// </summary>
        private AppDomain _appDomain;

        /// <summary>
        ///
        /// </summary>
        private WcfServiceHost _wcfServiceHost;

        /// <summary>
        /// Constructor of WcfIsolatedServiceHost, create an isolated AppDomain to host Wcf service
        /// </summary>
        /// <param name="assemblyFile">Wcf service assembly file</param>
        /// <param name="configFile">Wcf service config file</param>
        public WcfIsolatedServiceHost(string assemblyFile, string configFile)
        {
            this.AssemblyFile = assemblyFile;
            this.ConfigFile = configFile ?? string.Format("{0}.config", assemblyFile);
            this.CreateDomain();
        }

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<EventArgs> Created;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<EventArgs> Opening;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<EventArgs> Opened;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<EventArgs> Closing;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<EventArgs> Closed;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<EventArgs> Aborting;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<EventArgs> Aborted;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<EventArgs> Restarting;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<EventArgs> Restarted;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<EventArgs> Loaded;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<EventArgs> Unloaded;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<EventArgs> Reloaded;

        /// <summary>
        /// Gets a value indicating whether isolated AppDomain is loaded
        /// </summary>
        public bool IsAppDomainLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets current Wcf service assembly file
        /// </summary>
        public string AssemblyFile
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets current Wcf service config file
        /// </summary>
        public string ConfigFile
        {
            get;
            private set;
        }

        /// <summary>
        /// Open Wcf service
        /// </summary>
        public void Open()
        {
            if (this._wcfServiceHost != null)
            {
                try
                {
                    this._wcfServiceHost.Open();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfIsolatedServiceHostOpenExceptionStringFormat, e.Source, e.Message, e.StackTrace));
                    throw;
                }
            }
        }

        /// <summary>
        /// Close Wcf service
        /// </summary>
        public void Close()
        {
            if (this._wcfServiceHost != null)
            {
                try
                {
                    this._wcfServiceHost.Close();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfIsolatedServiceHostCloseExceptionStringFormat, e.Source, e.Message, e.StackTrace));
                    throw;
                }
            }
        }

        /// <summary>
        /// Abort Wcf service
        /// </summary>
        public void Abort()
        {
            if (this._wcfServiceHost != null)
            {
                try
                {
                    this._wcfServiceHost.Abort();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfIsolatedServiceHostAbortExceptionStringFormat, e.Source, e.Message, e.StackTrace));
                    throw;
                }
            }
        }

        /// <summary>
        /// Restart Wcf service
        /// </summary>
        public void Restart()
        {
            if (this._wcfServiceHost != null)
            {
                try
                {
                    this._wcfServiceHost.Restart();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfIsolatedServiceHostRestartExceptionStringFormat, e.Source, e.Message, e.StackTrace));
                    throw;
                }
            }
        }

        /// <summary>
        /// Unload current isolated AppDomain
        /// </summary>
        public void Unload()
        {
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
                this.RaiseEvent(Unloaded, null);
            }
        }

        /// <summary>
        /// Reload current isolated AppDomain
        /// </summary>
        public void Reload()
        {
            this.Unload();
            this.CreateDomain();
            this.RaiseEvent(Reloaded, null);
        }

        /// <summary>
        /// Get current isolated AppDomain
        /// </summary>
        /// <returns></returns>
        public AppDomain GetAppDomain()
        {
            return this._appDomain;
        }

        /// <summary>
        /// Get Wcf service state list
        /// </summary>
        /// <returns></returns>
        public List<WcfServiceHostState> GetStateList()
        {
            return this._wcfServiceHost.GetStateList();
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
                this.Unload();
            }

            // free native resources if there are any
        }

        /// <summary>
        ///
        /// </summary>
        private void CreateDomain()
        {
            try
            {
                AppDomainSetup appDomainSetup = new AppDomainSetup();
                appDomainSetup.ApplicationBase = Path.GetDirectoryName(this.AssemblyFile);
                appDomainSetup.ApplicationName = Path.GetFileNameWithoutExtension(this.AssemblyFile);
                appDomainSetup.ConfigurationFile = this.ConfigFile;
                appDomainSetup.LoaderOptimization = LoaderOptimization.MultiDomainHost;
                //appDomainSetup.ShadowCopyFiles = "true";
                //appDomainSetup.ShadowCopyDirectories = appDomainSetup.ApplicationBase;

                this._appDomain = AppDomain.CreateDomain(appDomainSetup.ApplicationName, AppDomain.CurrentDomain.Evidence, appDomainSetup);
                this._wcfServiceHost = _appDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(WcfServiceHost).FullName) as WcfServiceHost;
                this.IsAppDomainLoaded = true;
                this.SubscribeAllWcfServiceHostEvent();
                this._wcfServiceHost.Init(this.AssemblyFile, this.ConfigFile);
                this.RaiseEvent(Loaded, null);
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfIsolatedServiceHostCreateDomainExceptionStringFormat, e.Source, e.Message, e.StackTrace));
                this.Unload();
                throw;
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void SubscribeAllWcfServiceHostEvent()
        {
            this._wcfServiceHost.Created += (s, e) => this.RaiseEvent(Created, e);
            this._wcfServiceHost.Opening += (s, e) => this.RaiseEvent(Opening, e);
            this._wcfServiceHost.Opened += (s, e) => this.RaiseEvent(Opened, e);
            this._wcfServiceHost.Closing += (s, e) => this.RaiseEvent(Closing, e);
            this._wcfServiceHost.Closed += (s, e) => this.RaiseEvent(Closed, e);
            this._wcfServiceHost.Aborting += (s, e) => this.RaiseEvent(Aborting, e);
            this._wcfServiceHost.Aborted += (s, e) => this.RaiseEvent(Aborted, e);
            this._wcfServiceHost.Restarting += (s, e) => this.RaiseEvent(Restarting, e);
            this._wcfServiceHost.Restarted += (s, e) => this.RaiseEvent(Restarted, e);
        }

        /// <summary>
        ///
        /// </summary>
        private void UnSubscribeAllWcfServiceHostEvent()
        {
            this._wcfServiceHost.Created -= (s, e) => this.RaiseEvent(Created, e);
            this._wcfServiceHost.Opening -= (s, e) => this.RaiseEvent(Opening, e);
            this._wcfServiceHost.Opened -= (s, e) => this.RaiseEvent(Opened, e);
            this._wcfServiceHost.Closing -= (s, e) => this.RaiseEvent(Closing, e);
            this._wcfServiceHost.Closed -= (s, e) => this.RaiseEvent(Closed, e);
            this._wcfServiceHost.Aborting -= (s, e) => this.RaiseEvent(Aborting, e);
            this._wcfServiceHost.Aborted -= (s, e) => this.RaiseEvent(Aborted, e);
            this._wcfServiceHost.Restarting -= (s, e) => this.RaiseEvent(Restarting, e);
            this._wcfServiceHost.Restarted -= (s, e) => this.RaiseEvent(Restarted, e);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <param name="e"></param>
        private void RaiseEvent(EventHandler<EventArgs> eventHandler, EventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<EventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, e);
            }
        }
    }
}
