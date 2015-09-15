//-----------------------------------------------------------------------
// <copyright file="Sponsor.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Hosting.WebHost40
{
    using System;
    using System.Runtime.Remoting.Lifetime;

    /// <summary>
    /// Sponsor class.
    /// </summary>
    internal sealed class Sponsor : MarshalByRefObject, ISponsor
    {
        /// <summary>
        /// Requests a sponsoring client to renew the lease for the specified object.
        /// </summary>
        /// <param name="lease">The lifetime lease of the object that requires lease renewal.</param>
        /// <returns>The additional lease time for the specified object.</returns>
        public TimeSpan Renewal(ILease lease)
        {
            return lease.InitialLeaseTime;
        }
    }
}
