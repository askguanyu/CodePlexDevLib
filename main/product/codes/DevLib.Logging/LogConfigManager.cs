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
        /// Get log config from config file.
        /// </summary>
        /// <param name="logConfigFile">Log config file.</param>
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

                    using (XmlReader xmlReader = XmlReader.Create(new StringReader(loggerSetupNode.OuterXml), new XmlReaderSettings { CheckCharacters = false, IgnoreComments = true, IgnoreWhitespace = true, IgnoreProcessingInstructions = true }))
                    {
                        result.LoggerSetup = (LoggerSetup)xmlSerializer.Deserialize(xmlReader);
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
