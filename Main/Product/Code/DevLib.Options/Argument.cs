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
            this._parameters = ArgumentParser.Parse(arguments);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Argument" /> class.
        /// </summary>
        /// <param name="argument">Argument string to parse.</param>
        public Argument(string argument)
        {
            this._parameters = ArgumentParser.Parse(argument);
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
                return (T)ArgumentParser.ConvertTo(value, typeof(T));
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
