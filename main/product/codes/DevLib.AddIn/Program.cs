namespace DevLib.AddIn
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;

    public class AssemblyResolver : MarshalByRefObject
    {
        private readonly Dictionary<string, Dictionary<AssemblyName, string>> _assemblyDict;

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

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public void Mount()
        {
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ReflectionOnlyResolve;
        }

        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public void Unmount()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= Resolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= ReflectionOnlyResolve;
        }

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

        private Assembly Resolve(object sender, ResolveEventArgs args)
        {
            string assemblyFile = FindAssemblyName(args.Name);

            if (assemblyFile != null)
            {
                return Assembly.LoadFrom(assemblyFile);
            }

            return null;
        }

        private Assembly ReflectionOnlyResolve(object sender, ResolveEventArgs args)
        {
            string assemblyFile = FindAssemblyName(args.Name);

            if (assemblyFile != null)
            {
                return Assembly.ReflectionOnlyLoadFrom(assemblyFile);
            }

            return null;
        }

        private string FindAssemblyName(string name)
        {
            AssemblyName assemblyName = new AssemblyName(name);
            Dictionary<AssemblyName, string> subDict;

            if (!_assemblyDict.TryGetValue(assemblyName.Name, out subDict))
            {
                return null;
            }

            foreach (KeyValuePair<AssemblyName, string> item in subDict)
            {
                if (assemblyName.Version != null && assemblyName.Version != item.Key.Version)
                    continue;

                if (assemblyName.CultureInfo != null && !assemblyName.CultureInfo.Equals(item.Key.CultureInfo))
                    continue;

                if (!PublicKeysTokenEqual(assemblyName.GetPublicKeyToken(), item.Key.GetPublicKeyToken()))
                    continue;

                return item.Value;
            }

            return null;
        }
    }

    class Program
    {
        private static StreamWriter _logFile;

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
                Dictionary<AssemblyName, string> resolveMap = new Dictionary<AssemblyName, string>();
                resolveMap.Add(new AssemblyName("${AddInAssemblyName}"), args[0]);

                AssemblyResolver resolver = new AssemblyResolver(resolveMap);

                resolver.Mount();

                Type hostType = Type.GetType("${AddInActivatorHostTypeName}");

                if (hostType == null)
                {
                    throw new TypeLoadException(string.Format("Could not load AddInActivatorHost type {0} by using resolver with {1} mapped to {2}", "${AddInActivatorHostTypeName}", "${AddInAssemblyName}", args[0]));
                }

                Type[] types = new Type[1];
                types[0] = typeof(string[]);

                MethodInfo methodInfo = hostType.GetMethod("Run", BindingFlags.Static | BindingFlags.Public, null, types, null);

                if (methodInfo == null)
                {
                    throw new Exception("'Run' method on AddInActivatorHost was not found.");
                }

                object[] parameters = new object[1];
                parameters[0] = args;

                methodInfo.Invoke(null, parameters);
            }
            catch (Exception e)
            {
                Log("Failed to launch AddInActivator Host: {0}", e);
            }
        }

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