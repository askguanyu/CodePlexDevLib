//-----------------------------------------------------------------------
// <copyright file="GenericDataContractResolver.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.Xml;

    /// <summary>
    /// Generic DataContractResolver.
    /// </summary>
    public class GenericDataContractResolver : DataContractResolver
    {
        /// <summary>
        /// Field DefaultNamespace.
        /// </summary>
        private const string DefaultNamespace = "global";

        /// <summary>
        /// Field DotNetKeyToken1.
        /// </summary>
        private const string DotNetKeyToken1 = "b77a5c561934e089";

        /// <summary>
        /// Field DotNetKeyToken2.
        /// </summary>
        private const string DotNetKeyToken2 = "b03f5f7f11d50a3a";

        /// <summary>
        /// Field DotNetKeyToken3.
        /// </summary>
        private const string DotNetKeyToken3 = "31bf3856ad364e35";

        /// <summary>
        /// Field _typeToNameDictionary.
        /// </summary>
        private readonly Dictionary<Type, Tuple<string, string>> _typeToNameDictionary = new Dictionary<Type, Tuple<string, string>>();

        /// <summary>
        /// Field _nameToTypeDictionary.
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, Type>> _nameToTypeDictionary = new Dictionary<string, Dictionary<string, Type>>();

        /// <summary>
        /// Field _assemblyPath.
        /// </summary>
        private readonly string _assemblyPath;

        /// <summary>
        /// Field _recursive.
        /// </summary>
        private readonly bool _recursive;

        /// <summary>
        /// Field _assemblyFiles.
        /// </summary>
        private readonly IEnumerable<string> _assemblyFiles;

        /// <summary>
        /// Field _reload.
        /// </summary>
        private readonly bool _reload;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericDataContractResolver" /> class.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public GenericDataContractResolver()
        {
            this.AddKnownTypeRange(GetReflectTypes(Assembly.GetCallingAssembly()));
            this.AddKnownTypeRange(GetReflectTypes(Assembly.GetEntryAssembly()));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericDataContractResolver"/> class.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="recursive">true to get all files from directories and subdirectories; otherwise, false.</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public GenericDataContractResolver(string assemblyPath, bool recursive = false)
            : this()
        {
            this._assemblyPath = assemblyPath;
            this._recursive = recursive;

            this.AddKnownTypeRange(assemblyPath, recursive);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericDataContractResolver" /> class.
        /// </summary>
        /// <param name="assemblyFiles">The assembly files.</param>
        /// <param name="reload">true to reload assembly files every time; otherwise, false.</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public GenericDataContractResolver(IEnumerable<string> assemblyFiles, bool reload = true)
            : this()
        {
            this._assemblyFiles = assemblyFiles;
            this._reload = reload;

            this.AddKnownTypeRange(assemblyFiles);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericDataContractResolver" /> class.
        /// </summary>
        /// <param name="knownTypes">The known types.</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public GenericDataContractResolver(IEnumerable<Type> knownTypes)
            : this()
        {
            this.AddKnownTypeRange(knownTypes);
        }

        /// <summary>
        /// Gets the known types.
        /// </summary>
        public List<Type> KnownTypes
        {
            get
            {
                return this._typeToNameDictionary.Keys.ToList();
            }
        }

        /// <summary>
        /// Gets the reflect types.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="recursive">true to get all files from directories and subdirectories; otherwise, false.</param>
        /// <returns>Reflect types.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static List<Type> GetReflectTypes(string assemblyPath, bool recursive)
        {
            List<Type> result = new List<Type>();

            if (!Directory.Exists(assemblyPath))
            {
                return result;
            }

            try
            {
                foreach (var item in Directory.GetFiles(assemblyPath, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    result.AddRange(GetReflectTypes(item));
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
            }

            return result.Distinct().ToList();
        }

        /// <summary>
        /// Gets the reflect types.
        /// </summary>
        /// <param name="assemblyFile">The assembly file.</param>
        /// <returns>Reflect types.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static List<Type> GetReflectTypes(string assemblyFile)
        {
            if (!File.Exists(assemblyFile))
            {
                return new List<Type>();
            }

            Assembly assembly = null;

            try
            {
                assembly = Assembly.Load(File.ReadAllBytes(assemblyFile));
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                return new List<Type>();
            }

            return GetReflectTypes(assembly);
        }

        /// <summary>
        /// LoadFrom an assembly given its file name or path and gets the reflect types.
        /// </summary>
        /// <param name="assemblyFile">The assembly file.</param>
        /// <returns>Reflect types.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static List<Type> LoadTypesFrom(string assemblyFile)
        {
            if (!File.Exists(assemblyFile))
            {
                return new List<Type>();
            }

            Assembly assembly = null;

            try
            {
                assembly = Assembly.LoadFrom(assemblyFile);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                return new List<Type>();
            }

            return GetReflectTypes(assembly);
        }

        /// <summary>
        /// Gets the reflect types.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>Reflect types.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static List<Type> GetReflectTypes(Assembly assembly)
        {
            List<Type> result = new List<Type>();

            if (assembly == null)
            {
                return result;
            }

            if (assembly.FullName.StartsWith("mscorlib"))
            {
                return result;
            }

            string keyToken = assembly.FullName.Split('=')[3];

            switch (keyToken)
            {
                case DotNetKeyToken1:
                case DotNetKeyToken2:
                case DotNetKeyToken3:
                    return result;
            }

            foreach (Assembly item in GetReferencedAssemblies(assembly))
            {
                result.AddRange(GetAssemblyTypes(item, true));
            }

            if (assembly.FullName != typeof(ServiceHost).Assembly.FullName)
            {
                result.AddRange(GetAssemblyTypes(assembly, false));
            }

            if (Assembly.GetEntryAssembly() != null)
            {
                if (Assembly.GetEntryAssembly().FullName != assembly.FullName)
                {
                    result.AddRange(GetAssemblyTypes(Assembly.GetEntryAssembly(), false));
                }
            }
            else
            {
                foreach (Assembly item in GetWebAssemblies())
                {
                    result.AddRange(GetAssemblyTypes(item, false));
                }
            }

            return result.Distinct().ToList();
        }

        /// <summary>
        /// Gets the reflect types.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns>Reflect types.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static List<Type> GetReflectTypes(IEnumerable<Assembly> assemblies)
        {
            List<Type> result = new List<Type>();

            if (assemblies == null || assemblies.Count() < 1)
            {
                return result;
            }

            foreach (var item in assemblies)
            {
                if (item.FullName.StartsWith("mscorlib"))
                {
                    continue;
                }

                string keyToken = item.FullName.Split('=')[3];

                switch (keyToken)
                {
                    case DotNetKeyToken1:
                    case DotNetKeyToken2:
                    case DotNetKeyToken3:
                        continue;
                }

                result.AddRange(GetReflectTypes(item));
            }

            return result.Distinct().ToList();
        }

        /// <summary>
        /// Gets the referenced assemblies.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>Referenced assemblies.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static List<Assembly> GetReferencedAssemblies(Assembly assembly)
        {
            List<Assembly> result = new List<Assembly>();

            if (assembly == null)
            {
                return result;
            }

            if (assembly.FullName.StartsWith("mscorlib"))
            {
                return result;
            }

            string keyToken = assembly.FullName.Split('=')[3];

            switch (keyToken)
            {
                case DotNetKeyToken1:
                case DotNetKeyToken2:
                case DotNetKeyToken3:
                    return result;
            }

            if (assembly.FullName != typeof(ServiceHost).Assembly.FullName)
            {
                result.AddRange(GetCustomReferencedAssemblies(assembly));
            }

            if (Assembly.GetEntryAssembly() != null)
            {
                if (Assembly.GetEntryAssembly().FullName != assembly.FullName)
                {
                    result.AddRange(GetCustomReferencedAssemblies(Assembly.GetEntryAssembly()));
                }
            }
            else
            {
                foreach (Assembly item in GetWebAssemblies())
                {
                    result.AddRange(GetCustomReferencedAssemblies(item));
                }
            }

            return result.Distinct().ToList();
        }

        /// <summary>
        /// Gets the custom referenced assemblies.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>Custom referenced assemblies.</returns>
        public static List<Assembly> GetCustomReferencedAssemblies(Assembly assembly)
        {
            List<Assembly> result = new List<Assembly>();

            if (assembly == null)
            {
                return result;
            }

            if (assembly.FullName.StartsWith("mscorlib"))
            {
                return result;
            }

            string keyToken = assembly.FullName.Split('=')[3];

            switch (keyToken)
            {
                case DotNetKeyToken1:
                case DotNetKeyToken2:
                case DotNetKeyToken3:
                    return result;
            }

            foreach (AssemblyName item in assembly.GetReferencedAssemblies())
            {
                if (item.FullName.StartsWith("mscorlib"))
                {
                    continue;
                }

                string itemKeyToken = item.FullName.Split('=')[3];

                Assembly callingAssembly = Assembly.GetCallingAssembly();

                string callingAssemblyKeyToken = callingAssembly.GetName().FullName.Split('=')[3];

                if (itemKeyToken == callingAssemblyKeyToken)
                {
                    continue;
                }

                switch (itemKeyToken)
                {
                    case DotNetKeyToken1:
                    case DotNetKeyToken2:
                    case DotNetKeyToken3:
                        continue;
                }

                result.Add(Assembly.Load(item));
            }

            return result.Distinct().ToList();
        }

        /// <summary>
        /// Gets the assembly types.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="isPublicOnly">true to only get public; false to get public and non public.</param>
        /// <returns>Assembly types.</returns>
        public static List<Type> GetAssemblyTypes(Assembly assembly, bool isPublicOnly)
        {
            List<Type> result = new List<Type>();

            if (assembly == null)
            {
                return result;
            }

            if (assembly.FullName.StartsWith("mscorlib"))
            {
                return result;
            }

            string keyToken = assembly.FullName.Split('=')[3];

            switch (keyToken)
            {
                case DotNetKeyToken1:
                case DotNetKeyToken2:
                case DotNetKeyToken3:
                    return result;
            }

            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsEnum && !type.IsInterface && !type.IsGenericTypeDefinition && !CanConvert(type))
                {
                    if (isPublicOnly && !type.IsPublic)
                    {
                        if (!type.IsNested || type.IsNestedPrivate)
                        {
                            continue;
                        }
                    }

                    result.Add(type);
                }
            }

            return result.Distinct().ToList();
        }

        /// <summary>
        /// Gets the web assemblies.
        /// </summary>
        /// <returns>Web assemblies.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static List<Assembly> GetWebAssemblies()
        {
            List<Assembly> result = new List<Assembly>();

            if (Assembly.GetEntryAssembly() != null)
            {
                throw new InvalidOperationException("Can only call in a web assembly.");
            }

            foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
            {
                if (module.ModuleName.StartsWith("App_Code.") && module.ModuleName.EndsWith(".dll"))
                {
                    result.Add(Assembly.LoadFrom(module.FileName));
                }

                if (module.ModuleName.StartsWith("App_Web_") && module.ModuleName.EndsWith(".dll"))
                {
                    result.Add(Assembly.LoadFrom(module.FileName));
                }
            }

            if (result.Count == 0)
            {
                throw new InvalidOperationException("Could not find dynamic assembly");
            }

            return result.Distinct().ToList();
        }

        /// <summary>
        /// Determines whether the CurrentProcess is web process.
        /// </summary>
        /// <returns>true if the CurrentProcess is web process; otherwise, false.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static bool IsWebProcess()
        {
            if (Assembly.GetEntryAssembly() != null)
            {
                return false;
            }

            string processName = Process.GetCurrentProcess().ProcessName;

            return processName == "w3wp" || processName == "WebDev.WebServer40";
        }

        /// <summary>
        /// Adds the known type range.
        /// </summary>
        /// <param name="knownTypes">The known types.</param>
        public void AddKnownTypeRange(IEnumerable<Type> knownTypes)
        {
            if (knownTypes == null || knownTypes.Count() < 1)
            {
                return;
            }

            foreach (Type type in knownTypes)
            {
                string typeNamespace = type.Namespace ?? DefaultNamespace;
                string typeName = type.Name;

                this._typeToNameDictionary[type] = new Tuple<string, string>(typeNamespace, typeName);

                if (!this._nameToTypeDictionary.ContainsKey(typeNamespace))
                {
                    this._nameToTypeDictionary[typeNamespace] = new Dictionary<string, Type>();
                }

                this._nameToTypeDictionary[typeNamespace][typeName] = type;
            }
        }

        /// <summary>
        /// Adds the known type range.
        /// </summary>
        /// <param name="assemblyFiles">The assembly files.</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public void AddKnownTypeRange(IEnumerable<string> assemblyFiles)
        {
            if (assemblyFiles == null || assemblyFiles.Count() < 1)
            {
                return;
            }

            foreach (var item in assemblyFiles)
            {
                this.AddKnownTypeRange(GetReflectTypes(item));
            }
        }

        /// <summary>
        /// Adds the known type range.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="recursive">true to get all files from directories and subdirectories; otherwise, false.</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public void AddKnownTypeRange(string assemblyPath, bool recursive = false)
        {
            if (!Directory.Exists(assemblyPath))
            {
                return;
            }

            try
            {
                foreach (var item in Directory.GetFiles(assemblyPath, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    this.AddKnownTypeRange(GetReflectTypes(item));
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
            }
        }

        /// <summary>
        /// Override this method to map the specified xsi:type name and namespace to a data contract type during deserialization.
        /// </summary>
        /// <param name="typeName">The xsi:type name to map.</param>
        /// <param name="typeNamespace">The xsi:type namespace to map.</param>
        /// <param name="declaredType">The type declared in the data contract.</param>
        /// <param name="knownTypeResolver">The known type resolver.</param>
        /// <returns>The type the xsi:type name and namespace is mapped to.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
        {
            if (this._nameToTypeDictionary.ContainsKey(typeNamespace) && this._nameToTypeDictionary[typeNamespace].ContainsKey(typeName))
            {
                Type result = this._nameToTypeDictionary[typeNamespace][typeName];

                if (result.IsSubclassOf(declaredType))
                {
                    return result;
                }
            }

            if (Directory.Exists(this._assemblyPath))
            {
                this.AddKnownTypeRange(this._assemblyPath, this._recursive);
            }

            if (this._assemblyFiles != null && this._assemblyFiles.Count() > 0 && this._reload)
            {
                this.AddKnownTypeRange(this._assemblyFiles);
            }

            this.AddKnownTypeRange(LoadTypesFrom(typeNamespace + ".dll"));
            this.AddKnownTypeRange(LoadTypesFrom(typeNamespace + ".exe"));

            if (this._nameToTypeDictionary.ContainsKey(typeNamespace) && this._nameToTypeDictionary[typeNamespace].ContainsKey(typeName))
            {
                return this._nameToTypeDictionary[typeNamespace][typeName];
            }

            return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, null);
        }

        /// <summary>
        /// Override this method to map a data contract type to an xsi:type name and namespace during serialization.
        /// </summary>
        /// <param name="type">The type to map.</param>
        /// <param name="declaredType">The type declared in the data contract.</param>
        /// <param name="knownTypeResolver">The known type resolver.</param>
        /// <param name="typeName">The xsi:type name.</param>
        /// <param name="typeNamespace">The xsi:type namespace.</param>
        /// <returns>true if mapping succeeded; otherwise, false.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
        {
            if (CanConvert(type))
            {
                return knownTypeResolver.TryResolveType(type, declaredType, null, out typeName, out typeNamespace);
            }

            if (type.IsSubclassOf(declaredType))
            {
                XmlDictionary xmlDictionary = new XmlDictionary();
                typeName = xmlDictionary.Add(type.Name);
                typeNamespace = xmlDictionary.Add(type.Namespace);

                return true;
            }

            if (this._typeToNameDictionary.ContainsKey(type))
            {
                XmlDictionary xmlDictionary = new XmlDictionary();
                typeName = xmlDictionary.Add(this._typeToNameDictionary[type].Item2);
                typeNamespace = xmlDictionary.Add(this._typeToNameDictionary[type].Item1);

                return true;
            }

            if (Directory.Exists(this._assemblyPath))
            {
                this.AddKnownTypeRange(this._assemblyPath, this._recursive);
            }

            if (this._assemblyFiles != null && this._assemblyFiles.Count() > 0 && this._reload)
            {
                this.AddKnownTypeRange(this._assemblyFiles);
            }

            if (this._typeToNameDictionary.ContainsKey(type))
            {
                XmlDictionary xmlDictionary = new XmlDictionary();
                typeName = xmlDictionary.Add(this._typeToNameDictionary[type].Item2);
                typeNamespace = xmlDictionary.Add(this._typeToNameDictionary[type].Item1);

                return true;
            }

            return knownTypeResolver.TryResolveType(type, declaredType, null, out typeName, out typeNamespace);
        }

        /// <summary>
        /// Adds the specified resolver.
        /// </summary>
        /// <param name="resolver">The resolver.</param>
        public void Add(GenericDataContractResolver resolver)
        {
            if (resolver == null || resolver.KnownTypes.Count < 1)
            {
                return;
            }

            this.AddKnownTypeRange(resolver.KnownTypes);
        }

        /// <summary>
        /// Returns whether this converter can convert the object to a <see cref="T:System.String" /> and vice versa.
        /// </summary>
        /// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert.</param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        private static bool CanConvert(Type sourceType)
        {
            if (Type.GetTypeCode(sourceType) == TypeCode.Object &&
                !sourceType.IsEnum &&
                sourceType != typeof(Guid) &&
                sourceType != typeof(TimeSpan) &&
                sourceType != typeof(DateTimeOffset) &&
                !IsNullableCanConvert(sourceType))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Method IsNullableCanConvert.
        /// </summary>
        /// <param name="value">The type to check.</param>
        /// <returns>true if the type is Nullable{} type; otherwise, false.</returns>
        private static bool IsNullableCanConvert(Type value)
        {
            return value.IsGenericType && value.GetGenericTypeDefinition() == typeof(Nullable<>) && CanConvert(Nullable.GetUnderlyingType(value));
        }
    }
}
