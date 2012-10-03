//-----------------------------------------------------------------------
// <copyright file="WcfServiceHostEventArgs.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;

    /// <summary>
    /// WcfServiceHost EventArgs
    /// </summary>
    [Serializable]
    public class WcfServiceHostEventArgs : EventArgs
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="state"></param>
        public WcfServiceHostEventArgs(string wcfServiceName, WcfServiceHostState state)
        {
            this.WcfServiceName = wcfServiceName;
            this.State = state;
        }

        /// <summary>
        /// Gets Wcf service state
        /// </summary>
        public WcfServiceHostState State
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets Wcf service name
        /// </summary>
        public string WcfServiceName
        {
            get;
            private set;
        }
    }
}
