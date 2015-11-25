//-----------------------------------------------------------------------
// <copyright file="WcfServiceHostInfo.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    /// <summary>
    /// WcfServiceHost Info.
    /// </summary>
    [Serializable]
    public class WcfServiceHostInfo
    {
        /// <summary>
        /// Gets or sets service type string.
        /// </summary>
        public string ServiceType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets base address.
        /// </summary>
        public string BaseAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets CommunicationState.
        /// </summary>
        public CommunicationState State
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets ServiceCredentials.
        /// </summary>
        public ServiceCredentials Credentials
        {
            get;
            set;
        }

        /// <summary>
        /// Creates and returns a string representation of the current instance.
        /// </summary>
        /// <returns>A string representation of the current WcfServiceHostInfo.</returns>
        public override string ToString()
        {
            return string.Format(
                "ServiceType: {0} | BaseAddress: {1} | State: {2}",
                this.ServiceType,
                this.BaseAddress,
                this.State.ToString());
        }
    }
}
