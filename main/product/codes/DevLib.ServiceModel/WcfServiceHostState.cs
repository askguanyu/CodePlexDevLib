//-----------------------------------------------------------------------
// <copyright file="WcfServiceHostState.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.ServiceModel;

    /// <summary>
    /// Defines the WcfServiceHost state Enum
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
    /// WcfServiceHost State
    /// </summary>
    [Serializable]
    public class WcfServiceHostState
    {
        /// <summary>
        ///Gets or sets
        /// </summary>
        public string ServiceType
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        public string BaseAddress
        {
            get;
            set;
        }

        /// <summary>
        ///Gets or sets
        /// </summary>
        public CommunicationState State
        {
            get;
            set;
        }
    }
}
