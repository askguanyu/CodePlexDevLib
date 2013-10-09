//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Text;

    /// <summary>
    /// Class Program.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "Reviewed.")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1400:AccessModifierMustBeDeclared", Justification = "Reviewed.")]
    class Program
    {
        /// <summary>
        /// Field SyncRoot.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Method Main, entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            //// args[0] = AddInDomain assembly path
            //// args[1] = GUID
            //// args[2] = PID
            //// args[3] = AddInDomainSetup file
            //// args[4] = Redirect output or not

            if (args.Length < 4)
            {
                Console.WriteLine("Invalid arguments");
                Console.WriteLine("args[0] = AddInDomain assembly path");
                Console.WriteLine("args[1] = GUID");
                Console.WriteLine("args[2] = PID");
                Console.WriteLine("args[3] = AddInDomainSetup file");
                Console.WriteLine("args[4] = Redirect output or not");
                Log(false, "Invalid arguments");
                return;
            }

            bool redirectOutput = false;

            bool.TryParse(args[4], out redirectOutput);

            try
            {
                Log(redirectOutput, "AddInDomain is started");
                Log(redirectOutput, Environment.CommandLine);

                Dictionary<AssemblyName, string> resolveDict = new Dictionary<AssemblyName, string>();
                resolveDict.Add(new AssemblyName("$[AddInAssemblyName]"), args[0]);

                AssemblyResolver resolver = new AssemblyResolver(resolveDict);

                resolver.Mount();

                Type hostType = Type.GetType("$[AddInActivatorHostTypeName]");

                if (hostType != null)
                {
                    Log(redirectOutput, "Succeeded: Type.GetType($[AddInActivatorHostTypeName])");
                }
                else
                {
                    Log(redirectOutput, string.Format("Could not load AddInActivatorHost type $[AddInActivatorHostTypeName] by using resolver with $[AddInAssemblyName] mapped to {0}", args[0]));
                    throw new TypeLoadException(string.Format("Could not load AddInActivatorHost type $[AddInActivatorHostTypeName] by using resolver with $[AddInAssemblyName] mapped to {0}", args[0]));
                }

                MethodInfo methodInfo = hostType.GetMethod("Run", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string[]) }, null);

                if (methodInfo != null)
                {
                    Log(redirectOutput, "Succeeded: GetMethod on AddInActivatorHost");
                }
                else
                {
                    Log(redirectOutput, "'Run' method on AddInActivatorHost was not found.");
                    throw new Exception("'Run' method on AddInActivatorHost was not found.");
                }

                Log(redirectOutput, "Begin Invoke AddInActivatorHost method.");

                methodInfo.Invoke(null, new object[] { args });
            }
            catch (Exception e)
            {
                Log(redirectOutput, string.Format("Summary: Failed to launch AddInActivatorHost:\r\n{0}", e.ToString()));
            }
        }

        /// <summary>
        /// Static Method Log.
        /// </summary>
        /// <param name="redirectOutput">Whether redirect console output.</param>
        /// <param name="message">Message to log.</param>
        private static void Log(bool redirectOutput, string message)
        {
            string log = string.Format(
                "[{0}] [{1}] [{2}] [PID:{3}] [{4}]",
                DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffUTCzzz"),
                "INFO",
                Environment.UserName,
                Process.GetCurrentProcess().Id.ToString(),
                message);

            if (redirectOutput)
            {
                Console.WriteLine(log);
            }

            lock (SyncRoot)
            {
                FileStream fileStream = null;

                try
                {
                    fileStream = File.Open(string.Format("{0}.log", Assembly.GetEntryAssembly().Location), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

                    if (fileStream.Length > 10485760)
                    {
                        fileStream.SetLength(0);
                    }

                    byte[] bytes = Encoding.Default.GetBytes(log + Environment.NewLine);

                    fileStream.Seek(0, SeekOrigin.End);
                    fileStream.Write(bytes, 0, bytes.Length);
                    fileStream.Flush();
                }
                catch
                {
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Dispose();
                        fileStream = null;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Class AssemblyResolver.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed.")]
    public class AssemblyResolver : MarshalByRefObject
    {
        /// <summary>
        /// Readonly Field _assemblyDict.
        /// </summary>
        private readonly Dictionary<string, Dictionary<AssemblyName, string>> _assemblyDict;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyResolver" /> class.
        /// </summary>
        /// <param name="dict">Instance of Dictionary.</param>
        public AssemblyResolver(Dictionary<AssemblyName, string> dict)
        {
            this._assemblyDict = new Dictionary<string, Dictionary<AssemblyName, string>>();

            if (dict != null)
            {
                foreach (KeyValuePair<AssemblyName, string> item in dict)
                {
                    Dictionary<AssemblyName, string> subDict;

                    if (!this._assemblyDict.TryGetValue(item.Key.Name, out subDict))
                    {
                        this._assemblyDict[item.Key.Name] = subDict = new Dictionary<AssemblyName, string>();
                    }

                    subDict[item.Key] = item.Value;
                }
            }
        }

        /// <summary>
        /// Gives the <see cref="T:System.AppDomain" /> an infinite lifetime by preventing a lease from being created.
        /// </summary>
        /// <exception cref="T:System.AppDomainUnloadedException">The operation is attempted on an unloaded application domain.</exception>
        /// <returns>Always null.</returns>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// Subscribe events.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public void Mount()
        {
            AppDomain.CurrentDomain.AssemblyResolve += this.Resolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += this.ReflectionOnlyResolve;
        }

        /// <summary>
        /// Unsubscribe events.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public void Unmount()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= this.Resolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= this.ReflectionOnlyResolve;
        }

        /// <summary>
        /// Static Method PublicKeysTokenEqual.
        /// </summary>
        /// <param name="left">Left token.</param>
        /// <param name="right">Right token.</param>
        /// <returns>true if equals; otherwise, false.</returns>
        private static bool PublicKeysTokenEqual(byte[] left, byte[] right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }

            if (left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Method Resolve.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="args">Instance of ResolveEventArgs.</param>
        /// <returns>Instance of Assembly.</returns>
        private Assembly Resolve(object sender, ResolveEventArgs args)
        {
            string assemblyFile = this.FindAssemblyName(args.Name);

            if (assemblyFile != null)
            {
                return Assembly.LoadFrom(assemblyFile);
            }

            return null;
        }

        /// <summary>
        /// Method ReflectionOnlyResolve.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="args">Instance of ResolveEventArgs.</param>
        /// <returns>Instance of Assembly.</returns>
        private Assembly ReflectionOnlyResolve(object sender, ResolveEventArgs args)
        {
            string assemblyFile = this.FindAssemblyName(args.Name);

            if (assemblyFile != null)
            {
                return Assembly.ReflectionOnlyLoadFrom(assemblyFile);
            }

            return null;
        }

        /// <summary>
        /// Method FindAssemblyName.
        /// </summary>
        /// <param name="name">The display name of the assembly, as returned by the <see cref="P:System.Reflection.AssemblyName.FullName" /> property.</param>
        /// <returns>AssemblyName string.</returns>
        private string FindAssemblyName(string name)
        {
            AssemblyName assemblyName = new AssemblyName(name);
            Dictionary<AssemblyName, string> subDict;

            if (!this._assemblyDict.TryGetValue(assemblyName.Name, out subDict))
            {
                return null;
            }

            foreach (KeyValuePair<AssemblyName, string> item in subDict)
            {
                if (assemblyName.Version != null && assemblyName.Version != item.Key.Version)
                {
                    continue;
                }

                if (assemblyName.CultureInfo != null && !assemblyName.CultureInfo.Equals(item.Key.CultureInfo))
                {
                    continue;
                }

                if (!PublicKeysTokenEqual(assemblyName.GetPublicKeyToken(), item.Key.GetPublicKeyToken()))
                {
                    continue;
                }

                return item.Value;
            }

            return null;
        }
    }
}
