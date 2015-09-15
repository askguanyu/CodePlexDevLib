//-----------------------------------------------------------------------
// <copyright file="MailingAddress.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DirectoryServices
{
    using System;
    using System.Security.Permissions;

    /// <summary>
    /// Mailing address.
    /// </summary>
    [Serializable]
    public class MailingAddress : MarshalByRefObject
    {
        /// <summary>
        /// Gets or sets street line1.
        /// </summary>
        public string StreetLine1
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets street line2.
        /// </summary>
        public string StreetLine2
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets PO Box.
        /// </summary>
        public string POBox
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets city.
        /// </summary>
        public string City
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets state.
        /// </summary>
        public string State
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets postal code.
        /// </summary>
        public string PostalCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets country.
        /// </summary>
        public string Country
        {
            get;
            set;
        }

        /// <summary>
        /// Creates and returns a string representation of the current instance.
        /// </summary>
        /// <returns>A string representation of the current instance.</returns>
        public override string ToString()
        {
            return string.Format(
                "Street Line1: {0} | Street Line2: {1} | POBox: {2} | City: {3} | State: {4} | PostalCode: {5} | Country: {6}",
                this.StreetLine1 ?? string.Empty,
                this.StreetLine2 ?? string.Empty,
                this.POBox ?? string.Empty,
                this.City ?? string.Empty,
                this.State ?? string.Empty,
                this.PostalCode ?? string.Empty,
                this.Country ?? string.Empty);
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
