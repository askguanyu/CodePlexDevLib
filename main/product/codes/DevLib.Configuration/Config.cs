//-----------------------------------------------------------------------
// <copyright file="Config.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading;

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
        /// Field _settings.
        /// </summary>
        private Settings _settings;

        /// <summary>
        /// Field _readerWriterLock.
        /// </summary>
        private ReaderWriterLock _readerWriterLock = new ReaderWriterLock();

        /// <summary>
        /// Initializes a new instance of the <see cref="Config" /> class.
        /// </summary>
        /// <param name="configFile">Configuration file name.</param>
        internal Config(string configFile)
        {
            this.ConfigFile = configFile;

            this._settings = new Settings();

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
                this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    return this._configuration.AppSettings.Settings.Count + this._settings.Count;
                }
                finally
                {
                    this._readerWriterLock.ReleaseReaderLock();
                }
            }
        }

        /// <summary>
        /// Gets current configuration all keys.
        /// </summary>
        public string[] Keys
        {
            get
            {
                this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    List<string> result = new List<string>();
                    result.AddRange(this._configuration.AppSettings.Settings.AllKeys);
                    result.AddRange(this._settings.Keys);
                    return result.ToArray();
                }
                finally
                {
                    this._readerWriterLock.ReleaseReaderLock();
                }
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
                return this.GetValue(key, false);
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

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                if (this._settings.Count > 0)
                {
                    this._configuration.Sections.Remove("settings");
                    this._configuration.Sections.Add("settings", new DefaultSection());
                    this._configuration.Sections["settings"].SectionInformation.SetRawXml(this._settings.GetRawXml());
                }

                this._configuration.Save(ConfigurationSaveMode.Minimal, false);
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);

                throw;
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
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

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                if (this._settings.Count > 0)
                {
                    this._configuration.Sections.Remove("settings");
                    this._configuration.Sections.Add("settings", new DefaultSection());
                    this._configuration.Sections["settings"].SectionInformation.SetRawXml(this._settings.GetRawXml());
                }

                this._configuration.SaveAs(fileName, ConfigurationSaveMode.Minimal, false);
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);

                throw;
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
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

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                if (this.CanConvertible(value.GetType()))
                {
                    string valueString = Convert.ToString(value);

                    if (this.ArrayContains(this._configuration.AppSettings.Settings.AllKeys, key))
                    {
                        this._configuration.AppSettings.Settings[key].Value = valueString;
                    }
                    else
                    {
                        this._configuration.AppSettings.Settings.Add(key, valueString);
                    }

                    this._settings.Remove(key);
                }
                else
                {
                    this._settings.SetValue(key, value);

                    this._configuration.AppSettings.Settings.Remove(key);
                }
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Gets value of specified key.
        /// </summary>
        /// <param name="key">A string specifying the key.</param>
        /// <param name="refresh">Whether refresh settings before gets value.</param>
        /// <returns>A configuration object.</returns>
        public object GetValue(string key, bool refresh = false)
        {
            this.CheckNullKey(key);

            if (refresh)
            {
                this.Refresh();
            }

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
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

                if (this._settings.Contains(key))
                {
                    try
                    {
                        return this._settings.GetValue(key, false);
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);

                        throw;
                    }
                }

                throw new KeyNotFoundException(string.Format(ConfigurationConstants.KeyNotFoundExceptionStringFormat, key));
            }
            finally
            {
                this._readerWriterLock.ReleaseReaderLock();
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

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (this.ArrayContains(this._configuration.AppSettings.Settings.AllKeys, key))
                {
                    try
                    {
                        return (T)Convert.ChangeType(this._configuration.AppSettings.Settings[key].Value, typeof(T));
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);

                        throw;
                    }
                }

                if (this._settings.Contains(key))
                {
                    try
                    {
                        return this._settings.GetValue<T>(key, false);
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);

                        throw;
                    }
                }

                throw new KeyNotFoundException(string.Format(ConfigurationConstants.KeyNotFoundExceptionStringFormat, key));
            }
            finally
            {
                this._readerWriterLock.ReleaseReaderLock();
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

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (this.ArrayContains(this._configuration.AppSettings.Settings.AllKeys, key))
                {
                    try
                    {
                        return (T)Convert.ChangeType(this._configuration.AppSettings.Settings[key].Value, typeof(T));
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }

                if (this._settings.Contains(key))
                {
                    return this._settings.GetValue<T>(key, defaultValue, false);
                }

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
            finally
            {
                this._readerWriterLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Removes a configuration by key.
        /// </summary>
        /// <param name="key">A string specifying the key.</param>
        public void Remove(string key)
        {
            this.CheckNullKey(key);

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                this._configuration.AppSettings.Settings.Remove(key);

                this._settings.Remove(key);
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Clears the configuration.
        /// </summary>
        public void Clear()
        {
            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                this._configuration.AppSettings.Settings.Clear();
                this._settings.Clear();
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Determines whether configuration contains the specified key.
        /// </summary>
        /// <param name="key">A string specifying the key.</param>
        /// <returns>true if contains an element with the specified key; otherwise, false.</returns>
        public bool Contains(string key)
        {
            this.CheckNullKey(key);

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return this.ArrayContains(this._configuration.AppSettings.Settings.AllKeys, key) || this._settings.Contains(key);
            }
            finally
            {
                this._readerWriterLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Refresh configuration from current XML configuration file.
        /// </summary>
        public void Refresh()
        {
            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                this._configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap() { ExeConfigFilename = this.ConfigFile }, ConfigurationUserLevel.None);

                if (this._configuration.Sections["settings"] != null)
                {
                    this._settings.SetRawXml(this._configuration.Sections["settings"].SectionInformation.GetRawXml());
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
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
