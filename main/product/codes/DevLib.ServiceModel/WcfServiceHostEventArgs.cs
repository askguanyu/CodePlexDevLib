//-----------------------------------------------------------------------
// <copyright file="WcfServiceHostEventArgs.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;

    /// <summary>
    /// WcfServiceHost EventArgs.
    /// </summary>
    [Serializable]
    public class WcfServiceHostEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfServiceHostEventArgs" /> class.
        /// </summary>
        /// <param name="wcfServiceName">String of Wcf Service Name.</param>
        /// <param name="state">Instance of WcfServiceHostStateEnum.</param>
        public WcfServiceHostEventArgs(string wcfServiceName, WcfServiceHostStateEnum state)
        {
            this.WcfServiceName = wcfServiceName;
            this.State = state;
        }

        /// <summary>
        /// Gets Wcf service state.
        /// </summary>
        public WcfServiceHostStateEnum State
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets Wcf service name.
        /// </summary>
        public string WcfServiceName
        {
            get;
            private set;
        }
    }
}
