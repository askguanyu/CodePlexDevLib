//-----------------------------------------------------------------------
// <copyright file="Argument.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Options
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides utilities for parsing command line arguments. All lookups for keys ignore case.
    /// </summary>
    public class Argument : IEnumerable<KeyValuePair<string, string>>, IEnumerable
    {
        /// <summary>
        /// Field _parameters.
        /// </summary>
        private readonly Dictionary<string, string> _parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="Argument" /> class.
        /// </summary>
        /// <param name="arguments">Arguments to parse.</param>
        public Argument(IList<string> arguments)
        {
            this._parameters = Parse(arguments);
        }

        /// <summary>
        /// Gets parameter value.
        /// </summary>
        /// <param name="parameter">Parameter to get.</param>
        /// <returns>Parameter value.</returns>
        public string this[string parameter]
        {
            get
            {
                this.CheckNull(parameter);

                if (this._parameters.ContainsKey(parameter))
                {
                    return this._parameters[parameter];
                }
                else
                {
                    return null;
                }
            }
        }

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
            return Parse(SplitNest(argument, ' ', '"'));
        }

        /// <summary>
        /// Gets parameter value.
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        /// <param name="parameter">Parameter to get.</param>
        /// <param name="defaultValue">Default value for the parameter if get failed.</param>
        /// <returns>Parameter value.</returns>
        public T GetValue<T>(string parameter, T defaultValue = default(T))
        {
            this.CheckNull(parameter);

            string value = null;

            if (this._parameters.ContainsKey(parameter))
            {
                value = this._parameters[parameter];
            }
            else
            {
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Check whether has the parameter or not.
        /// </summary>
        /// <param name="parameter">Parameter to check.</param>
        /// <returns>true if parameter exists; otherwise, false.</returns>
        public bool HasParameter(string parameter)
        {
            this.CheckNull(parameter);

            return this._parameters.ContainsKey(parameter);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A System.Collections.Generic.IEnumerator{KeyValuePair{string, string}} that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return this._parameters.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An System.Collections.IEnumerator object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this._parameters).GetEnumerator();
        }

        /// <summary>
        /// Splits string by a specified delimiter and keep nested string with a specified qualifier.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="delimiter">Delimiter character.</param>
        /// <param name="qualifier">Qualifier character.</param>
        /// <returns>A list whose elements contain the substrings in this instance that are delimited by the delimiter.</returns>
        private static List<string> SplitNest(string source, char delimiter, char qualifier)
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
                    if (character.Equals(delimiter))
                    {
                        result.Add(string.Empty);
                        continue;
                    }

                    if (character.Equals(qualifier))
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
                    if (character.Equals(qualifier) && ((source.Length > (i + 1) && source[i + 1].Equals(delimiter)) || ((i + 1) == source.Length)))
                    {
                        inQuotes = false;
                        inItem = false;
                        i++;
                    }
                    else if (character.Equals(qualifier) && source.Length > (i + 1) && source[i + 1].Equals(qualifier))
                    {
                        i++;
                    }
                }
                else if (character.Equals(delimiter))
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
        /// Method CheckNull.
        /// </summary>
        /// <param name="parameter">Parameter to check.</param>
        private void CheckNull(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                throw new ArgumentNullException("parameter");
            }
        }
    }
}
