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
            this.ExeFileDirectory = Path.GetTempPath();
            this.ProcessStartTimeout = new TimeSpan(0, 0, 3);
            this.FileDeleteTimeout = new TimeSpan(0, 0, 3);
            this.DeleteOnUnload = true;
            this.RestartOnProcessExit = true;
            this.AppDomainSetup = AppDomain.CurrentDomain.SetupInformation;
            this.WorkingDirectory = Environment.CurrentDirectory;
            this.Evidence = AppDomain.CurrentDomain.Evidence;
            this.TypeFilterLevel = TypeFilterLevel.Low;
            this.DllDirectory = GetCurrentDllDirectory();
            this.EnvironmentVariables = new Dictionary<string, string>();
            this.ExternalAssemblies = new Dictionary<AssemblyName, string>();
            this.ProcessPriority = ProcessPriorityClass.Normal;
        }

        /// <summary>
        /// Specifies maximum time spent trying to delete a assembly from the disk.
        /// </summary>
        public TimeSpan FileDeleteTimeout
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies where the temporary remote process executable file will be created.
        /// </summary>
        public string ExeFileDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies the working directory for the remote process.
        /// </summary>
        public string WorkingDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies a directory to invoke SetDllDirectory with to redirect DLL probing to the working directory.
        /// </summary>
        public string DllDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies how long to wait for the remote process to start.
        /// </summary>
        public TimeSpan ProcessStartTimeout
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies whether or not to delete the generated executable after AddInDomain has unloaded.
        /// </summary>
        public bool DeleteOnUnload
        {
            get;
            set;
        }

        /// <summary>
        /// Specifices whether AddInDomain should be relaunched when the process exit prematurely.
        /// </summary>
        public bool RestartOnProcessExit
        {
            get;
            set;
        }

        /// <summary>
        /// Setup information for the AppDomain that the object will be created in, in the remote process.
        /// By default, this will be the current domain's setup information from which the proxy is being created.
        /// </summary>
        public AppDomainSetup AppDomainSetup
        {
            get;
            set;
        }

        /// <summary>
        /// Allows specifying which platform to compile the target remote process assembly for.
        /// </summary>
        public PlatformTargetEnum Platform
        {
            get;
            set;
        }

        /// <summary>
        /// Remote security policy.
        /// </summary>
        public Evidence Evidence
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies the level of automatic deserialization for .NET Framework remoting.
        /// </summary>
        public TypeFilterLevel TypeFilterLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Environment variables of the remote process.
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables
        {
            get;
            set;
        }

        /// <summary>
        /// A dictionary of assembly names to assembly file locations that will need to be resolved inside AddInDomain.
        /// </summary>
        public Dictionary<AssemblyName, string> ExternalAssemblies
        {
            get;
            set;
        }

        /// <summary>
        /// The priority to run the remote process at.
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
