//-----------------------------------------------------------------------
// <copyright file="IWcfClientTypeBuilder.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;

    /// <summary>
    /// Interface IWcfClientTypeBuilder.
    /// </summary>
    internal interface IWcfClientTypeBuilder
    {
        /// <summary>
        /// Generate type from class name.
        /// </summary>
        /// <param name="className">The class name of type.</param>
        /// <returns>Generated Type.</returns>
        Type GenerateType(string className);
    }
}
