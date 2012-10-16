using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DevLib.Settings
{
    public sealed class Settings
    {
        internal Settings(string configFile, Configuration configuration)
        {
            this.ConfigFile = configFile;
            this._configuration = configuration;
        }

        private Configuration _configuration = null;

        /// <summary>
        /// Gets current configuration file
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
                this._configuration.Save(ConfigurationSaveMode.Minimal, true);
            }
            else
            {
                this._configuration.SaveAs(this.ConfigFile, ConfigurationSaveMode.Minimal, true);
            }
        }

        /// <summary>
        /// Writes the configuration settings to the specified XML configuration file.
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveAs(string fileName)
        {
            this._configuration.SaveAs(fileName, ConfigurationSaveMode.Minimal, true);
        }

        /// <summary>
        /// Sets value for specified key
        /// </summary>
        /// <param name="key">A string specifying the key</param>
        /// <param name="value">An object specifying the value</param>
        public void SetValue(string key, object value)
        {
            if (this._configuration.AppSettings.Settings.AllKeys.Contains(key))
            {
                this._configuration.AppSettings.Settings[key].Value = ToJson(value);
            }
            else
            {
                this._configuration.AppSettings.Settings.Add(key, ToJson(value));
            }
        }

        /// <summary>
        /// Gets value of specified key
        /// </summary>
        /// <param name="key">A string specifying the key</param>
        /// <param name="defaultValue">If <paramref name="key"/> does not exist, return default value</param>
        /// <returns>A configuration object, or <paramref name="defaultValue"/> if <paramref name="key"/> does not exist in the collection</returns>
        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            if (this._configuration.AppSettings.Settings.AllKeys.Contains(key))
            {
                return FromJson<T>(this._configuration.AppSettings.Settings[key].Value);
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Removes a setting by key
        /// </summary>
        /// <param name="key">A string specifying the key</param>
        public void Remove(string key)
        {
            this._configuration.AppSettings.Settings.Remove(key);
        }

        /// <summary>
        /// Clears the settings
        /// </summary>
        public void Clear()
        {
            this._configuration.AppSettings.Settings.Clear();
        }

        /// <summary>
        /// Gets the keys to all setting items
        /// </summary>
        /// <returns>A string array of keys</returns>
        public string[] GetAllKeys()
        {
            return this._configuration.AppSettings.Settings.AllKeys;
        }

        /// <summary>
        /// Serializes an object to a JSON string
        /// </summary>
        /// <param name="source">Object to serialize</param>
        /// <returns>JSON string</returns>
        private static string ToJson(object source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (source == null)
            {
                return string.Empty;
            }

            MemoryStream memoryStream = null;
            string result = string.Empty;

            try
            {
                memoryStream = new MemoryStream();
                var serializer = new DataContractJsonSerializer(source.GetType());
                serializer.WriteObject(memoryStream, source);
                result = Encoding.UTF8.GetString(memoryStream.ToArray());
                memoryStream.Flush();
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format(SettingsConstants.ExceptionStringFormat, "SettingsManager.ToJson", e.Source, e.Message, e.StackTrace));
            }
            finally
            {
                if (memoryStream != null)
                {
                    memoryStream.Close();
                    memoryStream = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Serializes a JSON object to an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">JSON string object</param>
        /// <returns>The result object</returns>
        private static T FromJson<T>(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return default(T);
            }

            MemoryStream memoryStream = null;
            T result = default(T);

            try
            {
                memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(source));
                var serializer = new DataContractJsonSerializer(typeof(T));
                result = (T)serializer.ReadObject(memoryStream);
                memoryStream.Flush();
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format(SettingsConstants.ExceptionStringFormat, "SettingsManager.FromJson", e.Source, e.Message, e.StackTrace));
            }
            finally
            {
                if (memoryStream != null)
                {
                    memoryStream.Close();
                    memoryStream = null;
                }
            }

            return result;
        }
    }
}
