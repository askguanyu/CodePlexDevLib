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
        /// Initializes a new instance of the <see cref="GenericDataContractResolver" /> class.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public GenericDataContractResolver()
        {
            this.AddKnownTypeRange(GetReflectTypes(Assembly.GetCallingAssembly()));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericDataContractResolver" /> class.
        /// </summary>
        /// <param name="knownTypes">The known types.</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public GenericDataContractResolver(params Type[] knownTypes)
        {
            this.AddKnownTypeRange(knownTypes);
            this.AddKnownTypeRange(GetReflectTypes(Assembly.GetCallingAssembly()));
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
        /// <param name="assembly">The assembly.</param>
        /// <returns>Reflect types.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static List<Type> GetReflectTypes(Assembly assembly)
        {
            List<Type> result = new List<Type>();

            foreach (Assembly item in GetReferencedAssemblies(assembly))
            {
                result.AddRange(GetAssemblyTypes(item));
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

            return result;
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

            return result;
        }

        /// <summary>
        /// Gets the custom referenced assemblies.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>Custom referenced assemblies.</returns>
        public static List<Assembly> GetCustomReferencedAssemblies(Assembly assembly)
        {
            List<Assembly> result = new List<Assembly>();

            foreach (AssemblyName item in assembly.GetReferencedAssemblies())
            {
                string keyToken = item.FullName.Split('=')[3];

                Assembly module = Assembly.GetCallingAssembly();

                string moduleKeyToken = module.GetName().FullName.Split('=')[3];

                if (keyToken == moduleKeyToken)
                {
                    continue;
                }

                switch (keyToken)
                {
                    case DotNetKeyToken1:
                    case DotNetKeyToken2:
                    case DotNetKeyToken3:
                        continue;
                }

                result.Add(Assembly.Load(item));
            }

            return result;
        }

        /// <summary>
        /// Gets the assembly types.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="onlyPublic">true to get only public; otherwise, false.</param>
        /// <returns>Assembly types.</returns>
        public static List<Type> GetAssemblyTypes(Assembly assembly, bool onlyPublic = true)
        {
            List<Type> result = new List<Type>();

            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsEnum && !type.IsInterface && !type.IsGenericTypeDefinition)
                {
                    if (onlyPublic && !type.IsPublic)
                    {
                        if (!type.IsNested || type.IsNestedPrivate)
                        {
                            continue;
                        }
                    }

                    result.Add(type);
                }
            }

            return result;
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

            return result;
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
        public void AddKnownTypeRange(IList<Type> knownTypes)
        {
            if (knownTypes == null || knownTypes.Count < 1)
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
        /// Override this method to map the specified xsi:type name and namespace to a data contract type during deserialization.
        /// </summary>
        /// <param name="typeName">The xsi:type name to map.</param>
        /// <param name="typeNamespace">The xsi:type namespace to map.</param>
        /// <param name="declaredType">The type declared in the data contract.</param>
        /// <param name="knownTypeResolver">The known type resolver.</param>
        /// <returns>The type the xsi:type name and namespace is mapped to.</returns>
        public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
        {
            if (this._nameToTypeDictionary.ContainsKey(typeNamespace))
            {
                if (this._nameToTypeDictionary[typeNamespace].ContainsKey(typeName))
                {
                    return this._nameToTypeDictionary[typeNamespace][typeName];
                }
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
        public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
        {
            if (this._typeToNameDictionary.ContainsKey(type))
            {
                XmlDictionary dictionary = new XmlDictionary();
                typeNamespace = dictionary.Add(this._typeToNameDictionary[type].Item1);
                typeName = dictionary.Add(this._typeToNameDictionary[type].Item2);

                return true;
            }
            else
            {
                return knownTypeResolver.TryResolveType(type, declaredType, null, out typeName, out typeNamespace);
            }
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
    }
}
