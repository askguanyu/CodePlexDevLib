//-----------------------------------------------------------------------
// <copyright file="WcfClientPerSessionThrowableClassBuilder.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;

    /// <summary>
    /// Class WcfClientPerSessionThrowableClassBuilder.
    /// </summary>
    /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
    internal class WcfClientPerSessionThrowableClassBuilder<TChannel> : WcfClientPerSessionBaseClassBuilder<TChannel> where TChannel : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientPerSessionThrowableClassBuilder{TChannel}" /> class.
        /// </summary>
        public WcfClientPerSessionThrowableClassBuilder()
            : base(typeof(WcfClientBase<TChannel>))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientPerSessionThrowableClassBuilder{TChannel}" /> class.
        /// </summary>
        /// <param name="baseClassType">Type of Base Class.</param>
        public WcfClientPerSessionThrowableClassBuilder(Type baseClassType)
            : base(baseClassType)
        {
        }
    }
}
