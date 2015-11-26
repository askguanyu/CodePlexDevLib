//-----------------------------------------------------------------------
// <copyright file="WcfClientPerSessionBaseClassBuilder.cs" company="YuGuan Corporation">
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
    /// Class WcfClientPerSessionBaseClassBuilder.
    /// </summary>
    /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
    internal class WcfClientPerSessionBaseClassBuilder<TChannel> : WcfClientAbstractClassBuilder<TChannel> where TChannel : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientPerSessionBaseClassBuilder{TChannel}" /> class.
        /// </summary>
        public WcfClientPerSessionBaseClassBuilder()
            : base(typeof(WcfClientBase<TChannel>))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientPerSessionBaseClassBuilder{TChannel}" /> class.
        /// </summary>
        /// <param name="baseClassType">Type of Base Class.</param>
        public WcfClientPerSessionBaseClassBuilder(Type baseClassType)
            : base(baseClassType)
        {
        }

        /// <summary>
        /// Generates the method implementation.
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

            ilGenerator.BeginExceptionBlock();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.EmitCall(OpCodes.Call, this.GetMethodFromBaseClass("get_Proxy"), null);

            for (int index = 0; index < parameterTypes.Length; index++)
            {
                ilGenerator.Emit(OpCodes.Ldarg, ((short)index) + 1);
            }

            ilGenerator.Emit(OpCodes.Callvirt, methodInfo);

            if (hasReturn)
            {
                ilGenerator.Emit(OpCodes.Stloc_0);
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
                    ilGenerator.Emit(OpCodes.Call, this.GetMethodFromBaseClass(WcfClientConstants.CloseProxyMethodName));
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Ldloc_S, localVariableIndex);
                    ilGenerator.Emit(OpCodes.Callvirt, this.GetMethodFromBaseClass(WcfClientConstants.HandleFaultExceptionMethodName).MakeGenericMethod(faultAttribute.DetailType));

                    this.GenerateRethrow(ilGenerator);
                }
            }

            ilGenerator.BeginCatchBlock(typeof(object));
            ilGenerator.Emit(OpCodes.Pop);
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Call, this.GetMethodFromBaseClass(WcfClientConstants.CloseProxyMethodName));

            this.GenerateRethrow(ilGenerator);

            ilGenerator.EndExceptionBlock();

            if (hasReturn)
            {
                ilGenerator.Emit(OpCodes.Ldloc_0);
            }

            ilGenerator.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Method GenerateRethrow.
        /// </summary>
        /// <param name="ilGenerator">Instance of ILGenerator.</param>
        protected virtual void GenerateRethrow(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Rethrow);
        }
    }
}
