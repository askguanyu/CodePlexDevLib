//-----------------------------------------------------------------------
// <copyright file="LdapUser.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DirectoryServices
{
    using System;
    using System.Collections.Generic;
    using System.Security.Permissions;

    /// <summary>
    /// Ldap user.
    /// </summary>
    [Serializable]
    public class LdapUser : MarshalByRefObject
    {
        /// <summary>
        /// Gets or sets user account name.
        /// </summary>
        public string UserName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets display name.
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets email address.
        /// </summary>
        public string EmailAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets department.
        /// </summary>
        public string Department
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets phone number.
        /// </summary>
        public string PhoneNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets password last set time.
        /// </summary>
        public DateTime? PasswordLastSetTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets mailing address.
        /// </summary>
        public MailingAddress MailingAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets list of groups that the given user is a member of.
        /// </summary>
        public List<string> Groups
        {
            get;
            set;
        }

        /// <summary>
        /// Creates and returns a string representation of the current instance.
        /// </summary>
        /// <returns>A string representation of the current instance DisplayName.</returns>
        public override string ToString()
        {
            return string.Format(
                "User account: {0} | Display name: {1}",
                this.UserName ?? string.Empty,
                this.DisplayName ?? string.Empty);
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
