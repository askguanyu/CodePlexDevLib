namespace DevLib.AddIn
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;

    /// <summary>
    /// Class Program.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Static Field _logFile.
        /// </summary>
        private static StreamWriter _logFile;

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
        /// Static Method OpenLogFile.
        /// </summary>
        /// <param name="redirectOutput">Whether redirect console output.</param>
        private static void OpenLogFile(bool redirectOutput)
        {
            string fileName = string.Format("{0}.log", Assembly.GetEntryAssembly().Location);
            string log = string.Format("[{0}] [PID:{1}] [AddInDomain is started.]", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffff"), Process.GetCurrentProcess().Id.ToString());

            if (redirectOutput)
            {
                Console.WriteLine(log);
            }

            try
            {
                _logFile = new StreamWriter(fileName, true);
                _logFile.WriteLine();
                _logFile.WriteLine(log);
                _logFile.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to open AddInDomain bootstrap log: {0}", e.ToString());
            }
        }

        /// <summary>
        /// Static Method Log.
        /// </summary>
        /// <param name="redirectOutput">Whether redirect console output.</param>
        /// <param name="message">Message to log.</param>
        private static void Log(bool redirectOutput, string message)
        {
            string log = string.Format("[{0}] [PID:{1}] [Message: {2}]", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffff"), Process.GetCurrentProcess().Id.ToString(), message);

            if (_logFile == null)
            {
                OpenLogFile(redirectOutput);
            }

            if (redirectOutput)
            {
                Console.WriteLine(log);
            }

            if (_logFile != null)
            {
                _logFile.WriteLine(log);
                _logFile.Flush();
            }
        }
    }

    /// <summary>
    /// Class AssemblyResolver.
    /// </summary>
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
