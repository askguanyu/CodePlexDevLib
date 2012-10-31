//-----------------------------------------------------------------------
// <copyright file="AddInActivatorClient.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.Collections;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Ipc;
    using System.Security.Permissions;

    /// <summary>
    /// Provides access to an Activator in a remote process.
    /// </summary>
    internal class AddInActivatorClient : IDisposable
    {
        /// <summary>
        ///
        /// </summary>
        private readonly AddInActivator _addInActivator;

        /// <summary>
        ///
        /// </summary>
        private readonly IChannel _channel;

        /// <summary>
        ///
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="addInDomainSetup"></param>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public AddInActivatorClient(string guid, AddInDomainSetup addInDomainSetup)
        {
            BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider { TypeFilterLevel = addInDomainSetup.TypeFilterLevel };
            BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();

            Hashtable properties = new Hashtable();
            properties[AddInConstants.KeyIpcPortName] = string.Format(AddInActivatorHost.AddInClientChannelNameStringFormat, guid);
            properties[AddInConstants.KeyIpcChannelName] = string.Format(AddInActivatorHost.AddInClientChannelNameStringFormat, guid);

            this._channel = new IpcChannel(properties, clientProvider, serverProvider);
            ChannelServices.RegisterChannel(this._channel, false);

            this._addInActivator = (AddInActivator)Activator.GetObject(typeof(AddInActivator), string.Format(AddInConstants.IpcUrlStringFormat, string.Format(AddInActivatorHost.AddInServerChannelNameStringFormat, guid), AddInActivatorHost.AddInActivatorName));
        }

        /// <summary>
        /// Gets
        /// </summary>
        public AddInActivator AddInActivator
        {
            get { return this._addInActivator; }
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            ChannelServices.UnregisterChannel(_channel);
        }
    }
}