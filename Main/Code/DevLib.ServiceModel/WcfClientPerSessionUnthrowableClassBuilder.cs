//-----------------------------------------------------------------------
// <copyright file="WcfClientPerSessionUnthrowableClassBuilder.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;
    using System.Reflection.Emit;

    /// <summary>
    /// Class WcfClientPerSessionUnthrowableClassBuilder
    /// </summary>
    /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
    internal class WcfClientPerSessionUnthrowableClassBuilder<TChannel> : WcfClientPerSessionBaseClassBuilder<TChannel> where TChannel : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientPerSessionUnthrowableClassBuilder{TChannel}" /> class.
        /// </summary>
        public WcfClientPerSessionUnthrowableClassBuilder()
            : base(typeof(WcfClientBase<TChannel>))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientPerSessionUnthrowableClassBuilder{TChannel}" /> class.
        /// </summary>
        /// <param name="baseClassType">Type of Base Class.</param>
        public WcfClientPerSessionUnthrowableClassBuilder(Type baseClassType)
            : base(baseClassType)
        {
        }

        /// <summary>
        /// Method GenerateRethrow.
        /// </summary>
        /// <param name="ilGenerator">Instance of ILGenerator.</param>
        protected override void GenerateRethrow(ILGenerator ilGenerator)
        {
        }
    }
}
