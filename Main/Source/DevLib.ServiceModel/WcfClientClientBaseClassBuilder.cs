//-----------------------------------------------------------------------
// <copyright file="WcfClientClientBaseClassBuilder.cs" company="YuGuan Corporation">
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
    /// Class WcfClientClientBaseClassBuilder.
    /// </summary>
    /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
    internal class WcfClientClientBaseClassBuilder<TChannel> : WcfClientAbstractClassBuilder<TChannel> where TChannel : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientClientBaseClassBuilder{TChannel}" /> class.
        /// </summary>
        public WcfClientClientBaseClassBuilder()
            : base(typeof(ClientBase<TChannel>))
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
            ilGenerator.Emit(OpCodes.Ldarg_0);
            MethodInfo channelProperty = GetMethodFromBaseClass("get_Channel");
            ilGenerator.EmitCall(OpCodes.Call, channelProperty, null);

            for (int index = 0; index < parameterTypes.Length; index++)
            {
                ilGenerator.Emit(OpCodes.Ldarg, ((short)index) + 1);
            }

            ilGenerator.Emit(OpCodes.Callvirt, methodInfo);
            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}
