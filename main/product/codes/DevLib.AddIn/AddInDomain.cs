//-----------------------------------------------------------------------
// <copyright file="AddInDomain.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Threading;

    /// <summary>
    /// Represents an isolated environment in a separate process in which objects can be created and invoked.
    /// </summary>
    public sealed class AddInDomain : IAddInDomain, IDisposable
    {
        /// <summary>
        ///
        /// </summary>
        private readonly AutoResetEvent _attachedEvent = new AutoResetEvent(false);

        /// <summary>
        ///
        /// </summary>
        private readonly AutoResetEvent _detachedEvent = new AutoResetEvent(false);

        /// <summary>
        ///
        /// </summary>
        private readonly AddInActivatorProcess _process;

        /// <summary>
        ///
        /// </summary>
        private int _unloaded;

        /// <summary>
        ///
        /// </summary>
        /// <param name="friendlyName"></param>
        /// <param name="addInDomainSetup"></param>
        private AddInDomain(string friendlyName, AddInDomainSetup addInDomainSetup)
        {
            this.FriendlyName = string.IsNullOrEmpty(friendlyName) ? AddInConstants.DefaultFriendlyName : friendlyName;
            this.AddInDomainSetupInfo = (addInDomainSetup == null) ? new AddInDomainSetup() : addInDomainSetup;
            this._process = new AddInActivatorProcess(this.FriendlyName, this.AddInDomainSetupInfo);
            this._process.Attached += OnProcessAttached;
            this._process.Detached += OnProcessDetached;

            //this._process.Start();
        }

        /// <summary>
        ///
        /// </summary>
        ~AddInDomain()
        {
            Unload();
        }

        /// <summary>
        ///
        /// </summary>
        public event EventHandler Attached;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler Detached;

        /// <summary>
        /// Gets
        /// </summary>
        public string FriendlyName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets
        /// </summary>
        public AddInDomainSetup AddInDomainSetupInfo
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a AddInDomain which allows hosting objects and code in isolated process.
        /// </summary>
        /// <param name="friendlyName">The friendly name of the process domain which directly will also be the file name of the remote process.</param>
        /// <param name="setupInfo">Additional settings for creating the process domain.</param>
        public static AddInDomain CreateDomain(string friendlyName = null, AddInDomainSetup setupInfo = null)
        {
            return new AddInDomain(friendlyName, setupInfo);
        }

        /// <summary>
        /// Unloads a given process domain by terminating the process.
        /// </summary>
        /// <param name="domain">The process domain to unload.</param>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public static void Unload(AddInDomain domain)
        {
            domain.Unload();
        }

        /// <summary>
        /// Creates an object of the specified type.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName)
        {
            this._process.Start();
            return _process.AddInActivator.CreateInstanceAndUnwrap(assemblyName, typeName);
        }

        /// <summary>
        /// Creates an object of the specified type.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName, object[] activationAttributes)
        {
            this._process.Start();
            return _process.AddInActivator.CreateInstanceAndUnwrap(assemblyName, typeName, activationAttributes);
        }

        /// <summary>
        /// Creates an object of the specified type.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
        {
            this._process.Start();
            return _process.AddInActivator.CreateInstanceAndUnwrap(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
        }

        /// <summary>
        /// Creates an object of the specified type.
        /// </summary>
        /// <returns></returns>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public T CreateInstance<T>()
        {
            return (T)this.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
        }

        /// <summary>
        ///
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public void Dispose()
        {
            Unload();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="eventHandler"></param>
        private void RaiseEvent(EventHandler eventHandler)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(null, null);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProcessAttached(object sender, EventArgs e)
        {
            this.RaiseEvent(Attached);
            this._attachedEvent.Set();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProcessDetached(object sender, EventArgs e)
        {
            this.RaiseEvent(Detached);
            this._detachedEvent.Set();
        }

        /// <summary>
        ///
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        private void Unload()
        {
            if (Interlocked.CompareExchange(ref _unloaded, 1, 0) == 1)
            {
                return;
            }

            if (_process != null)
            {
                _process.Kill();
                _process.Dispose();
            }

            this._attachedEvent.Close();
            this._detachedEvent.Close();
        }
    }
}
