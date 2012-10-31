namespace DevLib.AddIn
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;

    /// <summary>
    ///
    /// </summary>
    public class AssemblyResolver : MarshalByRefObject
    {
        /// <summary>
        ///
        /// </summary>
        private readonly Dictionary<string, Dictionary<AssemblyName, string>> _assemblyDict;

        /// <summary>
        ///
        /// </summary>
        /// <param name="dict"></param>
        public AssemblyResolver(Dictionary<AssemblyName, string> dict)
        {
            this._assemblyDict = new Dictionary<string, Dictionary<AssemblyName, string>>();

            if (dict != null)
            {
                foreach (var item in dict)
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
        ///
        /// </summary>
        /// <returns></returns>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public void Mount()
        {
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ReflectionOnlyResolve;
        }

        /// <summary>
        ///
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public void Unmount()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= Resolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= ReflectionOnlyResolve;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
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
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly Resolve(object sender, ResolveEventArgs args)
        {
            string assemblyFile = FindAssemblyName(args.Name);

            if (assemblyFile != null)
            {
                return Assembly.LoadFrom(assemblyFile);
            }

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly ReflectionOnlyResolve(object sender, ResolveEventArgs args)
        {
            string assemblyFile = FindAssemblyName(args.Name);

            if (assemblyFile != null)
            {
                return Assembly.ReflectionOnlyLoadFrom(assemblyFile);
            }

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string FindAssemblyName(string name)
        {
            AssemblyName assemblyName = new AssemblyName(name);
            Dictionary<AssemblyName, string> subDict;

            if (!_assemblyDict.TryGetValue(assemblyName.Name, out subDict))
            {
                return null;
            }

            foreach (var entry in subDict)
            {
                if (assemblyName.Version != null && assemblyName.Version != entry.Key.Version)
                    continue;

                if (assemblyName.CultureInfo != null && !assemblyName.CultureInfo.Equals(entry.Key.CultureInfo))
                    continue;

                if (!PublicKeysTokenEqual(assemblyName.GetPublicKeyToken(), entry.Key.GetPublicKeyToken()))
                    continue;

                return entry.Value;
            }

            return null;
        }
    }

    /// <summary>
    ///
    /// </summary>
    class Program
    {
        /// <summary>
        ///
        /// </summary>
        private static StreamWriter _logFile;

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // args[0] = process domain assembly path
            // args[1] = guid
            // args[2] = process id
            // args[3] = ProcessDomainSetup file

            if (args.Length < 1)
            {
                Log("Invalid arguments");
                return;
            }

            try
            {
                Dictionary<AssemblyName, string> resolveMap = new Dictionary<AssemblyName, string> { { new AssemblyName("${AddInAssemblyName}"), args[0] } };

                AssemblyResolver resolver = new AssemblyResolver(resolveMap);

                resolver.Mount();

                Type hostType = Type.GetType("${AddInActivatorHostTypeName}");

                if (hostType == null)
                {
                    throw new TypeLoadException(string.Format("Could not load AddInActivatorHost type {0} by using resolver with {1} mapped to {2}", "${AddInActivatorHostTypeName}", "${AddInAssemblyName}", args[0]));
                }

                MethodInfo methodInfo = hostType.GetMethod("Run", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string[]) }, null);

                if (methodInfo == null)
                {
                    throw new Exception("'Run' method on AddInActivatorHost was not found.");
                }

                methodInfo.Invoke(null, new[] { args });
            }
            catch (Exception e)
            {
                Log("Failed to launch AddInActivator Host: {0}", e);
            }
        }

        /// <summary>
        ///
        /// </summary>
        private static void OpenLogFile()
        {
            string fileName = string.Format("{0}-{1}.log", Assembly.GetEntryAssembly().Location, Process.GetCurrentProcess().Id);

            try
            {
                _logFile = new StreamWriter(fileName, false);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to open AddInDomain bootstrap log: {0}", e);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        private static void Log(string message, params object[] args)
        {
            if (_logFile == null)
            {
                OpenLogFile();
            }

            if (_logFile != null)
            {
                _logFile.WriteLine(string.Format("[{0}] {1}", DateTime.Now, message), args);
                _logFile.Flush();
            }
        }
    }
}