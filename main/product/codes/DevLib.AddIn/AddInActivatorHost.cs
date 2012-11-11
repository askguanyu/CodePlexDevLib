//-----------------------------------------------------------------------
// <copyright file="AddInActivatorHost.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Ipc;
    using System.Threading;

    /// <summary>
    ///
    /// </summary>
    [Serializable]
    internal class AddInActivatorHost : MarshalByRefObject
    {
        /// <summary>
        ///
        /// </summary>
        public const string AddInServerChannelNameStringFormat = "DevLib_AddInDomainServer_{0}";

        /// <summary>
        ///
        /// </summary>
        public const string AddInClientChannelNameStringFormat = "DevLib_AddInDomainClient_{0}";

        /// <summary>
        ///
        /// </summary>
        public const string AddInDomainEventNameStringFormat = "DevLib_AddInDomainEvent_{0}";

        /// <summary>
        ///
        /// </summary>
        public const string AddInActivatorName = "DevLib_AddInActivator";

        /// <summary>
        ///
        /// </summary>
        [NonSerialized]
        private readonly Process _process;

        /// <summary>
        ///
        /// </summary>
        [NonSerialized]
        private readonly IpcChannel _ipcChannel;

        /// <summary>
        ///
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="processId"></param>
        /// <param name="addInDomainSetup"></param>
        public AddInActivatorHost(string guid, int processId, AddInDomainSetup addInDomainSetup)
        {
            SetupDllDirectory(addInDomainSetup.DllDirectory);
            BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider { TypeFilterLevel = addInDomainSetup.TypeFilterLevel };
            BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
            this._process = Process.GetProcessById(processId);

            Hashtable properties = new Hashtable();
            properties[AddInConstants.KeyIpcPortName] = string.Format(AddInServerChannelNameStringFormat, guid);
            properties[AddInConstants.KeyIpcChannelName] = string.Format(AddInServerChannelNameStringFormat, guid);

            this._ipcChannel = new IpcChannel(properties, clientProvider, serverProvider);
            ChannelServices.RegisterChannel(_ipcChannel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(AddInActivator), AddInActivatorName, WellKnownObjectMode.Singleton);

            bool isCreated;

            using (EventWaitHandle serverStartedHandle = new EventWaitHandle(false, EventResetMode.ManualReset, string.Format(AddInDomainEventNameStringFormat, guid), out isCreated))
            {
                if (isCreated)
                {
                    throw new Exception(AddInConstants.EventHandleNotExist);
                }

                serverStartedHandle.Set();
            }
        }

        /// <summary>
        /// Runs AddInActivatorHost until the parent process exits.
        /// </summary>
        public static void Run(string[] args)
        {
            // args[0] = AddInDomain assembly path
            // args[1] = GUID
            // args[2] = PID
            // args[3] = AddInDomainSetup file

            if (args.Length != 4)
            {
                return;
            }

            string friendlyName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
            string guid = args[1];
            int processId = int.Parse(args[2]);
            AddInDomainSetup addInDomainSetup = AddInDomainSetup.ReadSetupFile(args[3]);
            AppDomain appDomain = AppDomain.CreateDomain(friendlyName, addInDomainSetup.Evidence, addInDomainSetup.AppDomainSetup);
            Type type = Assembly.GetEntryAssembly().GetType("DevLib.AddIn.AssemblyResolver");

            if (type == null)
            {
                throw new TypeLoadException(AddInConstants.AssemblyResolverException);
            }

            // add AddInDomain assembly to resolver
            if (addInDomainSetup.ExternalAssemblies == null)
            {
                addInDomainSetup.ExternalAssemblies = new Dictionary<AssemblyName, string>();
            }

            addInDomainSetup.ExternalAssemblies[typeof(AddInActivatorHost).Assembly.GetName()] = typeof(AddInActivatorHost).Assembly.Location;

            object resolver = appDomain.CreateInstanceFromAndUnwrap(
                type.Assembly.Location,
                type.FullName,
                false,
                BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { addInDomainSetup.ExternalAssemblies },
                null,
                null,
                null);

            type.InvokeMember("Mount", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, resolver, null);

            AddInActivatorHost host = (AddInActivatorHost)appDomain.CreateInstanceFromAndUnwrap(
                typeof(AddInActivatorHost).Assembly.Location,
                typeof(AddInActivatorHost).FullName,
                false,
                BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance,
                null,
                new object[] { guid, processId, addInDomainSetup },
                null,
                null,
                null);

            host.WaitForExit();

            type.InvokeMember("Unmount", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, resolver, null);

            // If parent process (host) finishes, the current process must end
            Environment.Exit(0);
        }

        /// <summary>
        /// Waits for the parent process to exit.
        /// </summary>
        public void WaitForExit()
        {
            this._process.WaitForExit();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dllDirectory"></param>
        private static void SetupDllDirectory(string dllDirectory)
        {
            if (!string.IsNullOrEmpty(dllDirectory))
            {
                if (!NativeMethods.SetDllDirectory(dllDirectory))
                {
                    throw new Win32Exception();
                }
            }
        }
    }
}
