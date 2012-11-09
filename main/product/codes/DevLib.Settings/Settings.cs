//-----------------------------------------------------------------------
// <copyright file="Settings.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Settings
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Represents a configuration file that is applicable to a particular application. This class cannot be inherited.
    /// </summary>
    public sealed class Settings
    {
        /// <summary>
        ///
        /// </summary>
        private Configuration _configuration = null;

        /// <summary>
        ///
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="configuration"></param>
        internal Settings(string configFile, Configuration configuration)
        {
            this.ConfigFile = configFile;
            this._configuration = configuration;
        }

        /// <summary>
        /// Gets current configuration file.
        /// </summary>
        public string ConfigFile
        {
            get;
            private set;
        }

        /// <summary>
        /// Writes the configuration settings to the current XML configuration file.
        /// </summary>
        public void Save()
        {
            if (Path.Equals(this._configuration.FilePath, this.ConfigFile))
            {
                this._configuration.Save(ConfigurationSaveMode.Minimal, false);
            }
            else
            {
                this._configuration.SaveAs(this.ConfigFile, ConfigurationSaveMode.Minimal, false);
            }
        }

        /// <summary>
        /// Writes the configuration settings to the specified XML configuration file.
        /// </summary>
        /// <param name="fileName">The path and file name to save the configuration file to.</param>
        public void SaveAs(string fileName)
        {
            this._configuration.SaveAs(fileName, ConfigurationSaveMode.Minimal, false);
        }

        /// <summary>
        /// Sets value for specified key.
        /// </summary>
        /// <param name="key">A string specifying the key.</param>
        /// <param name="value">An object specifying the value.</param>
        public void SetValue(string key, object value)
        {
            string valueString = Serialize(value);

            if (Contains(this._configuration.AppSettings.Settings.AllKeys, key))
            {
                this._configuration.AppSettings.Settings[key].Value = valueString;
            }
            else
            {
                this._configuration.AppSettings.Settings.Add(key, valueString);
            }
        }

        /// <summary>
        /// Gets value of specified key.
        /// </summary>
        /// <param name="key">A string specifying the key.</param>
        /// <param name="defaultValue">If <paramref name="key"/> does not exist, return default value.</param>
        /// <returns>A configuration object, or <paramref name="defaultValue"/> if <paramref name="key"/> does not exist in the collection.</returns>
        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            if (Contains(this._configuration.AppSettings.Settings.AllKeys, key))
            {
                try
                {
                    return Deserialize<T>(this._configuration.AppSettings.Settings[key].Value);
                }
                catch
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Removes a setting by key.
        /// </summary>
        /// <param name="key">A string specifying the key.</param>
        public void Remove(string key)
        {
            this._configuration.AppSettings.Settings.Remove(key);
        }

        /// <summary>
        /// Clears the settings.
        /// </summary>
        public void Clear()
        {
            this._configuration.AppSettings.Settings.Clear();
        }

        /// <summary>
        /// Gets the keys to all setting items.
        /// </summary>
        /// <returns>A string array of keys.</returns>
        public string[] GetAllKeys()
        {
            return this._configuration.AppSettings.Settings.AllKeys;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="array"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool Contains(string[] array, string item)
        {
            return Array.IndexOf(array, item) >= array.GetLowerBound(0);
        }

        /// <summary>
        /// Serializes an object to string.
        /// </summary>
        /// <param name="source">Object to serialize.</param>
        /// <returns>String.</returns>
        private static string Serialize(object source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (source == null)
            {
                return string.Empty;
            }

            string result = null;

            StringBuilder stringBuilder = new StringBuilder();

            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.OmitXmlDeclaration = true;
            xmlWriterSettings.Encoding = new System.Text.UTF8Encoding(false);

            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, xmlWriterSettings))
            {
                XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
                xmlns.Add(String.Empty, String.Empty);

                XmlSerializer xmlSerializer = new XmlSerializer(source.GetType());
                xmlSerializer.Serialize(xmlWriter, source, xmlns);
                result = stringBuilder.ToString();
            }

            return result;
        }

        /// <summary>
        /// Deserializes string to an object.
        /// </summary>
        /// <typeparam name="T">Type of the result objet.</typeparam>
        /// <param name="source">String.</param>
        /// <returns>The result object.</returns>
        private static T Deserialize<T>(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return default(T);
            }

            using (TextReader inputStream = new StringReader(source))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                return (T)xmlSerializer.Deserialize(inputStream);
            }
        }
    }
}
