//-----------------------------------------------------------------------
// <copyright file="Settings.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Represents a configuration file that is applicable to a particular application. This class cannot be inherited.
    /// </summary>
    public sealed class Settings
    {
        /// <summary>
        /// Field ReaderSettings.
        /// </summary>
        private static readonly XmlReaderSettings ReaderSettings;

        /// <summary>
        /// Field WriterSettings.
        /// </summary>
        private static readonly XmlWriterSettings WriterSettings;

        /// <summary>
        /// Field EmptyXmlSerializerNamespaces.
        /// </summary>
        private static readonly XmlSerializerNamespaces EmptyXmlSerializerNamespaces;

        /// <summary>
        /// Field _settingsItemDictionary.
        /// </summary>
        private Dictionary<string, object> _settingsItemDictionary = new Dictionary<string, object>();

        /// <summary>
        /// Field _readerWriterLock.
        /// </summary>
        private ReaderWriterLock _readerWriterLock = new ReaderWriterLock();

        /// <summary>
        /// Initializes static members of the <see cref="Settings" /> class.
        /// </summary>
        static Settings()
        {
            WriterSettings = new XmlWriterSettings();
            WriterSettings.ConformanceLevel = ConformanceLevel.Auto;
            WriterSettings.OmitXmlDeclaration = true;
            WriterSettings.Indent = true;

            ReaderSettings = new XmlReaderSettings();
            ReaderSettings.IgnoreComments = true;
            ReaderSettings.IgnoreProcessingInstructions = true;
            ReaderSettings.IgnoreWhitespace = true;
            ReaderSettings.ConformanceLevel = ConformanceLevel.Auto;

            EmptyXmlSerializerNamespaces = new XmlSerializerNamespaces();
            EmptyXmlSerializerNamespaces.Add(string.Empty, string.Empty);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings" /> class.
        /// </summary>
        /// <param name="configFile">Configuration file name.</param>
        internal Settings(string configFile)
        {
            this.ConfigFile = configFile;

            try
            {
                this.Reload();
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings" /> class.
        /// </summary>
        internal Settings()
        {
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
        /// Gets current settings count.
        /// </summary>
        public int Count
        {
            get
            {
                this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    return this._settingsItemDictionary.Count;
                }
                finally
                {
                    this._readerWriterLock.ReleaseReaderLock();
                }
            }
        }

        /// <summary>
        /// Gets current settings all keys.
        /// </summary>
        public string[] Keys
        {
            get
            {
                this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    string[] result = new string[this._settingsItemDictionary.Count];
                    this._settingsItemDictionary.Keys.CopyTo(result, 0);
                    return result;
                }
                finally
                {
                    this._readerWriterLock.ReleaseReaderLock();
                }
            }
        }

        /// <summary>
        /// Gets current settings all values.
        /// </summary>
        public object[] Values
        {
            get
            {
                this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    object[] result = new object[this._settingsItemDictionary.Count];
                    this._settingsItemDictionary.Values.CopyTo(result, 0);
                    return result;
                }
                finally
                {
                    this._readerWriterLock.ReleaseReaderLock();
                }
            }
        }

        /// <summary>
        /// Gets or sets settings value.
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
        /// Writes the configuration settings to the current XML configuration file.
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrEmpty(this.ConfigFile))
            {
                throw new ArgumentNullException("Settings.ConfigFile", "Didn't specify a configuration file.");
            }

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                this.WriteXmlFile(this.ConfigFile);
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
        /// Writes the configuration settings to the specified XML configuration file. Keep using current Settings instance.
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
                this.WriteXmlFile(fileName);
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
                this._settingsItemDictionary[key] = value;
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
                if (this._settingsItemDictionary.ContainsKey(key))
                {
                    try
                    {
                        return this._settingsItemDictionary[key];
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);

                        throw;
                    }
                }
                else
                {
                    throw new KeyNotFoundException(string.Format(ConfigurationConstants.KeyNotFoundExceptionStringFormat, key));
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
                if (this._settingsItemDictionary.ContainsKey(key))
                {
                    try
                    {
                        return (T)this._settingsItemDictionary[key];
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);

                        throw;
                    }
                }
                else
                {
                    throw new KeyNotFoundException(string.Format(ConfigurationConstants.KeyNotFoundExceptionStringFormat, key));
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
                if (this._settingsItemDictionary.ContainsKey(key))
                {
                    try
                    {
                        return (T)this._settingsItemDictionary[key];
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);

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
        /// Removes a setting by key.
        /// </summary>
        /// <param name="key">A string specifying the key.</param>
        public void Remove(string key)
        {
            this.CheckNullKey(key);

            this._readerWriterLock.AcquireWriterLock(Timeout.Infinite);

            try
            {
                this._settingsItemDictionary.Remove(key);
            }
            finally
            {
                this._readerWriterLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Clears the settings.
        /// </summary>
        public void Clear()
        {
            lock (((ICollection)this._settingsItemDictionary).SyncRoot)
            {
                this._settingsItemDictionary.Clear();
            }
        }

        /// <summary>
        /// Determines whether settings contains the specified key.
        /// </summary>
        /// <param name="key">A string specifying the key.</param>
        /// <returns>true if contains an element with the specified key; otherwise, false.</returns>
        public bool Contains(string key)
        {
            this.CheckNullKey(key);

            this._readerWriterLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return this._settingsItemDictionary.ContainsKey(key);
            }
            finally
            {
                this._readerWriterLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Reload settings from current XML configuration file. All settings will be clear and load from XML configuration file again.
        /// </summary>
        public void Reload()
        {
            if (string.IsNullOrEmpty(this.ConfigFile) || !File.Exists(this.ConfigFile))
            {
                return;
            }

            lock (((ICollection)this._settingsItemDictionary).SyncRoot)
            {
                using (XmlReader xmlReader = XmlReader.Create(this.ConfigFile, ReaderSettings))
                {
                    if (xmlReader.IsEmptyElement || !xmlReader.Read())
                    {
                        return;
                    }

                    xmlReader.ReadStartElement("settings");

                    this._settingsItemDictionary.Clear();

                    while (xmlReader.NodeType != (XmlNodeType.None | XmlNodeType.EndElement) && xmlReader.ReadState != (ReadState.Error | ReadState.EndOfFile))
                    {
                        try
                        {
                            string key = xmlReader.GetAttribute("key");

                            object value = null;

                            try
                            {
                                xmlReader.ReadStartElement("item");

                                string valueTypeName = xmlReader.GetAttribute("type");

                                XmlSerializer valueSerializer = new XmlSerializer(Type.GetType(valueTypeName, false, true));

                                try
                                {
                                    xmlReader.ReadStartElement("value");

                                    value = valueSerializer.Deserialize(xmlReader);

                                    if (!string.IsNullOrEmpty(key))
                                    {
                                        this._settingsItemDictionary.Add(key, value);
                                    }
                                }
                                catch (Exception e)
                                {
                                    ExceptionHandler.Log(e);
                                }
                                finally
                                {
                                    xmlReader.ReadEndElement();
                                }
                            }
                            catch (Exception e)
                            {
                                xmlReader.Skip();

                                ExceptionHandler.Log(e);
                            }
                            finally
                            {
                                xmlReader.ReadEndElement();
                            }
                        }
                        catch (Exception e)
                        {
                            ExceptionHandler.Log(e);
                        }
                        finally
                        {
                            xmlReader.MoveToContent();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Refresh settings from current XML configuration file.
        /// </summary>
        public void Refresh()
        {
            if (string.IsNullOrEmpty(this.ConfigFile) || !File.Exists(this.ConfigFile))
            {
                return;
            }

            lock (((ICollection)this._settingsItemDictionary).SyncRoot)
            {
                using (XmlReader xmlReader = XmlReader.Create(this.ConfigFile, ReaderSettings))
                {
                    if (xmlReader.IsEmptyElement || !xmlReader.Read())
                    {
                        return;
                    }

                    xmlReader.ReadStartElement("settings");

                    while (xmlReader.NodeType != (XmlNodeType.None | XmlNodeType.EndElement) && xmlReader.ReadState != (ReadState.Error | ReadState.EndOfFile))
                    {
                        try
                        {
                            string key = xmlReader.GetAttribute("key");

                            object value = null;

                            try
                            {
                                xmlReader.ReadStartElement("item");

                                string valueTypeName = xmlReader.GetAttribute("type");

                                XmlSerializer valueSerializer = new XmlSerializer(Type.GetType(valueTypeName, false, true));

                                try
                                {
                                    xmlReader.ReadStartElement("value");

                                    value = valueSerializer.Deserialize(xmlReader);

                                    if (!string.IsNullOrEmpty(key))
                                    {
                                        this._settingsItemDictionary[key] = value;
                                    }
                                }
                                catch (Exception e)
                                {
                                    ExceptionHandler.Log(e);
                                }
                                finally
                                {
                                    xmlReader.ReadEndElement();
                                }
                            }
                            catch (Exception e)
                            {
                                xmlReader.Skip();

                                ExceptionHandler.Log(e);
                            }
                            finally
                            {
                                xmlReader.ReadEndElement();
                            }
                        }
                        catch (Exception e)
                        {
                            ExceptionHandler.Log(e);
                        }
                        finally
                        {
                            xmlReader.MoveToContent();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets an XML string that represents the Settings.
        /// </summary>
        /// <returns>The XML representation for this Settings.</returns>
        public string GetRawXml()
        {
            StringBuilder stringBuilder = new StringBuilder();

            lock (((ICollection)this._settingsItemDictionary).SyncRoot)
            {
                using (XmlWriter writer = XmlWriter.Create(stringBuilder, WriterSettings))
                {
                    writer.WriteStartElement("settings");

                    foreach (KeyValuePair<string, object> item in this._settingsItemDictionary)
                    {
                        XmlSerializer valueSerializer = null;

                        try
                        {
                            Type valueType = item.Value.GetType();
                            valueSerializer = new XmlSerializer(valueType);

                            writer.WriteStartElement("item");

                            writer.WriteStartAttribute("key");
                            writer.WriteValue(item.Key);
                            writer.WriteEndAttribute();

                            writer.WriteStartElement("value");

                            writer.WriteStartAttribute("type");
                            writer.WriteValue(valueType.AssemblyQualifiedName);
                            writer.WriteEndAttribute();

                            try
                            {
                                valueSerializer.Serialize(writer, item.Value, EmptyXmlSerializerNamespaces);
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Log(e);
                            }
                            finally
                            {
                                writer.WriteEndElement();
                            }

                            writer.WriteEndElement();
                        }
                        catch (Exception e)
                        {
                            ExceptionHandler.Log(e);
                        }
                    }

                    writer.WriteEndElement();
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Sets the Settings with an XML string.
        /// </summary>
        /// <param name="rawXml">The XML to use.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public void SetRawXml(string rawXml)
        {
            if (string.IsNullOrEmpty(rawXml))
            {
                return;
            }

            lock (((ICollection)this._settingsItemDictionary).SyncRoot)
            {
                using (XmlReader xmlReader = XmlReader.Create(new StringReader(rawXml), ReaderSettings))
                {
                    if (xmlReader.IsEmptyElement || !xmlReader.Read())
                    {
                        return;
                    }

                    xmlReader.ReadStartElement("settings");

                    this._settingsItemDictionary.Clear();

                    while (xmlReader.NodeType != (XmlNodeType.None | XmlNodeType.EndElement) && xmlReader.ReadState != (ReadState.Error | ReadState.EndOfFile))
                    {
                        try
                        {
                            string key = xmlReader.GetAttribute("key");
                            object value = null;

                            try
                            {
                                xmlReader.ReadStartElement("item");

                                string valueTypeName = xmlReader.GetAttribute("type");

                                XmlSerializer valueSerializer = new XmlSerializer(Type.GetType(valueTypeName, false, true));

                                try
                                {
                                    xmlReader.ReadStartElement("value");

                                    value = valueSerializer.Deserialize(xmlReader);

                                    if (!string.IsNullOrEmpty(key))
                                    {
                                        this._settingsItemDictionary.Add(key, value);
                                    }
                                }
                                catch (Exception e)
                                {
                                    ExceptionHandler.Log(e);
                                }
                                finally
                                {
                                    xmlReader.ReadEndElement();
                                }
                            }
                            catch (Exception e)
                            {
                                xmlReader.Skip();

                                ExceptionHandler.Log(e);
                            }
                            finally
                            {
                                xmlReader.ReadEndElement();
                            }
                        }
                        catch (Exception e)
                        {
                            ExceptionHandler.Log(e);
                        }
                        finally
                        {
                            xmlReader.MoveToContent();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Method WriteXmlFile.
        /// </summary>
        /// <param name="fileName"> Xml file name.</param>
        private void WriteXmlFile(string fileName)
        {
            lock (((ICollection)this._settingsItemDictionary).SyncRoot)
            {
                string fullPath = Path.GetFullPath(fileName);

                string fullDirectoryPath = Path.GetDirectoryName(fullPath);

                if (!Directory.Exists(fullDirectoryPath))
                {
                    try
                    {
                        Directory.CreateDirectory(fullDirectoryPath);
                    }
                    catch
                    {
                    }
                }

                using (XmlWriter writer = XmlWriter.Create(fullPath, WriterSettings))
                {
                    writer.WriteStartElement("settings");

                    foreach (KeyValuePair<string, object> item in this._settingsItemDictionary)
                    {
                        XmlSerializer valueSerializer = null;

                        try
                        {
                            Type valueType = item.Value.GetType();
                            valueSerializer = new XmlSerializer(valueType);

                            writer.WriteStartElement("item");

                            writer.WriteStartAttribute("key");
                            writer.WriteValue(item.Key);
                            writer.WriteEndAttribute();

                            writer.WriteStartElement("value");

                            writer.WriteStartAttribute("type");
                            writer.WriteValue(valueType.AssemblyQualifiedName);
                            writer.WriteEndAttribute();

                            try
                            {
                                valueSerializer.Serialize(writer, item.Value, EmptyXmlSerializerNamespaces);
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Log(e);
                            }
                            finally
                            {
                                writer.WriteEndElement();
                            }

                            writer.WriteEndElement();
                        }
                        catch (Exception e)
                        {
                            ExceptionHandler.Log(e);
                        }
                    }

                    writer.WriteEndElement();
                }
            }
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
