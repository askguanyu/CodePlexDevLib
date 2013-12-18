//-----------------------------------------------------------------------
// <copyright file="WcfClientReusableProxyClassBuilder.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Class WcfClientReusableProxyClassBuilder.
    /// </summary>
    /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
    internal class WcfClientReusableProxyClassBuilder<TChannel> : WcfClientAbstractClassBuilder<TChannel> where TChannel : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientReusableProxyClassBuilder{TChannel}" /> class.
        /// </summary>
        public WcfClientReusableProxyClassBuilder()
            : base(typeof(WcfClientReusableProxy<TChannel>))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientReusableProxyClassBuilder{TChannel}" /> class.
        /// </summary>
        /// <param name="baseClassType">Type of Base Class.</param>
        public WcfClientReusableProxyClassBuilder(Type baseClassType)
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

            this.GenerateStandardBeginCatchBlock(ilGenerator);

            ilGenerator.EndExceptionBlock();

            if (hasReturn)
            {
                ilGenerator.Emit(OpCodes.Ldloc_0);
            }

            ilGenerator.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Method GenerateStandardBeginCatchBlock.
        /// </summary>
        /// <param name="ilGenerator">Instance of ILGenerator.</param>
        protected void GenerateStandardBeginCatchBlock(ILGenerator ilGenerator)
        {
            ilGenerator.BeginCatchBlock(typeof(object));
            ilGenerator.Emit(OpCodes.Pop);
            ilGenerator.Emit(OpCodes.Ldarg_0);
            MethodInfo closeProxyMethod = GetMethodFromBaseClass(WcfClientConstants.CloseProxyMethodName);
            ilGenerator.Emit(OpCodes.Call, closeProxyMethod);
        }
    }
}
