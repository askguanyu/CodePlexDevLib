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
        ///
        /// </summary>
        /// <param name="assemblyFile"></param>
        /// <param name="configFile"></param>
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
        ///Gets or sets
        /// </summary>
        public string AssemblyFile
        {
            get;
            private set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        public string ConfigFile
        {
            get;
            private set;
        }

        /// <summary>
        ///
        /// </summary>
        public void Open()
        {
            if (this._wcfServiceHost != null)
            {
                this._wcfServiceHost.Open();
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Close()
        {
            if (this._wcfServiceHost != null)
            {
                this._wcfServiceHost.Close();
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Abort()
        {
            if (this._wcfServiceHost != null)
            {
                this._wcfServiceHost.Abort();
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Restart()
        {
            if (this._wcfServiceHost != null)
            {
                this._wcfServiceHost.Restart();
            }
        }

        /// <summary>
        ///
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
                this._appDomain = null;
                this.RaiseEvent(Unloaded, null);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Reload()
        {
            this.Unload();
            this.CreateDomain();
            this.RaiseEvent(Reloaded, null);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public AppDomain GetAppDomain()
        {
            return this._appDomain;
        }

        /// <summary>
        ///
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
                appDomainSetup.LoaderOptimization = LoaderOptimization.MultiDomain;
                this._appDomain = AppDomain.CreateDomain(appDomainSetup.ApplicationName, AppDomain.CurrentDomain.Evidence, appDomainSetup);
                this._wcfServiceHost = _appDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(WcfServiceHost).FullName) as WcfServiceHost;
                this.SubscribeAllWcfServiceHostEvent();
                this._wcfServiceHost.Init(this.AssemblyFile, this.ConfigFile);
                this.RaiseEvent(Loaded, null);
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format(WcfServiceHostConstants.WcfServiceHostInitExceptionStringFormat, e.Source, e.Message, e.StackTrace));
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
