//-----------------------------------------------------------------------
// <copyright file="LdapResult.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DirectoryServices
{
    using System;
    using System.Security.Permissions;

    /// <summary>
    /// Ldap result information.
    /// </summary>
    [Serializable]
    public class LdapResult : MarshalByRefObject
    {
        /// <summary>
        /// Gets or sets a value indicating whether get Ldap result succeeded or not.
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
        /// Gets or sets the name of the application or the object that causes the result.
        /// </summary>
        public string Source
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets LdapUserObject instance.
        /// </summary>
        public LdapUserObject UserObject
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
