//-----------------------------------------------------------------------
// <copyright file="WcfClientAbstractClassBuilder.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;

    /// <summary>
    /// Class WcfClientAbstractClassBuilder.
    /// </summary>
    /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
    internal abstract class WcfClientAbstractClassBuilder<TChannel> : IWcfClientTypeBuilder where TChannel : class
    {
        /// <summary>
        /// Field AssemblyNameStringFormat.
        /// </summary>
        private const string AssemblyNameStringFormat = "DevLib.ServiceModel.{0}";

        /// <summary>
        /// Field AssemblyFileStringFormat.
        /// </summary>
        private const string AssemblyFileStringFormat = "{0}.dll";

        /// <summary>
        /// Field DefaultMethodAttributes.
        /// </summary>
        private const MethodAttributes DefaultMethodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.NewSlot;

        /// <summary>
        /// Field saveGeneratedAssembly.
        /// </summary>
        private static bool saveGeneratedAssembly = false;

        /// <summary>
        /// Field _baseClassType.
        /// </summary>
        private readonly Type _baseClassType;

        /// <summary>
        /// Field _assemblyBuilder.
        /// </summary>
        private AssemblyBuilder _assemblyBuilder;

        /// <summary>
        /// Field _assemblyName.
        /// </summary>
        private AssemblyName _assemblyName;

        /// <summary>
        /// Field _moduleBuilder.
        /// </summary>
        private ModuleBuilder _moduleBuilder;

        /// <summary>
        /// Field _generatedAssemblyName.
        /// </summary>
        private string _generatedAssemblyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientAbstractClassBuilder{TChannel}" /> class.
        /// </summary>
        /// <param name="baseClassType">Base Class Type.</param>
        protected WcfClientAbstractClassBuilder(Type baseClassType)
        {
            this._baseClassType = baseClassType;
        }

        /// <summary>
        /// Gets generated class name.
        /// </summary>
        public string GeneratedClassName
        {
            get;
            private set;
        }

        /// <summary>
        /// Method SaveGeneratedAssembly.
        /// </summary>
        public void SaveGeneratedAssembly()
        {
            if (saveGeneratedAssembly)
            {
                this._assemblyBuilder.Save(string.Format(AssemblyFileStringFormat, this._generatedAssemblyName));
                this._assemblyBuilder = null;
            }
        }

        /// <summary>
        /// Method GenerateType.
        /// </summary>
        /// <param name="className">Class name.</param>
        /// <returns>Type of the class.</returns>
        public Type GenerateType(string className)
        {
            this.GeneratedClassName = className;
            this._generatedAssemblyName = string.Format(AssemblyNameStringFormat, this.GeneratedClassName);

            this.GenerateAssembly();

            Type type = this._moduleBuilder.GetType(this.GeneratedClassName);

            if (type != null)
            {
                return type;
            }

            type = this.GenerateTypeImplementation();

            this.SaveGeneratedAssembly();

            return type;
        }

        /// <summary>
        /// Method GenerateMethodImpl.
        /// </summary>
        /// <param name="typeBuilder">Instance of TypeBuilder.</param>
        protected virtual void GenerateMethodImpl(TypeBuilder typeBuilder)
        {
            this.GenerateMethodImpl(typeBuilder, typeof(TChannel));
        }

        /// <summary>
        /// Method GenerateMethodImpl.
        /// </summary>
        /// <param name="typeBuilder">Instance of TypeBuilder.</param>
        /// <param name="currentType">Type to be processed.</param>
        protected virtual void GenerateMethodImpl(TypeBuilder typeBuilder, Type currentType)
        {
            MethodInfo[] methods = currentType.GetMethods();

            foreach (MethodInfo method in methods)
            {
                Type[] parameterTypes = GetParameterTypeList(method.GetParameters());
                MethodBuilder methodBuilder = typeBuilder.DefineMethod(method.Name, DefaultMethodAttributes, method.ReturnType, parameterTypes);

                methodBuilder.CreateMethodBody(null, 0);
                ILGenerator ilGenerator = methodBuilder.GetILGenerator();

                this.GenerateMethodImpl(method, parameterTypes, ilGenerator);

                typeBuilder.DefineMethodOverride(methodBuilder, method);
            }

            Type[] inheritedInterfaces = currentType.GetInterfaces();

            foreach (Type inheritedInterface in inheritedInterfaces)
            {
                this.GenerateMethodImpl(typeBuilder, inheritedInterface);
            }
        }

        /// <summary>
        /// Method GenerateMethodImpl.
        /// </summary>
        /// <param name="methodInfo">Instance of MethodInfo.</param>
        /// <param name="parameterTypes">Parameter type array.</param>
        /// <param name="ilGenerator">Instance of ILGenerator.</param>
        protected abstract void GenerateMethodImpl(MethodInfo methodInfo, Type[] parameterTypes, ILGenerator ilGenerator);

        /// <summary>
        /// Method IsVoidMethod.
        /// </summary>
        /// <param name="methodInfo">Instance of MethodInfo.</param>
        /// <returns>true if the methodInfo is void method; otherwise, false.</returns>
        protected bool IsVoidMethod(MethodInfo methodInfo)
        {
            return methodInfo.ReturnType == typeof(void);
        }

        /// <summary>
        /// Method GetMethodFromBaseClass.
        /// </summary>
        /// <param name="methodName">Method name.</param>
        /// <returns>Instance of MethodInfo.</returns>
        protected MethodInfo GetMethodFromBaseClass(string methodName)
        {
            return this._baseClassType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty);
        }

        /// <summary>
        /// Method GetParameterTypeList.
        /// </summary>
        /// <param name="declareParams">ParameterInfo list.</param>
        /// <returns>Parameter type list.</returns>
        private static Type[] GetParameterTypeList(IList<ParameterInfo> declareParams)
        {
            List<Type> parameters = new List<Type>();

            foreach (ParameterInfo param in declareParams)
            {
                parameters.Add(param.ParameterType);
            }

            return parameters.ToArray();
        }

        /// <summary>
        /// Method GenerateAssembly.
        /// </summary>
        private void GenerateAssembly()
        {
            try
            {
                if (this._assemblyBuilder == null)
                {
                    this._assemblyName = new AssemblyName();
                    this._assemblyName.Name = this._generatedAssemblyName;
                    this._assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(this._assemblyName, saveGeneratedAssembly ? AssemblyBuilderAccess.RunAndSave : AssemblyBuilderAccess.Run);

                    if (saveGeneratedAssembly)
                    {
                        this._moduleBuilder = this._assemblyBuilder.DefineDynamicModule(this._generatedAssemblyName, string.Format(AssemblyFileStringFormat, this._generatedAssemblyName));
                    }
                    else
                    {
                        this._moduleBuilder = this._assemblyBuilder.DefineDynamicModule(this._generatedAssemblyName);
                    }
                }

                if (this._moduleBuilder == null)
                {
                    throw new InvalidOperationException("Could not generate module builder.");
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Method GenerateTypeImplementation.
        /// </summary>
        /// <returns>Type object.</returns>
        private Type GenerateTypeImplementation()
        {
            TypeBuilder builder;

            try
            {
                builder = this._moduleBuilder.DefineType(string.Format("{0}.{1}", this._generatedAssemblyName, this.GeneratedClassName), TypeAttributes.Public, this._baseClassType);
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
                throw;
            }

            if (builder == null)
            {
                throw new InvalidOperationException(string.Format("Could not DefineType {0} : {1} interface {2}", this.GeneratedClassName ?? string.Empty, this._baseClassType == null ? string.Empty : this._baseClassType.FullName, typeof(TChannel).FullName));
            }

            try
            {
                builder.AddInterfaceImplementation(typeof(TChannel));
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
                throw;
            }

            this.GenerateConstructors(builder);

            this.GenerateMethodImpl(builder);

            try
            {
                return builder.CreateType();
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Method GenerateConstructors.
        /// </summary>
        /// <param name="typeBuilder">Instance of TypeBuilder.</param>
        private void GenerateConstructors(TypeBuilder typeBuilder)
        {
            Type[] constructorParameters = Type.EmptyTypes;
            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.RTSpecialName, CallingConventions.Standard, constructorParameters);
            ILGenerator ilGenerator = constructorBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ConstructorInfo originalConstructor = this._baseClassType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, constructorParameters, null);
            ilGenerator.Emit(OpCodes.Call, originalConstructor);
            ilGenerator.Emit(OpCodes.Ret);

            constructorParameters = new Type[] { typeof(string) };
            constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.RTSpecialName, CallingConventions.Standard, constructorParameters);
            ilGenerator = constructorBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            originalConstructor = this._baseClassType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, constructorParameters, null);
            ilGenerator.Emit(OpCodes.Call, originalConstructor);
            ilGenerator.Emit(OpCodes.Ret);

            constructorParameters = new Type[] { typeof(string), typeof(EndpointAddress) };
            constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.RTSpecialName, CallingConventions.Standard, constructorParameters);
            ilGenerator = constructorBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldarg_2);
            originalConstructor = this._baseClassType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, constructorParameters, null);
            ilGenerator.Emit(OpCodes.Call, originalConstructor);
            ilGenerator.Emit(OpCodes.Ret);

            constructorParameters = new Type[] { typeof(Binding), typeof(EndpointAddress) };
            constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.RTSpecialName, CallingConventions.Standard, constructorParameters);
            ilGenerator = constructorBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldarg_2);
            originalConstructor = this._baseClassType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, constructorParameters, null);
            ilGenerator.Emit(OpCodes.Call, originalConstructor);
            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}
