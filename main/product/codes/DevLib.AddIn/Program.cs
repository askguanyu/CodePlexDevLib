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
            AppDomain.CurrentDomain.AssemblyResolve += this.Resolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += this.ReflectionOnlyResolve;
        }

        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public void Unmount()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= this.Resolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= this.ReflectionOnlyResolve;
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
            string assemblyFile = this.FindAssemblyName(args.Name);

            if (assemblyFile != null)
            {
                return Assembly.LoadFrom(assemblyFile);
            }

            return null;
        }

        private Assembly ReflectionOnlyResolve(object sender, ResolveEventArgs args)
        {
            string assemblyFile = this.FindAssemblyName(args.Name);

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

            if (!this._assemblyDict.TryGetValue(assemblyName.Name, out subDict))
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
            // args[0] = AddInDomain assembly path
            // args[1] = GUID
            // args[2] = PID
            // args[3] = AddInDomainSetup file

            if (args.Length < 4)
            {
                Console.WriteLine("Invalid arguments");
                Console.WriteLine("args[0] = AddInDomain assembly path");
                Console.WriteLine("args[1] = GUID");
                Console.WriteLine("args[2] = PID");
                Console.WriteLine("args[3] = AddInDomainSetup file");
                Log("Invalid arguments");
                return;
            }

            try
            {
                Log(Environment.CommandLine);

                Dictionary<AssemblyName, string> resolveDict = new Dictionary<AssemblyName, string>();
                resolveDict.Add(new AssemblyName("$[AddInAssemblyName]"), args[0]);

                AssemblyResolver resolver = new AssemblyResolver(resolveDict);

                resolver.Mount();

                Type hostType = Type.GetType("$[AddInActivatorHostTypeName]");

                if (hostType != null)
                {
                    Console.WriteLine("Type.GetType($[AddInActivatorHostTypeName]) Succeed!");
                    Log("Type.GetType($[AddInActivatorHostTypeName]) succeed!");
                }
                else
                {
                    Console.WriteLine(string.Format("Could not load AddInActivatorHost type $[AddInActivatorHostTypeName] by using resolver with $[AddInAssemblyName] mapped to {0}", args[0]));
                    throw new TypeLoadException(string.Format("Could not load AddInActivatorHost type $[AddInActivatorHostTypeName] by using resolver with $[AddInAssemblyName] mapped to {0}", args[0]));
                }

                MethodInfo methodInfo = hostType.GetMethod("Run", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string[]) }, null);

                if (methodInfo != null)
                {
                    Console.WriteLine("GetMethod on AddInActivatorHost succeed!");
                    Log("GetMethod on AddInActivatorHost succeed!");
                }
                else
                {
                    Console.WriteLine("'Run' method on AddInActivatorHost was not found.");
                    Log("'Run' method on AddInActivatorHost was not found.");
                    throw new Exception("'Run' method on AddInActivatorHost was not found.");
                }

                Console.WriteLine("Begin Invoke AddInActivatorHost method...");
                Log("Begin Invoke AddInActivatorHost method...");

                methodInfo.Invoke(null, new object[] { args });
            }
            catch (Exception e)
            {
                Console.WriteLine("Summary: Failed to launch AddInActivatorHost: {0}", e);
                Log("Summary: Failed to launch AddInActivatorHost: {0}", e);
            }
        }

        private static void OpenLogFile()
        {
            string fileName = string.Format("{0}.log", Assembly.GetEntryAssembly().Location);

            try
            {
                _logFile = new StreamWriter(fileName, true);
                _logFile.WriteLine();
                _logFile.WriteLine(string.Format("[{0}] [PID:{1}] Started.", DateTime.Now, Process.GetCurrentProcess().Id.ToString()));
                _logFile.Flush();
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
                _logFile.WriteLine(string.Format("[{0}] [PID:{1}] [Message: {2}]", DateTime.Now, Process.GetCurrentProcess().Id.ToString(), message), args);
                _logFile.Flush();
            }
        }
    }
}