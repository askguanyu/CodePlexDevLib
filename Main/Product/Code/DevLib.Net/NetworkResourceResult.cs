//-----------------------------------------------------------------------
// <copyright file="NetworkResourceResult.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net
{
    using System;
    using System.Security.Permissions;

    /// <summary>
    /// NetworkResource Result.
    /// </summary>
    [Serializable]
    public class NetworkResourceResult : MarshalByRefObject
    {
        /// <summary>
        /// Gets or sets a value indicating whether network resource operation succeeded or not.
        /// </summary>
        public bool Succeeded
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public int ErrorCode
        {
            get;
            set;
        }

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns>An infinite lifetime.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
