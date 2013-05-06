//-----------------------------------------------------------------------
// <copyright file="AddInDomainSetup.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Security.Policy;

    /// <summary>
    /// Class AddInDomainSetup.
    /// </summary>
    [Serializable]
    public sealed class AddInDomainSetup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddInDomainSetup" /> class.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public AddInDomainSetup()
        {
            this.AppDomainSetup = AppDomain.CurrentDomain.SetupInformation;
            this.DeleteOnUnload = true;
            this.DllDirectory = Directory.GetCurrentDirectory();
            this.EnvironmentVariables = new Dictionary<string, string>();
            this.Evidence = AppDomain.CurrentDomain.Evidence;
            this.ExeFileDirectory = Path.GetTempPath();
            this.ExternalAssemblies = new Dictionary<AssemblyName, string>();
            this.Platform = PlatformTargetEnum.AnyCPU;
            this.ProcessPriority = ProcessPriorityClass.Normal;
            this.ProcessStartTimeout = new TimeSpan(0, 0, 15);
            this.RestartOnProcessExit = true;
            this.TypeFilterLevel = TypeFilterLevel.Full;
            this.WorkingDirectory = Environment.CurrentDirectory;
        }

        /// <summary>
        /// Gets or sets where the temporary remote process executable file will be created.
        /// </summary>
        public string ExeFileDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the working directory for the remote process.
        /// </summary>
        public string WorkingDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a directory to invoke SetDllDirectory with to redirect DLL probing to the working directory.
        /// </summary>
        public string DllDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets how long to wait for the remote process to start.
        /// </summary>
        public TimeSpan ProcessStartTimeout
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not to delete the generated executable after AddInDomain has unloaded.
        /// </summary>
        public bool DeleteOnUnload
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether AddInDomain should be relaunched when the process exit prematurely.
        /// </summary>
        public bool RestartOnProcessExit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets setup information for the AppDomain that the object will be created in, in the remote process.
        /// By default, this will be the current domain's setup information from which the proxy is being created.
        /// </summary>
        public AppDomainSetup AppDomainSetup
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets which platform to compile the target remote process assembly for.
        /// </summary>
        public PlatformTargetEnum Platform
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets remote security policy.
        /// </summary>
        public Evidence Evidence
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the level of automatic deserialization for .NET Framework remoting.
        /// </summary>
        public TypeFilterLevel TypeFilterLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets environment variables of the remote process.
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a dictionary of assembly names to assembly file locations that will need to be resolved inside AddInDomain.
        /// </summary>
        public Dictionary<AssemblyName, string> ExternalAssemblies
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the priority to run the remote process at.
        /// </summary>
        public ProcessPriorityClass ProcessPriority
        {
            get;
            set;
        }

        /// <summary>
        /// Static Method WriteSetupFile.
        /// </summary>
        /// <param name="addInDomainSetup">Instance of AddInDomainSetup.</param>
        /// <param name="fileName">Setup file name.</param>
        internal static void WriteSetupFile(AddInDomainSetup addInDomainSetup, string fileName)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                formatter.Serialize(fileStream, addInDomainSetup);
            }
        }

        /// <summary>
        /// Static Method ReadSetupFile.
        /// </summary>
        /// <param name="fileName">Setup file name.</param>
        /// <returns>Instance of AddInDomainSetup.</returns>
        internal static AddInDomainSetup ReadSetupFile(string fileName)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                return (AddInDomainSetup)formatter.Deserialize(fileStream);
            }
        }
    }
}
