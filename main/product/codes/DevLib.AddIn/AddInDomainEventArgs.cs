//-----------------------------------------------------------------------
// <copyright file="AddInDomainEventArgs.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;

    /// <summary>
    ///
    /// </summary>
    public class AddInDomainEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor of AddInDomainEventArgs.
        /// </summary>
        /// <param name="friendlyName"></param>
        /// <param name="addInTypeName"></param>
        public AddInDomainEventArgs(string friendlyName, string addInTypeName)
        {
            this.FriendlyName = friendlyName;
            this.AddInTypeName = addInTypeName;
        }

        /// <summary>
        /// Gets AddIn type name.
        /// </summary>
        public string AddInTypeName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the friendly name of the AddInDomain.
        /// </summary>
        public string FriendlyName
        {
            get;
            private set;
        }
    }
}
