//-----------------------------------------------------------------------
// <copyright file="AddInDomainSetup.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Text;

    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public sealed class AddInDomainSetup
    {
        /// <summary>
        ///
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public AddInDomainSetup()
        {
            this.AppDomainSetup = AppDomain.CurrentDomain.SetupInformation;
            this.DeleteOnUnload = true;
            this.DllDirectory = GetCurrentDllDirectory();
            this.EnvironmentVariables = new Dictionary<string, string>();
            this.Evidence = AppDomain.CurrentDomain.Evidence;
            this.ExeFileDirectory = Path.GetTempPath();
            this.ExternalAssemblies = new Dictionary<AssemblyName, string>();
            this.FileDeleteTimeout = new TimeSpan(0, 0, 3);
            this.Platform = PlatformTargetEnum.AnyCPU;
            this.ProcessPriority = ProcessPriorityClass.Normal;
            this.ProcessStartTimeout = new TimeSpan(0, 0, 10);
            this.RestartOnProcessExit = true;
            this.TypeFilterLevel = TypeFilterLevel.Low;
            this.WorkingDirectory = Environment.CurrentDirectory;
        }

        /// <summary>
        /// Gets or sets maximum time spent trying to delete a assembly from the disk.
        /// </summary>
        public TimeSpan FileDeleteTimeout
        {
            get;
            set;
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
        /// Gets or sets whether or not to delete the generated executable after AddInDomain has unloaded.
        /// </summary>
        public bool DeleteOnUnload
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether AddInDomain should be relaunched when the process exit prematurely.
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
        /// Gets the currently configured DLL search path as set by SetDllDirectory.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public static string GetCurrentDllDirectory()
        {
            int bytesNeeded = NativeMethods.GetDllDirectory(0, null);

            if (bytesNeeded == 0)
            {
                throw new Win32Exception();
            }

            StringBuilder stringBuilder = new StringBuilder(bytesNeeded);
            NativeMethods.SetLastError(0);
            bytesNeeded = NativeMethods.GetDllDirectory(bytesNeeded, stringBuilder);

            if (bytesNeeded == 0)
            {
                int errorCode = Marshal.GetLastWin32Error();

                if (errorCode != 0)
                {
                    throw new Win32Exception(errorCode);
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="addInDomainSetup"></param>
        /// <param name="fileName"></param>
        internal static void WriteSetupFile(AddInDomainSetup addInDomainSetup, string fileName)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                formatter.Serialize(fileStream, addInDomainSetup);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
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
