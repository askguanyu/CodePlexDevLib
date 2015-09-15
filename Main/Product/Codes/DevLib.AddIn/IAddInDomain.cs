//-----------------------------------------------------------------------
// <copyright file="IAddInDomain.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System.Globalization;
    using System.Reflection;
    using System.Security.Policy;

    /// <summary>
    ///
    /// </summary>
    internal interface IAddInDomain
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        object CreateInstanceAndUnwrap(string assemblyName, string typeName);

        /// <summary>
        ///
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="typeName"></param>
        /// <param name="activationAttributes"></param>
        /// <returns></returns>
        object CreateInstanceAndUnwrap(string assemblyName, string typeName, object[] activationAttributes);

        /// <summary>
        ///
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="typeName"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="bindingAttr"></param>
        /// <param name="binder"></param>
        /// <param name="args"></param>
        /// <param name="culture"></param>
        /// <param name="activationAttributes"></param>
        /// <param name="securityAttributes"></param>
        /// <returns></returns>
        object CreateInstanceAndUnwrap(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes);
    }
}
