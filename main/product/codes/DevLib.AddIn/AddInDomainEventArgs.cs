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
        /// <param name="addInObject"></param>
        /// <param name="addInDomainSetupInfo"></param>
        /// <param name="processInfo"></param>
        public AddInDomainEventArgs(string friendlyName, string addInTypeName, object addInObject, AddInDomainSetup addInDomainSetupInfo, AddInActivatorProcessInfo processInfo)
        {
            this.FriendlyName = friendlyName;
            this.AddInTypeName = addInTypeName;
            this.AddInObject = addInObject;
            this.AddInDomainSetupInfo = addInDomainSetupInfo;
            this.ProcessInfo = processInfo;
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
        /// Gets AddIn object.
        /// </summary>
        public object AddInObject
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

        /// <summary>
        /// Gets AddInDomainSetup infomation.
        /// </summary>
        public AddInDomainSetup AddInDomainSetupInfo
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets AddInActivatorProcessInfo.
        /// </summary>
        public AddInActivatorProcessInfo ProcessInfo
        {
            get;
            private set;
        }
    }
}
