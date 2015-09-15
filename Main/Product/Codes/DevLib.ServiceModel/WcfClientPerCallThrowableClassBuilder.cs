//-----------------------------------------------------------------------
// <copyright file="WcfClientPerCallThrowableClassBuilder.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;

    /// <summary>
    /// Class WcfClientPerCallThrowableClassBuilder.
    /// </summary>
    /// <typeparam name="TChannel">The channel to be used to connect to the service.</typeparam>
    internal class WcfClientPerCallThrowableClassBuilder<TChannel> : WcfClientPerCallBaseClassBuilder<TChannel> where TChannel : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientPerCallThrowableClassBuilder{TChannel}" /> class.
        /// </summary>
        public WcfClientPerCallThrowableClassBuilder()
            : base(typeof(WcfClientBase<TChannel>))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfClientPerCallThrowableClassBuilder{TChannel}" /> class.
        /// </summary>
        /// <param name="baseClassType">Type of Base Class.</param>
        public WcfClientPerCallThrowableClassBuilder(Type baseClassType)
            : base(baseClassType)
        {
        }
    }
}
