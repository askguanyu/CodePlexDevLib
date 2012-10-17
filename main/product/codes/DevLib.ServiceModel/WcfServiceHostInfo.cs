//-----------------------------------------------------------------------
// <copyright file="WcfServiceHostInfo.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.ServiceModel;

    /// <summary>
    /// Defines the WcfServiceHost state Enum.
    /// </summary>
    public enum WcfServiceHostStateEnum
    {
        Created,
        Opening,
        Opened,
        Closing,
        Closed,
        Aborting,
        Aborted,
        Restarting,
        Restarted,
        Faulted
    }

    /// <summary>
    /// WcfServiceHost Info.
    /// </summary>
    [Serializable]
    public class WcfServiceHostInfo
    {
        /// <summary>
        /// Gets or sets service type string
        /// </summary>
        public string ServiceType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets base address
        /// </summary>
        public string BaseAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets CommunicationState
        /// </summary>
        public CommunicationState State
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.State.ToString();
        }
    }
}
