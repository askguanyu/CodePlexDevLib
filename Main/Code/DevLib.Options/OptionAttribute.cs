//-----------------------------------------------------------------------
// <copyright file="OptionAttribute.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Options
{
    using System;

    /// <summary>
    /// Attribute for argument properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class OptionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionAttribute" /> class.
        /// </summary>
        public OptionAttribute()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionAttribute" /> class.
        /// </summary>
        /// <param name="alias">Argument alias.</param>
        public OptionAttribute(params string[] alias)
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
