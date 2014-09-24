//-----------------------------------------------------------------------
// <copyright file="EnumExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// Enum Extensions.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Convert string to enum.
        /// </summary>
        /// <typeparam name="TEnum">The type of returns enum.</typeparam>
        /// <param name="source">Source string.</param>
        /// <param name="defaultValue">Default value of enum.</param>
        /// <param name="ignoreCase">Whether ignore case.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>Instance of TEnum.</returns>
        public static TEnum ToEnum<TEnum>(this string source, TEnum defaultValue = default(TEnum), bool ignoreCase = false, bool throwOnError = false) where TEnum : struct
        {
            try
            {
                return (TEnum)Enum.Parse(typeof(TEnum), source, ignoreCase);
            }
            catch
            {
                if (throwOnError)
                {
                    throw;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Whether string is in enum.
        /// </summary>
        /// <typeparam name="TEnum">The type of enum.</typeparam>
        /// <param name="source">Source string.</param>
        /// <returns>true if string in enum; otherwise, false.</returns>
        public static bool IsItemInEnum<TEnum>(this string source) where TEnum : struct
        {
            return Enum.IsDefined(typeof(TEnum), source);
        }

        /// <summary>
        /// If the specified Enum has a System.ComponentModel.DescriptionAttribute defined, the defined description is returned. Otherwise call ToString() on the Enum value.
        /// </summary>
        /// <param name="source">Source enum.</param>
        /// <returns>Description string.</returns>
        public static string ToDescriptionString(this Enum source)
        {
            DescriptionAttribute descriptionAttribute = source.GetType().GetField(source.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
            return (descriptionAttribute != null) ? descriptionAttribute.Description : source.ToString();
        }

        /// <summary>
        /// Retrieves a list of the values of the constants in a specified enumeration.
        /// </summary>
        /// <typeparam name="TEnum">An enumeration type.</typeparam>
        /// <param name="source">Source enum.</param>
        /// <returns>A list of the values of the constants in enumType. The elements of the array are sorted by the binary values of the enumeration constants.</returns>
        public static List<TEnum> Foo<TEnum>(this TEnum source) where TEnum : struct
        {
            return Enum.GetValues(source.GetType()).Cast<TEnum>().ToList();
        }
    }
}
