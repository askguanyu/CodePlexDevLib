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
        /// <returns>Instance of LoggerSetup.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static LoggerSetup GetLoggerSetup(string configFile)
        {
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
                    ExceptionHandler.Log(e);
                }
            }

            return result;
        }

        /// <summary>
        /// Get log config from configuration file.
        /// </summary>
        /// <param name="configFile">Configuration file which contains LogConfig info.</param>
        /// <returns>Instance of LogConfig.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        internal static LogConfig GetLogConfig(string configFile)
        {
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
                        return result;
                    }

                    XmlNode logFileNode = logConfigNode.SelectSingleNode("descendant::LogFile");

                    if (logFileNode != null)
                    {
                        result.LogFile = logFileNode.InnerText;
                    }

                    XmlNode loggerSetupNode = logConfigNode.SelectSingleNode("descendant::LoggerSetup");

                    if (loggerSetupNode != null)
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(LoggerSetup));

                        using (StringReader stringReader = new StringReader(loggerSetupNode.OuterXml))
                        {
                            result.LoggerSetup = (LoggerSetup)xmlSerializer.Deserialize(stringReader);
                        }
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                }
            }

            return result;
        }
    }
}
