//-----------------------------------------------------------------------
// <copyright file="WcfServiceHostState.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    /// <summary>
    /// Defines the WcfServiceHost states
    /// </summary>
    public enum WcfServiceHostState
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
}
