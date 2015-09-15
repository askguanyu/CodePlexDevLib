//-----------------------------------------------------------------------
// <copyright file="ExpandableObjectConverter.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// Provides a type converter to convert expandable objects to and from various other representations.
    /// </summary>
    /// <typeparam name="T">Type of object to convert.</typeparam>
    public class ExpandableObjectConverter<T> : ExpandableObjectConverter
    {
        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert from.</param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture.</param>
        /// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
        /// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                object result = null;

                try
                {
                    result = Activator.CreateInstance(typeof(T));
                }
                catch
                {
                    try
                    {
                        result = FormatterServices.GetUninitializedObject(typeof(T));
                    }
                    catch
                    {
                        try
                        {
                            result = base.ConvertFrom(context, culture, value);
                        }
                        catch
                        {
                        }
                    }
                }

                return result;
            }
        }
    }
}
