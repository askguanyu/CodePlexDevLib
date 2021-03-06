﻿//-----------------------------------------------------------------------
// <copyright file="IniEntry.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// Represents an ini file that is applicable to a particular application. This class cannot be inherited.
    /// </summary>
    public sealed class IniEntry
    {
        /// <summary>
        /// Field _readerWriterLock.
        /// </summary>
        private ReaderWriterLock _readerWriterLock = new ReaderWriterLock();

        /// <summary>
        /// Initializes a new instance of the <see cref="IniEntry" /> class.
        /// </summary>
        /// <param name="iniFile">Ini file name.</param>
        internal IniEntry(string iniFile)
        {
            this.IniFile = iniFile;

            this.Sections = new Dictionary<string, Dictionary<string, string>>();

            try
            {
                this.Reload();
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IniEntry" /> class.
        /// </summary>
        internal IniEntry()
        {
        }

        /// <summary>
        /// Gets current ini file.
        /// </summary>
        public string IniFile
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets current ini all sections.
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> Sections
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets sections.
        /// </summary>
        /// <param name="section">The section of the ini to get or set.</param>
        /// <returns>The value associated with the specified section.</returns>
        public Dictionary<string, string> this[string section]
        {
            get
            {
                return this.GetSection(section);
            }

            set
            {
                this.SetSection(section, value);
            }
        }

        /// <summary>
        /// Gets or sets ini property.
        /// </summary>
        /// <param name="section">The section of the ini to get or set.</param>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key.</returns>
        public object this[string section, string key]
        {
            get
            {
                return this.GetValue(section, key);
            }

            set
            {
                this.SetValue(section, key, value);
            }
        }

        /// <summary>
        /// Sets value for specified section.
        /// </summary>
        /// <param name="section">A string specifying the section.</param>
        /// <param name="properties">The value to set.</param>
        public void SetSection(string section, IDictionary<string, string> properties = null)
        {
            this.CheckNullValue(section);

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                if (!this.Sections.ContainsKey(section))
                {
                    this.Sections[section] = new Dictionary<string, string>();
                }

                if (properties != null && properties.Count > 0)
                {
                    foreach (var item in properties)
                    {
                        this.Sections[section][item.Key] = item.Value;
                    }
                }
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Gets value of specified section.
        /// </summary>
        /// <param name="section">A string specifying the section.</param>
        /// <returns>An ini section.</returns>
        public Dictionary<string, string> GetSection(string section)
        {
            this.CheckNullValue(section);

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                if (!this.Sections.ContainsKey(section))
                {
                    return null;
                }
                else
                {
                    return new Dictionary<string, string>(this.Sections[section]);
                }
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Removes a section.
        /// </summary>
        /// <param name="section">A string specifying the section.</param>
        public void RemoveSection(string section)
        {
            this.CheckNullValue(section);

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                this.Sections.Remove(section);
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Sets value for specified key.
        /// </summary>
        /// <param name="section">A string specifying the section.</param>
        /// <param name="key">A string specifying the key.</param>
        /// <param name="value">An object specifying the value.</param>
        public void SetValue(string section, string key, object value)
        {
            this.CheckNullValue(section);

            this.CheckNullValue(key);

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                if (!this.Sections.ContainsKey(section))
                {
                    this.Sections[section] = new Dictionary<string, string>();
                }

                this.Sections[section][key] = XmlConverter.ToString(value);
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Gets value of specified key.
        /// </summary>
        /// <param name="section">A string specifying the section.</param>
        /// <param name="key">A string specifying the key.</param>
        /// <param name="refresh">Whether refresh ini before gets value.</param>
        /// <returns>The property string.</returns>
        public string GetValue(string section, string key, bool refresh = false)
        {
            this.CheckNullValue(section);

            this.CheckNullValue(key);

            if (refresh)
            {
                this.Refresh();
            }

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (this.Sections.ContainsKey(section))
                {
                    try
                    {
                        return this.Sections[section][key];
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                        throw;
                    }
                }
                else
                {
                    throw new KeyNotFoundException(string.Format(ConfigurationConstants.KeyNotFoundExceptionStringFormat, section));
                }
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
        /// <param name="section">A string specifying the section.</param>
        /// <param name="key">A string specifying the key.</param>
        /// <param name="refresh">Whether refresh settings before gets value.</param>
        /// <returns>The property object.</returns>
        public T GetValue<T>(string section, string key, bool refresh = false)
        {
            this.CheckNullValue(section);

            this.CheckNullValue(key);

            if (refresh)
            {
                this.Refresh();
            }

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (this.Sections.ContainsKey(section))
                {
                    try
                    {
                        return XmlConverter.ToObject<T>(this.Sections[section][key]);
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                        throw;
                    }
                }
                else
                {
                    throw new KeyNotFoundException(string.Format(ConfigurationConstants.KeyNotFoundExceptionStringFormat, section));
                }
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
        /// <param name="section">A string specifying the section.</param>
        /// <param name="key">A string specifying the key.</param>
        /// <param name="defaultValue">If <paramref name="key"/> does not exist, return default value.</param>
        /// <param name="refresh">Whether refresh settings before gets value.</param>
        /// <returns>The property object, or <paramref name="defaultValue"/> if <paramref name="key"/> does not exist in the collection.</returns>
        public T GetValue<T>(string section, string key, T defaultValue, bool refresh)
        {
            this.CheckNullValue(section);

            this.CheckNullValue(key);

            if (refresh)
            {
                this.Refresh();
            }

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (this.Sections.ContainsKey(section))
                {
                    try
                    {
                        T result = XmlConverter.ToObject<T>(this.Sections[section][key]);
                        return result != null ? result : defaultValue;
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                        return defaultValue;
                    }
                }
                else
                {
                    return defaultValue;
                }
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
        /// Removes a property by key.
        /// </summary>
        /// <param name="section">A string specifying the section.</param>
        /// <param name="key">A string specifying the key.</param>
        public void RemoveKey(string section, string key)
        {
            this.CheckNullValue(section);

            this.CheckNullValue(key);

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                if (this.Sections.ContainsKey(section))
                {
                    this.Sections[section].Remove(key);
                }
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Clears the all ini properties.
        /// </summary>
        public void Clear()
        {
            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                this.Sections.Clear();
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Determines whether ini contains the specified section.
        /// </summary>
        /// <param name="section">A string specifying the section.</param>
        /// <returns>true if contains an element with the specified section; otherwise, false.</returns>
        public bool ContainsSection(string section)
        {
            this.CheckNullValue(section);

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return this.Sections.ContainsKey(section);
            }
            finally
            {
                this._readerWriterLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Determines whether settings contains the specified key.
        /// </summary>
        /// <param name="section">A string specifying the section.</param>
        /// <param name="key">A string specifying the key.</param>
        /// <returns>true if contains an element with the specified key; otherwise, false.</returns>
        public bool ContainsKey(string section, string key)
        {
            this.CheckNullValue(section);

            this.CheckNullValue(key);

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return this.Sections.ContainsKey(section) && this.Sections[section].ContainsKey(key);
            }
            finally
            {
                this._readerWriterLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Find all sections contains the specified key.
        /// </summary>
        /// <param name="key">A string specifying the key.</param>
        /// <returns>A list of sections.</returns>
        public List<string> FindSectionsByKey(string key)
        {
            this.CheckNullValue(key);

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                List<string> result = new List<string>();

                foreach (var section in this.Sections)
                {
                    if (section.Value.ContainsKey(key))
                    {
                        result.Add(section.Key);
                    }
                }

                return result;
            }
            finally
            {
                this._readerWriterLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Reload properties from current ini file. All properties will be clear and load from ini file again.
        /// </summary>
        public void Reload()
        {
            if (!File.Exists(this.IniFile))
            {
                return;
            }

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                this.Sections.Clear();

                this.InternalLoad();
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Refresh properties from current ini file.
        /// </summary>
        public void Refresh()
        {
            if (!File.Exists(this.IniFile))
            {
                return;
            }

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                this.InternalLoad();
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Writes the ini properties to the current ini file.
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrEmpty(this.IniFile))
            {
                throw new ArgumentNullException("IniEntry.IniFile", "Didn't specify an ini file.");
            }

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                this.InternalSave(this.IniFile);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Writes the ini properties to the specified ini file. Keep using current ini instance.
        /// </summary>
        /// <param name="filename">The path and file name to save the ini file to.</param>
        public void SaveAs(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename", "Didn't specify an ini file.");
            }

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                this.InternalSave(filename);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Load ini file.
        /// </summary>
        private void InternalLoad()
        {
            StreamReader streamReader = null;

            try
            {
                streamReader = File.OpenText(this.IniFile);

                string section = null;

                while (streamReader.Peek() >= 0)
                {
                    string line = streamReader.ReadLine().Trim();

                    if (!string.IsNullOrEmpty(line) && !line.StartsWith(";", StringComparison.OrdinalIgnoreCase) && !line.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                    {
                        if (line.StartsWith("[", StringComparison.OrdinalIgnoreCase) && line.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                        {
                            section = line.TrimStart('[').TrimEnd(']');

                            if (!this.Sections.ContainsKey(section))
                            {
                                this.Sections[section] = new Dictionary<string, string>();
                            }
                        }
                        else if (!string.IsNullOrEmpty(section) && !line.StartsWith("=", StringComparison.OrdinalIgnoreCase) && line.Contains("="))
                        {
                            int index = line.IndexOf("=", StringComparison.OrdinalIgnoreCase);

                            string key = line.Substring(0, index).Trim();
                            string value = line.Substring(index + 1).Trim();

                            this.Sections[section][key] = value;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Dispose();
                    streamReader = null;
                }
            }
        }

        /// <summary>
        /// Write ini file.
        /// </summary>
        /// <param name="filename">File to write.</param>
        private void InternalSave(string filename)
        {
            string fullPath = Path.GetFullPath(filename);

            string fullDirectoryPath = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(fullDirectoryPath))
            {
                try
                {
                    Directory.CreateDirectory(fullDirectoryPath);
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                    throw;
                }
            }

            StreamWriter streamWriter = null;

            try
            {
                streamWriter = new StreamWriter(fullPath);

                foreach (var section in this.Sections)
                {
                    streamWriter.WriteLine("[{0}]", section.Key);

                    foreach (var item in section.Value)
                    {
                        streamWriter.WriteLine("{0} = {1}", item.Key, item.Value);
                    }

                    streamWriter.WriteLine();
                }

                streamWriter.Flush();
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Dispose();
                    streamWriter = null;
                }
            }
        }

        /// <summary>
        /// Method CheckNullValue.
        /// </summary>
        /// <param name="value">String to check.</param>
        private void CheckNullValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }
        }
    }
}
