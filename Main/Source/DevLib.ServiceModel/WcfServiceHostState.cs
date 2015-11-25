//-----------------------------------------------------------------------
// <copyright file="WcfServiceHostState.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    /// <summary>
    /// Defines the WcfServiceHost state.
    /// </summary>
    public enum WcfServiceHostState
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
}
