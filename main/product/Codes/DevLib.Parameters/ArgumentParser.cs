//-----------------------------------------------------------------------
// <copyright file="ArgumentParser.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Parameters
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides utilities for parsing command line values.
    /// </summary>
    public static class ArgumentParser
    {
        /// <summary>
        /// Parse arguments to a ignore case dictionary.
        /// </summary>
        /// <param name="arguments">Arguments to parse.</param>
        /// <returns>A dictionary represents that is aware of command line input patterns. All lookups for keys ignore case.</returns>
        public static Dictionary<string, string> Parse(IList<string> arguments)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (arguments == null || arguments.Count < 1)
            {
                return result;
            }

            Regex delimiterRegex = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex qualifierRegex = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string parameter = null;
            string[] parts;

            foreach (string argument in arguments)
            {
                parts = delimiterRegex.Split(argument, 3);

                switch (parts.Length)
                {
                    case 1:

                        if (parameter != null)
                        {
                            if (!result.ContainsKey(parameter))
                            {
                                parts[0] = qualifierRegex.Replace(parts[0], "$1");
                                result.Add(parameter, parts[0]);
                            }

                            parameter = null;
                        }

                        break;

                    case 2:

                        if (parameter != null)
                        {
                            if (!result.ContainsKey(parameter))
                            {
                                result.Add(parameter, "true");
                            }
                        }

                        parameter = parts[1];

                        break;

                    case 3:

                        if (parameter != null)
                        {
                            if (!result.ContainsKey(parameter))
                            {
                                result.Add(parameter, "true");
                            }
                        }

                        parameter = parts[1];

                        if (!result.ContainsKey(parameter))
                        {
                            parts[2] = qualifierRegex.Replace(parts[2], "$1");
                            result.Add(parameter, parts[2]);
                        }

                        parameter = null;

                        break;
                }
            }

            if (parameter != null)
            {
                if (!result.ContainsKey(parameter))
                {
                    result.Add(parameter, "true");
                }
            }

            return result;
        }

        /// <summary>
        /// Parse argument string to a ignore case dictionary.
        /// </summary>
        /// <param name="argument">Argument string to parse.</param>
        /// <returns>A dictionary represents that is aware of command line input patterns. All lookups for keys ignore case.</returns>
        public static Dictionary<string, string> Parse(string argument)
        {
            return Parse(SplitNested(argument, ' ', '"'));
        }

        /// <summary>
        /// Parse arguments to an object.
        /// </summary>
        /// <typeparam name="T">The type of return object.</typeparam>
        /// <param name="arguments">Arguments to parse.</param>
        /// <returns>An object represents arguments.</returns>
        public static T ParseTo<T>(IList<string> arguments) where T : new()
        {
            T result = new T();

            var parameters = ArgumentParser.Parse(arguments);

            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                if (property.CanWrite && property.CanRead)
                {
                    ParameterAttribute optionAttribute = null;

                    foreach (Attribute attribute in property.GetCustomAttributes(true))
                    {
                        if (attribute.GetType() == typeof(ParameterAttribute))
                        {
                            optionAttribute = attribute as ParameterAttribute;
                            break;
                        }
                    }

                    if (optionAttribute != null)
                    {
                        bool isFound = false;
                        List<string> alias = new List<string>();
                        alias.Add(property.Name);

                        if (optionAttribute.Alias != null && optionAttribute.Alias.Length > 0)
                        {
                            alias.AddRange(optionAttribute.Alias);
                        }

                        foreach (var key in alias)
                        {
                            if (parameters.ContainsKey(key))
                            {
                                isFound = true;
                                property.SetValue(result, ConvertTo(parameters[key], property.PropertyType), null);
                                break;
                            }
                        }

                        if (!isFound)
                        {
                            if (optionAttribute.DefaultValue != null)
                            {
                                property.SetValue(result, optionAttribute.DefaultValue, null);
                            }
                            else if (optionAttribute.Required)
                            {
                                throw new ArgumentException("The property is marked as required but cannot find argument for this property.", property.Name);
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Parse arguments to an object.
        /// </summary>
        /// <typeparam name="T">The type of return object.</typeparam>
        /// <param name="argument">Argument string to parse.</param>
        /// <returns>An object represents arguments.</returns>
        public static T ParseTo<T>(string argument) where T : new()
        {
            return ParseTo<T>(SplitNested(argument, ' ', '"'));
        }

        /// <summary>
        /// Splits string by a specified delimiter and keep nested string with a specified qualifier.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="delimiter">Delimiter character.</param>
        /// <param name="qualifier">Qualifier character.</param>
        /// <returns>A list whose elements contain the substrings in this instance that are delimited by the delimiter.</returns>
        internal static List<string> SplitNested(string source, char delimiter, char qualifier)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            StringBuilder itemStringBuilder = new StringBuilder();
            List<string> result = new List<string>();
            bool inItem = false;
            bool inQuotes = false;

            for (int i = 0; i < source.Length; i++)
            {
                char character = source[i];

                if (!inItem)
                {
                    if (character == delimiter)
                    {
                        result.Add(string.Empty);
                        continue;
                    }

                    if (character == qualifier)
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        itemStringBuilder.Append(character);
                    }

                    inItem = true;
                    continue;
                }

                if (inQuotes)
                {
                    if (character == qualifier && ((source.Length > (i + 1) && source[i + 1] == delimiter) || ((i + 1) == source.Length)))
                    {
                        inQuotes = false;
                        inItem = false;
                        i++;
                    }
                    else if (character == qualifier && source.Length > (i + 1) && source[i + 1] == qualifier)
                    {
                        i++;
                    }
                }
                else if (character == delimiter)
                {
                    inItem = false;
                }

                if (!inItem)
                {
                    result.Add(itemStringBuilder.ToString());
                    itemStringBuilder.Remove(0, itemStringBuilder.Length);
                }
                else
                {
                    itemStringBuilder.Append(character);
                }
            }

            if (inItem)
            {
                result.Add(itemStringBuilder.ToString());
            }

            return result;
        }

        /// <summary>
        /// Converts an object to the specified target type or returns null if those two types are not convertible.
        /// </summary>
        /// <param name="source">The value.</param>
        /// <param name="targetType">The type of returns object.</param>
        /// <returns>The target type object.</returns>
        internal static object ConvertTo(object source, Type targetType)
        {
            if (source == null)
            {
                return null;
            }

            try
            {
                if (source.GetType() == targetType)
                {
                    return source;
                }

                var converter = TypeDescriptor.GetConverter(source);

                if (converter != null && converter.CanConvertTo(targetType))
                {
                    return converter.ConvertTo(source, targetType);
                }

                converter = TypeDescriptor.GetConverter(targetType);

                if (converter != null && converter.CanConvertFrom(source.GetType()))
                {
                    return converter.ConvertFrom(source);
                }

                throw new InvalidOperationException();
            }
            catch
            {
                return null;
            }
        }
    }
}
