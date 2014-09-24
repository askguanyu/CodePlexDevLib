//-----------------------------------------------------------------------
// <copyright file="MemorySnapshotCollection.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Diagnostics
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// A collection of MemorySnapshots that can be serialized to an XML file.
    /// </summary>
    /// <example>
    /// The following example demonstrates taking multiple memory snapshots of Notepad
    /// and saving them on disk for later analysis.
    /// <code>
    /// MemorySnapshotCollection c = new MemorySnapshotCollection();
    ///
    /// Process p = Process.Start("notepad.exe");
    /// p.WaitForInputIdle(5000);
    /// MemorySnapshot s1 = MemorySnapshot.FromProcess(p.Id);
    /// c.Add(s1);
    ///
    /// // Perform operations that may cause a leak...
    ///
    /// MemorySnapshot s2 = MemorySnapshot.FromProcess(p.Id);
    /// c.Add(s2);
    ///
    /// c.ToFile(@"MemorySnapshots.xml");
    ///
    /// p.CloseMainWindow();
    /// p.Close();
    /// </code>
    /// </example>
    /// <example>
    /// A MemorySnapshotCollection can also be loaded from a XML file.
    /// <code>
    /// MemorySnapshotCollection c = MemorySnapshotCollection.FromFile(@"MemorySnapshots.xml");
    /// </code>
    /// </example>
    public class MemorySnapshotCollection : Collection<MemorySnapshot>
    {
        /// <summary>
        /// Creates a MemorySnapshotCollection instance from data in the specified file.
        /// </summary>
        /// <param name="filename">The path to the MemorySnapshotCollection file.</param>
        /// <returns>A MemorySnapshotCollection instance, containing memory information recorded in the specified file.</returns>
        public static MemorySnapshotCollection FromFile(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("MemorySnapshotCollection.FromFile(): The specified file does not exist.", filename);
            }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(filename);
                XmlNodeList memorySnapshotNodeList = xmlDocument.DocumentElement.SelectNodes("MemorySnapshot");

                MemorySnapshotCollection result = new MemorySnapshotCollection();

                foreach (XmlNode xmlNode in memorySnapshotNodeList)
                {
                    MemorySnapshot memorySnapshot = MemorySnapshot.Deserialize(xmlNode);
                    result.Add(memorySnapshot);
                }

                return result;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Writes the current MemorySnapshotCollection to a file.
        /// </summary>
        /// <param name="filename">The path to the output file.</param>
        public void ToFile(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("MemorySnapshotCollection.ToFile(): the specified file path \"" + filename + "\" is null or empty.");
            }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlNode rootNode = xmlDocument.CreateElement("MemorySnapshotCollection");

                foreach (MemorySnapshot memorySnapshot in this.Items)
                {
                    XmlNode xmlNode = memorySnapshot.Serialize(xmlDocument);
                    rootNode.AppendChild(xmlNode);
                }

                xmlDocument.AppendChild(rootNode);
                xmlDocument.Save(filename);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
        }
    }
}
