//-----------------------------------------------------------------------
// <copyright file="WebServiceClientProxyFactoryOptions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Services
{
    using System;

    /// <summary>
    /// Delegate method allows the user of the proxy factory to modify the generated proxy code before it is compiled and used.
    /// </summary>
    /// <param name="proxyCode">The proxy code to modify.</param>
    /// <returns>Modified code.</returns>
    public delegate string ProxyCodeModifier(string proxyCode);

    /// <summary>
    /// Represents options for WebServiceClientProxyFactory.
    /// </summary>
    [Serializable]
    public class WebServiceClientProxyFactoryOptions : MarshalByRefObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceClientProxyFactoryOptions" /> class.
        /// </summary>
        public WebServiceClientProxyFactoryOptions()
        {
            this.Language = LanguageOptions.CS;
            this.GenerateAsync = false;
            this.CodeModifier = null;
        }

        /// <summary>
        /// Language options.
        /// </summary>
        public enum LanguageOptions
        {
            /// <summary>
            /// Specifies CSharp.
            /// </summary>
            CS,

            /// <summary>
            /// Specifies Visual Basic.
            /// </summary>
            VB
        }

        /// <summary>
        /// Gets or sets Language to use.
        /// </summary>
        public LanguageOptions Language
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether generate async methods.
        /// </summary>
        public bool GenerateAsync
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a delegate method that allows the user of the proxy factory to modify the generated proxy code before it is compiled and used.
        /// </summary>
        public ProxyCodeModifier CodeModifier
        {
            get;
            set;
        }

        /// <summary>
        /// Creates and returns a string representation of the current options.
        /// </summary>
        /// <returns>A string representation of the current options.</returns>
        public override string ToString()
        {
            string result = string.Format("WebServiceClientProxyFactoryOptions[Language={0},CodeModifier={1}]", this.Language.ToString(), this.CodeModifier == null ? string.Empty : this.CodeModifier.ToString());

            return result;
        }
    }
}
