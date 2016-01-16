//-----------------------------------------------------------------------
// <copyright file="LogConfigManager.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Logging
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Class LogConfigManager.
    /// </summary>
    public static class LogConfigManager
    {
        /// <summary>
        /// Get logger setup from configuration file.
        /// </summary>
        /// <param name="configFile">Configuration file which contains LoggerSetup info.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>Instance of LoggerSetup.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static LoggerSetup GetLoggerSetup(string configFile, bool throwOnError = false)
        {
            if (string.IsNullOrEmpty(configFile) && throwOnError)
            {
                throw new ArgumentNullException("configFile", "The specified file name is null or empty.");
            }

            LoggerSetup result = new LoggerSetup();

            if (File.Exists(configFile))
            {
                try
                {
                    XmlDocument xmlDocument = new XmlDocument();

                    xmlDocument.Load(configFile);

                    XmlNode loggerSetupNode = xmlDocument.SelectSingleNode("descendant-or-self::LoggerSetup");

                    if (loggerSetupNode != null)
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(LoggerSetup));

                        using (StringReader stringReader = new StringReader(loggerSetupNode.OuterXml))
                        {
                            result = (LoggerSetup)xmlSerializer.Deserialize(stringReader);
                        }
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);

                    if (throwOnError)
                    {
                        throw;
                    }
                }
            }
            else
            {
                if (throwOnError)
                {
                    throw new FileNotFoundException("The specified file does not exist.", configFile);
                }
            }

            return result;
        }

        /// <summary>
        /// Get log config from configuration file.
        /// </summary>
        /// <param name="configFile">Configuration file which contains LogConfig info.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>Instance of LogConfig.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static LogConfig GetLogConfig(string configFile, bool throwOnError = false)
        {
            if (string.IsNullOrEmpty(configFile) && throwOnError)
            {
                throw new ArgumentNullException("configFile", "The specified file name is null or empty.");
            }

            LogConfig result = new LogConfig();

            if (File.Exists(configFile))
            {
                try
                {
                    XmlDocument xmlDocument = new XmlDocument();

                    xmlDocument.Load(configFile);

                    XmlNode logConfigNode = xmlDocument.SelectSingleNode("descendant-or-self::LogConfig");

                    if (logConfigNode == null)
                    {
                        XmlNode loggerSetupNode = xmlDocument.SelectSingleNode("descendant-or-self::LoggerSetup");

                        if (loggerSetupNode != null)
                        {
                            XmlSerializer xmlSerializer = new XmlSerializer(typeof(LoggerSetup));

                            using (StringReader stringReader = new StringReader(loggerSetupNode.OuterXml))
                            {
                                result.LoggerSetup = (LoggerSetup)xmlSerializer.Deserialize(stringReader);
                            }
                        }

                        return result;
                    }

                    XmlNode logFileNode = logConfigNode.SelectSingleNode("descendant::LogFile");

                    if (logFileNode != null)
                    {
                        result.LogFile = logFileNode.InnerText;
                    }

                    XmlNode logConfigLoggerSetupNode = logConfigNode.SelectSingleNode("descendant::LoggerSetup");

                    if (logConfigLoggerSetupNode != null)
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(LoggerSetup));

                        using (StringReader stringReader = new StringReader(logConfigLoggerSetupNode.OuterXml))
                        {
                            result.LoggerSetup = (LoggerSetup)xmlSerializer.Deserialize(stringReader);
                        }
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);

                    if (throwOnError)
                    {
                        throw;
                    }
                }
            }
            else
            {
                if (throwOnError)
                {
                    throw new FileNotFoundException("The specified file does not exist.", configFile);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the file full path.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>A string containing the fully qualified location of path.</returns>
        public static string GetFileFullPath(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return filename;
            }

            if (!filename.Contains("%") && !filename.Contains("$"))
            {
                return Path.GetFullPath(filename);
            }

            string[] parts = filename.Split(Path.DirectorySeparatorChar);

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].StartsWith("%") || parts[i].StartsWith("$"))
                {
                    parts[i] = Environment.GetEnvironmentVariable(parts[i].Trim('%', '$')) ?? string.Empty;
                }
            }

            return Path.GetFullPath(string.Join(Path.DirectorySeparatorChar.ToString(), parts));
        }
    }
}
