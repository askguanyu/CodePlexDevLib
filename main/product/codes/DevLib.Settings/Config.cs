//-----------------------------------------------------------------------
// <copyright file="Config.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Represents a configuration file that is applicable to a particular application. This class cannot be inherited.
    /// </summary>
    public sealed class Config
    {
        /// <summary>
        /// Field _configuration.
        /// </summary>
        private Configuration _configuration;

        /// <summary>
        /// Field _xmlWriterSettings.
        /// </summary>
        private XmlWriterSettings _xmlWriterSettings;

        /// <summary>
        /// Field _xmlNamespaces.
        /// </summary>
        private XmlSerializerNamespaces _xmlNamespaces;

        /// <summary>
        /// Initializes a new instance of the <see cref="Config" /> class.
        /// </summary>
        /// <param name="configFile">Configuration file name.</param>
        internal Config(string configFile)
        {
            this.ConfigFile = configFile;

            this.Init();

            try
            {
                this.Refresh();
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
            }
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
        /// Gets current configuration count.
        /// </summary>
        public int Count
        {
            get
            {
                return this._configuration.AppSettings.Settings.Count;
            }
        }

        /// <summary>
        /// Gets current configuration all keys.
        /// </summary>
        public string[] Keys
        {
            get
            {
                return this._configuration.AppSettings.Settings.AllKeys;
            }
        }

        /// <summary>
        /// Gets or sets configuration value.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key.</returns>
        public object this[string key]
        {
            get
            {
                return this.GetValue(key, true);
            }

            set
            {
                this.SetValue(key, value);
            }
        }

        /// <summary>
        /// Writes the configuration to the current XML configuration file.
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrEmpty(this.ConfigFile))
            {
                throw new ArgumentNullException("Config.ConfigFile", "Didn't specify a configuration file.");
            }

            try
            {
                this._configuration.Save(ConfigurationSaveMode.Minimal, false);
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);

                throw;
            }
        }

        /// <summary>
        /// Writes the configuration to the specified XML configuration file. Keep using current configuration instance.
        /// </summary>
        /// <param name="fileName">The path and file name to save the configuration file to.</param>
        public void SaveAs(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName", "Didn't specify a configuration file.");
            }

            try
            {
                this._configuration.SaveAs(fileName, ConfigurationSaveMode.Minimal, false);
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);

                throw;
            }
        }

        /// <summary>
        /// Sets value for specified key.
        /// </summary>
        /// <param name="key">A string specifying the key.</param>
        /// <param name="value">An object specifying the value.</param>
        public void SetValue(string key, object value)
        {
            this.CheckNullKey(key);

            string valueString = this.Serialize(value);

            if (this.ArrayContains(this._configuration.AppSettings.Settings.AllKeys, key))
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
        /// <param name="refresh">Whether refresh settings before gets value.</param>
        /// <returns>A configuration object.</returns>
        public string GetValue(string key, bool refresh = false)
        {
            this.CheckNullKey(key);

            if (refresh)
            {
                this.Refresh();
            }

            if (this.ArrayContains(this._configuration.AppSettings.Settings.AllKeys, key))
            {
                try
                {
                    return this._configuration.AppSettings.Settings[key].Value;
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);

                    throw;
                }
            }
            else
            {
                throw new KeyNotFoundException(string.Format(SettingsConstants.KeyNotFoundExceptionStringFormat, key));
            }
        }

        /// <summary>
        /// Gets value of specified key.
        /// </summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <param name="key">A string specifying the key.</param>
        /// <param name="refresh">Whether refresh settings before gets value.</param>
        /// <returns>A configuration object.</returns>
        public T GetValue<T>(string key, bool refresh = false)
        {
            this.CheckNullKey(key);

            if (refresh)
            {
                this.Refresh();
            }

            if (this.ArrayContains(this._configuration.AppSettings.Settings.AllKeys, key))
            {
                try
                {
                    return this.Deserialize<T>(this._configuration.AppSettings.Settings[key].Value);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);

                    throw;
                }
            }
            else
            {
                throw new KeyNotFoundException(string.Format(SettingsConstants.KeyNotFoundExceptionStringFormat, key));
            }
        }

        /// <summary>
        /// Gets value of specified key.
        /// </summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <param name="key">A string specifying the key.</param>
        /// <param name="defaultValue">If <paramref name="key"/> does not exist, return default value.</param>
        /// <param name="refresh">Whether refresh settings before gets value.</param>
        /// <returns>A configuration object, or <paramref name="defaultValue"/> if <paramref name="key"/> does not exist in the collection.</returns>
        public T GetValue<T>(string key, T defaultValue, bool refresh)
        {
            this.CheckNullKey(key);

            if (refresh)
            {
                this.Refresh();
            }

            if (this.ArrayContains(this._configuration.AppSettings.Settings.AllKeys, key))
            {
                try
                {
                    return this.Deserialize<T>(this._configuration.AppSettings.Settings[key].Value);
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
        /// Removes a configuration by key.
        /// </summary>
        /// <param name="key">A string specifying the key.</param>
        public void Remove(string key)
        {
            this.CheckNullKey(key);

            this._configuration.AppSettings.Settings.Remove(key);
        }

        /// <summary>
        /// Clears the configuration.
        /// </summary>
        public void Clear()
        {
            this._configuration.AppSettings.Settings.Clear();
        }

        /// <summary>
        /// Determines whether configuration contains the specified key.
        /// </summary>
        /// <param name="key">A string specifying the key.</param>
        /// <returns>true if contains an element with the specified key; otherwise, false.</returns>
        public bool Contains(string key)
        {
            this.CheckNullKey(key);

            return this.ArrayContains(this._configuration.AppSettings.Settings.AllKeys, key);
        }

        /// <summary>
        /// Refresh configuration from current XML configuration file.
        /// </summary>
        public void Refresh()
        {
            if (string.IsNullOrEmpty(this.ConfigFile))
            {
                this.ConfigFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            }

            this._configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap() { ExeConfigFilename = this.ConfigFile }, ConfigurationUserLevel.None);
        }

        /// <summary>
        /// Method ArrayContains.
        /// </summary>
        /// <param name="array">Source array.</param>
        /// <param name="item">Target item.</param>
        /// <returns>true if the source sequence contains an element that has the specified value; otherwise, false.</returns>
        private bool ArrayContains(string[] array, string item)
        {
            return Array.IndexOf(array, item) >= array.GetLowerBound(0);
        }

        /// <summary>
        /// Method CanConvertible.
        /// </summary>
        /// <param name="source">Source Type.</param>
        /// <returns>true if the source Type inherit IConvertible interface; otherwise, false.</returns>
        private bool CanConvertible(Type source)
        {
            return source.GetInterface("IConvertible") != null;
        }

        /// <summary>
        /// Serializes an object to string.
        /// </summary>
        /// <param name="source">Object to serialize.</param>
        /// <returns>Serialized string.</returns>
        private string Serialize(object source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (source == null)
            {
                return string.Empty;
            }

            string result = null;

            Type sourceType = source.GetType();

            if (this.CanConvertible(sourceType))
            {
                result = Convert.ToString(source);
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();

                using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, this._xmlWriterSettings))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(sourceType);
                    xmlSerializer.Serialize(xmlWriter, source, this._xmlNamespaces);
                    result = stringBuilder.ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// Deserializes string to an object.
        /// </summary>
        /// <typeparam name="T">Type of the result objet.</typeparam>
        /// <param name="source">Source string.</param>
        /// <returns>The result object.</returns>
        private T Deserialize<T>(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return default(T);
            }

            T result = default(T);

            if (this.CanConvertible(typeof(T)))
            {
                result = (T)Convert.ChangeType(source, typeof(T));
            }
            else
            {
                using (TextReader textReader = new StringReader(source))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                    return (T)xmlSerializer.Deserialize(textReader);
                }
            }

            return result;
        }

        /// <summary>
        /// Method Init.
        /// </summary>
        private void Init()
        {
            this._xmlWriterSettings = new XmlWriterSettings();
            this._xmlWriterSettings.ConformanceLevel = ConformanceLevel.Auto;
            this._xmlWriterSettings.OmitXmlDeclaration = true;
            this._xmlWriterSettings.Indent = true;

            this._xmlNamespaces = new XmlSerializerNamespaces();
            this._xmlNamespaces.Add(string.Empty, string.Empty);
        }

        /// <summary>
        /// Method CheckNullKey.
        /// </summary>
        /// <param name="key">Key to check.</param>
        private void CheckNullKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
        }
    }
}
