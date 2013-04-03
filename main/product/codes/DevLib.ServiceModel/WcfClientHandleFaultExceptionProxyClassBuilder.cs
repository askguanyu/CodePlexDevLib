//-----------------------------------------------------------------------
// <copyright file="WcfClientHandleFaultExceptionProxyClassBuilder.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.ServiceModel;

    /// <summary>
    /// Class WcfClientHandleFaultExceptionProxyClassBuilder.
    /// </summary>
    /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
    internal class WcfClientHandleFaultExceptionProxyClassBuilder<TChannel> : WcfClientReusableProxyClassBuilder<TChannel> where TChannel : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientHandleFaultExceptionProxyClassBuilder{TChannel}" /> class.
        /// </summary>
        public WcfClientHandleFaultExceptionProxyClassBuilder()
            : base(typeof(WcfClientHandleFaultExceptionProxy<TChannel>))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientHandleFaultExceptionProxyClassBuilder{TChannel}" /> class.
        /// </summary>
        /// <param name="baseClassType">Type of Base Class.</param>
        public WcfClientHandleFaultExceptionProxyClassBuilder(Type baseClassType)
            : base(baseClassType)
        {
        }

        /// <summary>
        /// Method GenerateMethodImpl.
        /// </summary>
        /// <param name="methodInfo">Instance of MethodInfo.</param>
        /// <param name="parameterTypes">Parameter type array.</param>
        /// <param name="ilGenerator">Instance of ILGenerator.</param>
        protected override void GenerateMethodImpl(MethodInfo methodInfo, Type[] parameterTypes, ILGenerator ilGenerator)
        {
            bool hasReturn = !IsVoidMethod(methodInfo);

            if (hasReturn)
            {
                ilGenerator.DeclareLocal(methodInfo.ReturnType);
            }

            Label tryLabel = ilGenerator.BeginExceptionBlock();
            {
                ilGenerator.Emit(OpCodes.Ldarg_0);

                MethodInfo proxyProperty = GetMethodFromBaseClass("get_Proxy");
                ilGenerator.EmitCall(OpCodes.Call, proxyProperty, null);
                ParameterInfo[] parameters = methodInfo.GetParameters();

                for (int index = 0; index < parameterTypes.Length; index++)
                {
                    ilGenerator.Emit(OpCodes.Ldarg, ((short)index) + 1);
                }

                ilGenerator.Emit(OpCodes.Callvirt, methodInfo);

                if (hasReturn)
                {
                    ilGenerator.Emit(OpCodes.Stloc_0);
                }
            }

            object[] objAttributes = methodInfo.GetCustomAttributes(typeof(FaultContractAttribute), true);

            int localVariableIndex = 0;

            foreach (FaultContractAttribute faultAttribute in objAttributes)
            {
                if (typeof(Exception).IsAssignableFrom(faultAttribute.DetailType))
                {
                    Type faultExceptionOfDetail = typeof(FaultException<>).MakeGenericType(faultAttribute.DetailType);
                    localVariableIndex = ilGenerator.DeclareLocal(faultExceptionOfDetail).LocalIndex;
                    ilGenerator.BeginCatchBlock(faultExceptionOfDetail);
                    ilGenerator.Emit(OpCodes.Stloc_S, localVariableIndex);
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    MethodInfo closeProxyMethod = GetMethodFromBaseClass(WcfClientConstants.CloseProxyMethodName);
                    ilGenerator.Emit(OpCodes.Call, closeProxyMethod);
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Ldloc_S, localVariableIndex);
                    MethodInfo handleExceptionMethod = GetMethodFromBaseClass(WcfClientConstants.HandleFaultExceptionMethodName);
                    MethodInfo typedHandleExceptionMethod = handleExceptionMethod.MakeGenericMethod(faultAttribute.DetailType);
                    ilGenerator.Emit(OpCodes.Callvirt, typedHandleExceptionMethod);
                    ilGenerator.Emit(OpCodes.Rethrow);
                }
            }

            this.GenerateStandardCatch(ilGenerator);

            ilGenerator.EndExceptionBlock();

            if (hasReturn)
            {
                ilGenerator.Emit(OpCodes.Ldloc_0);
            }

            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}
