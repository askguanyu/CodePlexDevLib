//-----------------------------------------------------------------------
// <copyright file="DynamicClientProxyFactorySetup.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceModel
{
    using System;

    /// <summary>
    /// Delegate method allows the user of the proxy factory to modify the generated proxy code before it is compiled and used.
    /// </summary>
    /// <param name="proxyCode">The proxy code to modify.</param>
    /// <returns>Modified code.</returns>
    public delegate string ProxyCodeModifier(string proxyCode);

    /// <summary>
    /// Represents setup information for DynamicClientProxyFactory.
    /// </summary>
    [Serializable]
    public class DynamicClientProxyFactorySetup : MarshalByRefObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicClientProxyFactorySetup" /> class.
        /// </summary>
        public DynamicClientProxyFactorySetup()
        {
            this.Language = LanguageOptions.CS;
            this.GenerateAsync = false;
            this.FormatMode = FormatModeOptions.Auto;
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
        /// Format mode options.
        /// </summary>
        public enum FormatModeOptions
        {
            /// <summary>
            /// Specifies Auto.
            /// </summary>
            Auto,

            /// <summary>
            /// Specifies Xml serializer.
            /// </summary>
            XmlSerializer,

            /// <summary>
            /// Specifies DataContract serializer.
            /// </summary>
            DataContractSerializer
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
        /// Gets or sets format mode.
        /// </summary>
        public FormatModeOptions FormatMode
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
        /// Creates and returns a string representation of the current setup.
        /// </summary>
        /// <returns>A string representation of the current setup.</returns>
        public override string ToString()
        {
            string result = string.Format("DynamicClientProxyFactorySetup[Language={0},FormatMode={1},CodeModifier={2}]", this.Language.ToString(), this.FormatMode.ToString(), this.CodeModifier == null ? string.Empty : this.CodeModifier.ToString());

            return result;
        }
    }
}
