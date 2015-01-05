//-----------------------------------------------------------------------
// <copyright file="Settings.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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
        /// Field _syncRoot.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Field _settingsItemDictionary.
        /// </summary>
        private readonly Dictionary<string, object> _settingsItemDictionary = new Dictionary<string, object>();

        /// <summary>
        /// Initializes static members of the <see cref="Settings" /> class.
        /// </summary>
        static Settings()
        {
            WriterSettings = new XmlWriterSettings();
            WriterSettings.ConformanceLevel = ConformanceLevel.Auto;
            WriterSettings.OmitXmlDeclaration = true;
            WriterSettings.Indent = true;
            WriterSettings.Encoding = new UTF8Encoding(false);
            WriterSettings.CloseOutput = true;

            ReaderSettings = new XmlReaderSettings();
            ReaderSettings.IgnoreComments = true;
            ReaderSettings.IgnoreProcessingInstructions = true;
            ReaderSettings.IgnoreWhitespace = true;
            ReaderSettings.ConformanceLevel = ConformanceLevel.Auto;
            ReaderSettings.CloseInput = true;

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

            this.Reload();
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
                lock (this._syncRoot)
                {
                    return this._settingsItemDictionary.Count;
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
                lock (this._syncRoot)
                {
                    string[] result = new string[this._settingsItemDictionary.Count];
                    this._settingsItemDictionary.Keys.CopyTo(result, 0);
                    return result;
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
                lock (this._syncRoot)
                {
                    object[] result = new object[this._settingsItemDictionary.Count];
                    this._settingsItemDictionary.Values.CopyTo(result, 0);
                    return result;
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

            this.WriteXmlFile(this.ConfigFile);
        }

        /// <summary>
        /// Writes the configuration settings to the specified XML configuration file. Keep using current Settings instance.
        /// </summary>
        /// <param name="filename">The path and file name to save the configuration file to.</param>
        public void SaveAs(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename", "Didn't specify a configuration file.");
            }

            this.WriteXmlFile(filename);
        }

        /// <summary>
        /// Sets value for specified key.
        /// </summary>
        /// <param name="key">A string specifying the key.</param>
        /// <param name="value">An object specifying the value.</param>
        public void SetValue(string key, object value)
        {
            this.CheckNullKey(key);

            lock (this._syncRoot)
            {
                this._settingsItemDictionary[key] = value;
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

            lock (this._syncRoot)
            {
                if (this._settingsItemDictionary.ContainsKey(key))
                {
                    return this._settingsItemDictionary[key];
                }
                else
                {
                    throw new KeyNotFoundException(string.Format(ConfigurationConstants.KeyNotFoundExceptionStringFormat, key));
                }
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

            lock (this._syncRoot)
            {
                if (this._settingsItemDictionary.ContainsKey(key))
                {
                    return (T)this._settingsItemDictionary[key];
                }
                else
                {
                    throw new KeyNotFoundException(string.Format(ConfigurationConstants.KeyNotFoundExceptionStringFormat, key));
                }
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

            lock (this._syncRoot)
            {
                if (this._settingsItemDictionary.ContainsKey(key))
                {
                    try
                    {
                        return (T)this._settingsItemDictionary[key];
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
        }

        /// <summary>
        /// Removes a setting by key.
        /// </summary>
        /// <param name="key">A string specifying the key.</param>
        public void Remove(string key)
        {
            this.CheckNullKey(key);

            lock (this._syncRoot)
            {
                this._settingsItemDictionary.Remove(key);
            }
        }

        /// <summary>
        /// Clears the settings.
        /// </summary>
        public void Clear()
        {
            lock (this._syncRoot)
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

            lock (this._syncRoot)
            {
                return this._settingsItemDictionary.ContainsKey(key);
            }
        }

        /// <summary>
        /// Reload settings from current XML configuration file. All settings will be clear and load from XML configuration file again.
        /// </summary>
        public void Reload()
        {
            if (!File.Exists(this.ConfigFile))
            {
                return;
            }

            lock (this._syncRoot)
            {
                try
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
                                        InternalLogger.Log(e);
                                    }
                                    finally
                                    {
                                        xmlReader.ReadEndElement();
                                    }
                                }
                                catch (Exception e)
                                {
                                    xmlReader.Skip();

                                    InternalLogger.Log(e);
                                }
                                finally
                                {
                                    xmlReader.ReadEndElement();
                                }
                            }
                            catch (Exception e)
                            {
                                InternalLogger.Log(e);
                            }
                            finally
                            {
                                xmlReader.MoveToContent();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }
            }
        }

        /// <summary>
        /// Refresh settings from current XML configuration file.
        /// </summary>
        public void Refresh()
        {
            if (!File.Exists(this.ConfigFile))
            {
                return;
            }

            lock (this._syncRoot)
            {
                try
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
                                        InternalLogger.Log(e);
                                    }
                                    finally
                                    {
                                        xmlReader.ReadEndElement();
                                    }
                                }
                                catch (Exception e)
                                {
                                    xmlReader.Skip();

                                    InternalLogger.Log(e);
                                }
                                finally
                                {
                                    xmlReader.ReadEndElement();
                                }
                            }
                            catch (Exception e)
                            {
                                InternalLogger.Log(e);
                            }
                            finally
                            {
                                xmlReader.MoveToContent();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }
            }
        }

        /// <summary>
        /// Gets an XML string that represents the Settings.
        /// </summary>
        /// <returns>The XML representation for this Settings.</returns>
        public string GetRawXml()
        {
            lock (this._syncRoot)
            {
                try
                {
                    StringBuilder stringBuilder = new StringBuilder();

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
                                    InternalLogger.Log(e);
                                }
                                finally
                                {
                                    writer.WriteEndElement();
                                }

                                writer.WriteEndElement();
                            }
                            catch (Exception e)
                            {
                                InternalLogger.Log(e);
                            }
                        }

                        writer.WriteEndElement();

                        writer.Flush();

                        return stringBuilder.ToString();
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                    return string.Empty;
                }
            }
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

            lock (this._syncRoot)
            {
                try
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
                                        InternalLogger.Log(e);
                                    }
                                    finally
                                    {
                                        xmlReader.ReadEndElement();
                                    }
                                }
                                catch (Exception e)
                                {
                                    xmlReader.Skip();

                                    InternalLogger.Log(e);
                                }
                                finally
                                {
                                    xmlReader.ReadEndElement();
                                }
                            }
                            catch (Exception e)
                            {
                                InternalLogger.Log(e);
                            }
                            finally
                            {
                                xmlReader.MoveToContent();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }
            }
        }

        /// <summary>
        /// Method WriteXmlFile.
        /// </summary>
        /// <param name="filename"> Xml file name.</param>
        private void WriteXmlFile(string filename)
        {
            lock (this._syncRoot)
            {
                try
                {
                    string fullPath = Path.GetFullPath(filename);

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
                                    InternalLogger.Log(e);
                                }
                                finally
                                {
                                    writer.WriteEndElement();
                                }

                                writer.WriteEndElement();
                            }
                            catch (Exception e)
                            {
                                InternalLogger.Log(e);
                            }
                        }

                        writer.WriteEndElement();

                        writer.Flush();
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
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
