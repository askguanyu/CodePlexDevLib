//-----------------------------------------------------------------------
// <copyright file="IOExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// IO Extensions
    /// </summary>
    public static class IOExtensions
    {
        /// <summary>
        /// Creates a new text file from a string, this method will overwrite exists file
        /// </summary>
        /// <param name="text">Text to write to the file</param>
        /// <param name="fileName">Full path to the file</param>
        /// <returns>True if write file successfully</returns>
        public static bool CreateTextFile(this string text, string fileName)
        {
            string fullName = Path.GetFullPath(fileName);
            string fullPath = Path.GetDirectoryName(fullName);

            StreamWriter streamWriter;

            if (!Directory.Exists(fullPath))
            {
                try
                {
                    Directory.CreateDirectory(fullPath);
                }
                catch
                {
                    return false;
                }
            }

            try
            {
                streamWriter = File.CreateText(Path.GetFullPath(fileName));
                streamWriter.Write(text);
                streamWriter.Flush();
                streamWriter.Close();
                streamWriter.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Read text file to string
        /// </summary>
        /// <param name="fileName">Text file to be read</param>
        /// <returns>Text file string</returns>
        public static string ReadTextFile(this string fileName)
        {
            string fullName = Path.GetFullPath(fileName);

            if (File.Exists(Path.GetFullPath(fileName)))
            {
                try
                {
                    return File.ReadAllText(fullName);
                }
                catch
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
