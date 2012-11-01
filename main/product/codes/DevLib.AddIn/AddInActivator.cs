//-----------------------------------------------------------------------
// <copyright file="AddInActivator.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Policy;

    /// <summary>
    ///
    /// </summary>
    [Serializable]
    internal class AddInActivator : MarshalByRefObject
    {
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName)
        {
            return AppDomain.CurrentDomain.CreateInstanceAndUnwrap(assemblyName, typeName);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="typeName"></param>
        /// <param name="activationAttributes"></param>
        /// <returns></returns>
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName, object[] activationAttributes)
        {
            return AppDomain.CurrentDomain.CreateInstanceAndUnwrap(assemblyName, typeName, activationAttributes);
        }

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
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
        {
            return AppDomain.CurrentDomain.CreateInstanceAndUnwrap(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
        }
    }
}
