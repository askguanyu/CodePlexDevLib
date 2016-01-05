//-----------------------------------------------------------------------
// <copyright file="WcfServiceHostEventArgs.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.ServiceModel;

    /// <summary>
    /// WcfServiceHost EventArgs.
    /// </summary>
    [Serializable]
    public class WcfServiceHostEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHostEventArgs" /> class.
        /// </summary>
        /// <param name="serviceHostBase">The service host.</param>
        /// <param name="state">The state of WcfServiceHost.</param>
        public WcfServiceHostEventArgs(ServiceHostBase serviceHostBase, WcfServiceHostState state)
        {
            this.ServiceHost = serviceHostBase;
            this.State = state;
        }

        /// <summary>
        /// Gets the service host.
        /// </summary>
        public ServiceHostBase ServiceHost
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets Wcf service state.
        /// </summary>
        public WcfServiceHostState State
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format(
                "BaseAddress: {0} \r\nState: {1}",
                (this.ServiceHost != null && this.ServiceHost.BaseAddresses != null && this.ServiceHost.BaseAddresses.Count > 0) ? this.ServiceHost.BaseAddresses[0].ToString() : string.Empty,
                this.State.ToString());
        }
    }
}
