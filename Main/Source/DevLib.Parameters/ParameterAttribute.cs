//-----------------------------------------------------------------------
// <copyright file="ParameterAttribute.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Parameters
{
    using System;

    /// <summary>
    /// Attribute for argument properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ParameterAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterAttribute" /> class.
        /// </summary>
        public ParameterAttribute()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterAttribute" /> class.
        /// </summary>
        /// <param name="alias">Argument alias.</param>
        public ParameterAttribute(params string[] alias)
        {
            this.Alias = alias;
            this.Required = false;
            this.DefaultValue = null;
            this.HelpText = null;
        }

        /// <summary>
        /// Gets or sets alias for an argument.
        /// </summary>
        public string[] Alias
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether an argument is required.
        /// </summary>
        public bool Required
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets mapped property default value.
        /// </summary>
        public object DefaultValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a short description of this argument. Usually a sentence summary.
        /// </summary>
        public string HelpText
        {
            get;
            set;
        }
    }
}
