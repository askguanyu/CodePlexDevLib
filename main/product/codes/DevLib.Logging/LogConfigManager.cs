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
    internal static class LogConfigManager
    {
        /// <summary>
        /// Get log config from configuration file.
        /// </summary>
        /// <param name="logConfigFile">Log configuration file.</param>
        /// <returns>Instance of LogConfig.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static LogConfig GetConfig(string logConfigFile)
        {
            LogConfig result = new LogConfig();

            try
            {
                XmlDocument xmlDocument = new XmlDocument();

                xmlDocument.Load(logConfigFile);

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

            return result;
        }
    }
}
