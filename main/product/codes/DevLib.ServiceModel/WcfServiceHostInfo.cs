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
        /// <summary>
        /// Represents Created.
        /// </summary>
        Created,

        /// <summary>
        /// Represents Opening.
        /// </summary>
        Opening,

        /// <summary>
        /// Represents Opened.
        /// </summary>
        Opened,

        /// <summary>
        /// Represents Closing.
        /// </summary>
        Closing,

        /// <summary>
        /// Represents Closed.
        /// </summary>
        Closed,

        /// <summary>
        /// Represents Aborting.
        /// </summary>
        Aborting,

        /// <summary>
        /// Represents Aborted.
        /// </summary>
        Aborted,

        /// <summary>
        /// Represents Restarting.
        /// </summary>
        Restarting,

        /// <summary>
        /// Represents Restarted.
        /// </summary>
        Restarted,

        /// <summary>
        /// Represents Faulted.
        /// </summary>
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
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <returns>Current CommunicationState string.</returns>
        public override string ToString()
        {
            return this.State.ToString();
        }
    }
}
