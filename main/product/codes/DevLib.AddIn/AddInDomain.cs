//-----------------------------------------------------------------------
// <copyright file="AddInDomain.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Threading;

    /// <summary>
    /// Represents an isolated environment in a separate process in which objects can be created and invoked.
    /// </summary>
    public sealed class AddInDomain : IDisposable
    {
        /// <summary>
        ///
        /// </summary>
        private AddInActivatorProcess _addInActivatorProcess;

        /// <summary>
        ///
        /// </summary>
        private int _unloaded;

        /// <summary>
        ///
        /// </summary>
        private int _overloadCreateInstanceAndUnwrap = 0;

        /// <summary>
        ///
        /// </summary>
        private string _addInAssemblyName;

        /// <summary>
        ///
        /// </summary>
        private object[] _addInArgs = null;

        /// <summary>
        ///
        /// </summary>
        private object[] _addInActivationAttributes = null;

        /// <summary>
        ///
        /// </summary>
        private bool _addInIgnoreCase;

        /// <summary>
        ///
        /// </summary>
        private BindingFlags _addInBindingAttr = BindingFlags.Default;

        /// <summary>
        ///
        /// </summary>
        private Binder _addInBinder = null;

        /// <summary>
        ///
        /// </summary>
        private CultureInfo _addInCulture = null;

        /// <summary>
        ///
        /// </summary>
        private Evidence _addInSecurityAttributes = null;

        /// <summary>
        ///
        /// </summary>
        private bool _canRestart = true;

        /// <summary>
        ///
        /// </summary>
        private bool _redirectOutput;

        /// <summary>
        /// Creates a AddInDomain which allows hosting objects and code in isolated process.
        /// </summary>
        /// <param name="friendlyName">The friendly name of the AddInDomain.</param>
        /// <param name="showRedirectConsoleOutput">Whether the output of AddInActivatorProcess is shown in current console.</param>
        /// <param name="addInDomainSetup">Additional settings for creating AddInDomain.</param>
        public AddInDomain(string friendlyName = null, bool showRedirectConsoleOutput = true, AddInDomainSetup addInDomainSetup = null)
        {
            this.FriendlyName = string.IsNullOrEmpty(friendlyName) ? AddInConstants.DefaultFriendlyName : friendlyName;
            this.AddInDomainSetupInfo = (addInDomainSetup == null) ? new AddInDomainSetup() : addInDomainSetup;
            this._redirectOutput = showRedirectConsoleOutput;
            this.CreateAddInActivatorProcess();
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
        public event EventHandler<AddInDomainEventArgs> Loaded;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<AddInDomainEventArgs> Unloaded;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<AddInDomainEventArgs> Reloaded;

        /// <summary>
        /// Occurs when AddInActivatorProcess writes to its redirected <see cref="P:System.Diagnostics.Process.StandardOutput" /> stream.
        /// </summary>
        public event DataReceivedEventHandler DataReceived;

        /// <summary>
        /// Gets AddInActivatorProcessInfo.
        /// </summary>
        public AddInActivatorProcessInfo ProcessInfo
        {
            get
            {
                if (this._addInActivatorProcess != null)
                {
                    return this._addInActivatorProcess.ProcessInfo;
                }
                else
                {
                    return new AddInActivatorProcessInfo();
                }
            }
        }

        /// <summary>
        /// Gets AddIn type name.
        /// </summary>
        public string AddInTypeName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets AddIn object.
        /// </summary>
        public object AddInObject
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the friendly name of the AddInDomain.
        /// </summary>
        public string FriendlyName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets AddInDomainSetup infomation.
        /// </summary>
        public AddInDomainSetup AddInDomainSetupInfo
        {
            get;
            private set;
        }

        /// <summary>
        /// Unloads a given AddInDomain by terminating the process.
        /// </summary>
        /// <param name="addInDomain">The AddInDomain to unload.</param>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public static void Unload(AddInDomain addInDomain)
        {
            addInDomain.Unload();
        }

        /// <summary>
        ///
        /// </summary>
        public void Reload()
        {
            this.Unload();
            this.RestartAddInActivatorProcess();
        }

        /// <summary>
        /// Creates a new instance of the specified type.
        /// </summary>
        /// <param name="assemblyName">The display name of the assembly. See <see cref="P:System.Reflection.Assembly.FullName" />.</param>
        /// <param name="typeName">The fully qualified name of the requested type, including the namespace but not the assembly, as returned by the <see cref="P:System.Type.FullName" /> property.</param>
        /// <returns>An instance of the object specified by <paramref name="typeName" />.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName)
        {
            this.StartAddInActivatorProcess();

            this._overloadCreateInstanceAndUnwrap = 1;
            this._addInAssemblyName = assemblyName;
            this.AddInTypeName = typeName;
            this.AddInObject = this._addInActivatorProcess.AddInActivatorClient.CreateInstanceAndUnwrap(this._addInAssemblyName, this.AddInTypeName);
            return this.AddInObject;
        }

        /// <summary>
        /// Creates a new instance of the specified type.
        /// </summary>
        /// <param name="assemblyName">The display name of the assembly. See <see cref="P:System.Reflection.Assembly.FullName" />.</param>
        /// <param name="typeName">The fully qualified name of the requested type, including the namespace but not the assembly, as returned by the <see cref="P:System.Type.FullName" /> property.</param>
        /// <param name="activationAttributes">An array of one or more attributes that can participate in activation. Typically, an array that contains a single <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> object. The <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> specifies the URL that is required to activate a remote object.</param>
        /// <returns>An instance of the object specified by <paramref name="typeName" />.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName, object[] activationAttributes)
        {
            this.StartAddInActivatorProcess();

            this._overloadCreateInstanceAndUnwrap = 2;
            this._addInAssemblyName = assemblyName;
            this.AddInTypeName = typeName;
            this._addInActivationAttributes = activationAttributes;
            this.AddInObject = this._addInActivatorProcess.AddInActivatorClient.CreateInstanceAndUnwrap(this._addInAssemblyName, this.AddInTypeName, this._addInActivationAttributes);
            return this.AddInObject;
        }

        /// <summary>Creates a new instance of the specified type defined in the specified assembly, specifying whether the case of the type name is ignored; the binding attributes and the binder that are used to select the type to be created; the arguments of the constructor; the culture; and the activation attributes.</summary>
        /// <param name="assemblyName">The display name of the assembly. See <see cref="P:System.Reflection.Assembly.FullName" />.</param>
        /// <param name="typeName">The fully qualified name of the requested type, including the namespace but not the assembly, as returned by the <see cref="P:System.Type.FullName" /> property. </param>
        /// <param name="ignoreCase">A Boolean value specifying whether to perform a case-sensitive search or not. </param>
        /// <param name="bindingAttr">A combination of zero or more bit flags that affect the search for the <paramref name="typeName" /> constructor. If <paramref name="bindingAttr" /> is zero, a case-sensitive search for public constructors is conducted. </param>
        /// <param name="binder">An object that enables the binding, coercion of argument types, invocation of members, and retrieval of <see cref="T:System.Reflection.MemberInfo" /> objects using reflection. If <paramref name="binder" /> is null, the default binder is used. </param>
        /// <param name="args">The arguments to pass to the constructor. This array of arguments must match in number, order, and type the parameters of the constructor to invoke. If the default constructor is preferred, <paramref name="args" /> must be an empty array or null. </param>
        /// <param name="culture">A culture-specific object used to govern the coercion of types. If <paramref name="culture" /> is null, the CultureInfo for the current thread is used. </param>
        /// <param name="activationAttributes">An array of one or more attributes that can participate in activation. Typically, an array that contains a single <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> object. The <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> specifies the URL that is required to activate a remote object. </param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="assemblyName" /> or <paramref name="typeName" /> is null. </exception>
        /// <exception cref="T:System.MissingMethodException">No matching constructor was found. </exception>
        /// <exception cref="T:System.TypeLoadException">
        ///   <paramref name="typename" /> was not found in <paramref name="assemblyName" />. </exception>
        /// <exception cref="T:System.IO.FileNotFoundException">
        ///   <paramref name="assemblyName" /> was not found. </exception>
        /// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
        /// <exception cref="T:System.NotSupportedException">The caller cannot provide activation attributes for an object that does not inherit from <see cref="T:System.MarshalByRefObject" />. </exception>
        /// <exception cref="T:System.AppDomainUnloadedException">The operation is attempted on an unloaded application domain. </exception>
        /// <exception cref="T:System.BadImageFormatException">
        ///   <paramref name="assemblyName" /> is not a valid assembly. -or-<paramref name="assemblyName" /> was compiled with a later version of the common language runtime than the version that is currently loaded.</exception>
        /// <exception cref="T:System.IO.FileLoadException">An assembly or module was loaded twice with two different evidences. </exception>
        /// <returns>An instance of the object specified by <paramref name="typeName" />.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
        {
            this.StartAddInActivatorProcess();

            this._overloadCreateInstanceAndUnwrap = 3;
            this._addInAssemblyName = assemblyName;
            this.AddInTypeName = typeName;
            this._addInIgnoreCase = ignoreCase;
            this._addInBindingAttr = bindingAttr;
            this._addInBinder = binder;
            this._addInArgs = args;
            this._addInCulture = culture;
            this._addInActivationAttributes = activationAttributes;
            this._addInSecurityAttributes = securityAttributes;
            this.AddInObject = this._addInActivatorProcess.AddInActivatorClient.CreateInstanceAndUnwrap(
                this._addInAssemblyName,
                this.AddInTypeName,
                this._addInIgnoreCase,
                this._addInBindingAttr,
                this._addInBinder,
                this._addInArgs,
                this._addInCulture,
                this._addInActivationAttributes,
                this._addInSecurityAttributes);
            return this.AddInObject;
        }

        /// <summary>
        /// Creates an object of the specified type.
        /// </summary>
        /// <returns>An instance of the object specified by <paramref name="typeName" />.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public T CreateInstance<T>()
        {
            return (T)this.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
        }

        /// <summary>
        /// Creates an object of the specified type.
        /// </summary>
        /// <param name="args">The arguments to pass to the constructor. This array of arguments must match in number, order, and type the parameters of the constructor to invoke. If the default constructor is preferred, <paramref name="args" /> must be an empty array or null. </param>
        /// <returns>An instance of the object specified by <paramref name="typeName" />.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public T CreateInstance<T>(params object[] args)
        {
            return (T)this.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName, true, BindingFlags.Default, null, args, null, null, null);
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
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        private void Unload()
        {
            this._canRestart = false;

            if (Interlocked.CompareExchange(ref _unloaded, 1, 0) == 1)
            {
                return;
            }

            if (this._addInActivatorProcess != null)
            {
                this._addInActivatorProcess.Dispose();
                this._addInActivatorProcess = null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void CreateAddInActivatorProcess()
        {
            this._canRestart = true;

            this._addInActivatorProcess = new AddInActivatorProcess(this.FriendlyName, this._redirectOutput, this.AddInDomainSetupInfo);
            this._addInActivatorProcess.Attached += this.OnProcessAttached;
            this._addInActivatorProcess.Detached += this.OnProcessDetached;
            this._addInActivatorProcess.DataReceived += this.OnDataReceived;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.RaiseDataReceivedEvent(sender, e);
        }

        /// <summary>
        ///
        /// </summary>
        private void StartAddInActivatorProcess()
        {
            if (this._addInActivatorProcess != null && !this._addInActivatorProcess.IsRunning)
            {
                this._addInActivatorProcess.Start();
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void RestartAddInActivatorProcess()
        {
            this.CreateAddInActivatorProcess();

            this.StartAddInActivatorProcess();

            if (this.AddInObject != null)
            {
                try
                {
                    switch (this._overloadCreateInstanceAndUnwrap)
                    {
                        case 1:
                            this.AddInObject = this._addInActivatorProcess.AddInActivatorClient.CreateInstanceAndUnwrap(this._addInAssemblyName, this.AddInTypeName);
                            break;
                        case 2:
                            this.AddInObject = this._addInActivatorProcess.AddInActivatorClient.CreateInstanceAndUnwrap(this._addInAssemblyName, this.AddInTypeName, this._addInActivationAttributes);
                            break;
                        case 3:
                            this.AddInObject = this._addInActivatorProcess.AddInActivatorClient.CreateInstanceAndUnwrap(
                                this._addInAssemblyName,
                                this.AddInTypeName,
                                this._addInIgnoreCase,
                                this._addInBindingAttr,
                                this._addInBinder,
                                this._addInArgs,
                                this._addInCulture,
                                this._addInActivationAttributes,
                                this._addInSecurityAttributes);
                            break;
                        default:
                            break;
                    }
                }
                catch
                {
                }
            }

            this.RaiseEvent(Reloaded);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="eventHandler"></param>
        private void RaiseEvent(EventHandler<AddInDomainEventArgs> eventHandler)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<AddInDomainEventArgs> temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(null, new AddInDomainEventArgs(this.FriendlyName, this.AddInTypeName, this.AddInObject, this.AddInDomainSetupInfo, this.ProcessInfo));
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RaiseDataReceivedEvent(object sender, DataReceivedEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            DataReceivedEventHandler temp = Interlocked.CompareExchange(ref DataReceived, null, null);

            if (temp != null)
            {
                temp(sender, e);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProcessAttached(object sender, EventArgs e)
        {
            this.RaiseEvent(Loaded);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProcessDetached(object sender, EventArgs e)
        {
            this.RaiseEvent(Unloaded);

            if (this.AddInDomainSetupInfo.RestartOnProcessExit && this._canRestart)
            {
                RestartAddInActivatorProcess();
            }
        }
    }
}
