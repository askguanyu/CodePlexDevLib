//-----------------------------------------------------------------------
// <copyright file="FtpFileParser.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.Ftp
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Ftp File Parser Class.
    /// </summary>
    internal class FtpFileParser
    {
        /// <summary>
        /// Const Field UnixSymLinkPathSeparator.
        /// </summary>
        private const string UnixSymLinkPathSeparator = " -> ";

        /// <summary>
        /// Ftp File Style Enum.
        /// </summary>
        private enum FtpFileStyle
        {
            /// <summary>
            /// Represents WindowsStyle.
            /// </summary>
            WindowsStyle,

            /// <summary>
            /// Represents UnixStyle.
            /// </summary>
            UnixStyle,

            /// <summary>
            /// Represents Unknown.
            /// </summary>
            Unknown
        }

        /// <summary>
        /// Get Ftp full directory list.
        /// </summary>
        /// <param name="rawString">Source string.</param>
        /// <returns>List of FtpFileInfo.</returns>
        public static List<FtpFileInfo> GetFullDirectoryList(string rawString)
        {
            List<FtpFileInfo> result = null;

            string[] rawList = rawString.Split('\n');

            if (rawList == null || rawList.Length < 1)
            {
                return null;
            }

            FtpFileStyle ftpFileStyle = GetFtpFileStyle(rawList);

            switch (ftpFileStyle)
            {
                case FtpFileStyle.WindowsStyle:

                    result = new List<FtpFileInfo>();
                    foreach (string item in rawList)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            FtpFileInfo ftpFileInfo = new FtpFileInfo();
                            ftpFileInfo.Name = "..";
                            ftpFileInfo = ParseWindowsStyleFtpFile(item);
                            if (ftpFileInfo != null && ftpFileInfo.Name != "." && ftpFileInfo.Name != "..")
                            {
                                result.Add(ftpFileInfo);
                            }
                        }
                    }

                    break;
                case FtpFileStyle.UnixStyle:

                    result = new List<FtpFileInfo>();
                    foreach (string item in rawList)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            FtpFileInfo ftpFileInfo = new FtpFileInfo();
                            ftpFileInfo.Name = "..";
                            ftpFileInfo = ParseUnixStyleFtpFile(item);
                            if (ftpFileInfo != null && ftpFileInfo.Name != "." && ftpFileInfo.Name != "..")
                            {
                                result.Add(ftpFileInfo);
                            }
                        }
                    }

                    break;
                case FtpFileStyle.Unknown:
                    break;
                default:
                    break;
            }

            return result;
        }

        /// <summary>
        /// Get Ftp directory list.
        /// </summary>
        /// <param name="rawString">Source string.</param>
        /// <returns>List of FtpFileInfo.</returns>
        public static List<FtpFileInfo> GetDirectoryList(string rawString)
        {
            List<FtpFileInfo> result = null;

            string[] rawList = rawString.Split('\n');

            if (rawList == null || rawList.Length < 1)
            {
                return null;
            }

            FtpFileStyle ftpFileStyle = GetFtpFileStyle(rawList);

            switch (ftpFileStyle)
            {
                case FtpFileStyle.WindowsStyle:

                    result = new List<FtpFileInfo>();
                    foreach (string item in rawList)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            FtpFileInfo ftpFileInfo = new FtpFileInfo();
                            ftpFileInfo.Name = "..";
                            ftpFileInfo = ParseWindowsStyleFtpFile(item);
                            if (ftpFileInfo != null && ftpFileInfo.Name != "." && ftpFileInfo.Name != ".." && ftpFileInfo.IsDirectory)
                            {
                                result.Add(ftpFileInfo);
                            }
                        }
                    }

                    break;
                case FtpFileStyle.UnixStyle:

                    result = new List<FtpFileInfo>();
                    foreach (string item in rawList)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            FtpFileInfo ftpFileInfo = new FtpFileInfo();
                            ftpFileInfo.Name = "..";
                            ftpFileInfo = ParseUnixStyleFtpFile(item);
                            if (ftpFileInfo != null && ftpFileInfo.Name != "." && ftpFileInfo.Name != ".." && ftpFileInfo.IsDirectory)
                            {
                                result.Add(ftpFileInfo);
                            }
                        }
                    }

                    break;
                case FtpFileStyle.Unknown:
                    break;
                default:
                    break;
            }

            return result;
        }

        /// <summary>
        /// Get Ftp file list.
        /// </summary>
        /// <param name="rawString">Source string.</param>
        /// <returns>List of FtpFileInfo.</returns>
        public static List<FtpFileInfo> GetFileList(string rawString)
        {
            List<FtpFileInfo> result = null;

            string[] rawList = rawString.Split('\n');

            if (rawList == null || rawList.Length < 1)
            {
                return null;
            }

            FtpFileStyle ftpFileStyle = GetFtpFileStyle(rawList);

            switch (ftpFileStyle)
            {
                case FtpFileStyle.WindowsStyle:

                    result = new List<FtpFileInfo>();
                    foreach (string item in rawList)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            FtpFileInfo ftpFileInfo = new FtpFileInfo();
                            ftpFileInfo.Name = "..";
                            ftpFileInfo = ParseWindowsStyleFtpFile(item);
                            if (ftpFileInfo != null && ftpFileInfo.Name != "." && ftpFileInfo.Name != ".." && !ftpFileInfo.IsDirectory)
                            {
                                result.Add(ftpFileInfo);
                            }
                        }
                    }

                    break;
                case FtpFileStyle.UnixStyle:

                    result = new List<FtpFileInfo>();
                    foreach (string item in rawList)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            FtpFileInfo ftpFileInfo = new FtpFileInfo();
                            ftpFileInfo.Name = "..";
                            ftpFileInfo = ParseUnixStyleFtpFile(item);
                            if (ftpFileInfo != null && ftpFileInfo.Name != "." && ftpFileInfo.Name != ".." && !ftpFileInfo.IsDirectory)
                            {
                                result.Add(ftpFileInfo);
                            }
                        }
                    }

                    break;
                case FtpFileStyle.Unknown:
                    break;
                default:
                    break;
            }

            return result;
        }

        /// <summary>
        /// Method ParseWindowsStyleFtpFile.
        /// </summary>
        /// <param name="rawString">Source string.</param>
        /// <returns>Instance of FtpFileInfo.</returns>
        private static FtpFileInfo ParseWindowsStyleFtpFile(string rawString)
        {
            //// 12-20-13  04:56PM       <DIR>          FolderName

            FtpFileInfo result = new FtpFileInfo();
            string inputString = rawString.Trim();
            string dateString = inputString.Substring(0, 8);
            inputString = inputString.Substring(8, inputString.Length - 8).Trim();
            string timeString = inputString.Substring(0, 7);
            inputString = inputString.Substring(7, inputString.Length - 7).Trim();

            DateTime fileTime = DateTime.Now;
            DateTime.TryParse(string.Format("{0} {1}", dateString, timeString), CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.AllowWhiteSpaces, out fileTime);
            result.LastModifiedTime = fileTime;

            if (inputString.Substring(0, 5) == "<DIR>")
            {
                result.IsDirectory = true;
                result.Size = -1;

                inputString = inputString.Substring(5, inputString.Length - 5).Trim();
            }
            else
            {
                result.IsDirectory = false;

                int index = inputString.IndexOf(' ');

                long fileSize = 0;
                long.TryParse(inputString.Substring(0, index), out fileSize);
                result.Size = fileSize;

                inputString = inputString.Substring(index + 1);
            }

            result.Name = inputString;

            return result;
        }

        /// <summary>
        /// Method ParseUnixStyleFtpFile.
        /// </summary>
        /// <param name="rawString">Source string.</param>
        /// <returns>Instance of FtpFileInfo.</returns>
        private static FtpFileInfo ParseUnixStyleFtpFile(string rawString)
        {
            //// dr-xr-xr-x   1 owner    group               0 Dec 20  2012 FolderName
            //// Mac OS X - tnftpd returns the total on the first line

            if (rawString.StartsWith("total ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            FtpFileInfo result = new FtpFileInfo();
            string inputString = rawString.Trim();
            result.Flags = inputString.Substring(0, 9);
            result.IsDirectory = result.Flags[0] == 'd';
            result.IsSymLink = result.Flags[0] == 'l';
            inputString = inputString.Substring(11).Trim();
            CutSubstringFromStringWithTrim(ref inputString, " ", 0);
            result.Owner = CutSubstringFromStringWithTrim(ref inputString, " ", 0);
            result.Group = CutSubstringFromStringWithTrim(ref inputString, " ", 0);
            result.Size = long.Parse(CutSubstringFromStringWithTrim(ref inputString, " ", 0));

            string lastModifiedTimeString = CutSubstringFromStringWithTrim(ref inputString, " ", 8);

            string dateFormat;
            if (lastModifiedTimeString.IndexOf(':') < 0)
            {
                dateFormat = "MMM dd yyyy";
            }
            else
            {
                dateFormat = "MMM dd H:mm";
            }

            if (lastModifiedTimeString[4] == ' ')
            {
                lastModifiedTimeString = lastModifiedTimeString.Substring(0, 4) + "0" + lastModifiedTimeString.Substring(5);
            }

            DateTime fileTime = DateTime.Now;
            DateTime.TryParseExact(lastModifiedTimeString, dateFormat, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.AllowWhiteSpaces, out fileTime);
            result.LastModifiedTime = fileTime;

            if (result.IsSymLink && inputString.IndexOf(UnixSymLinkPathSeparator) > 0)
            {
                result.Name = CutSubstringFromStringWithTrim(ref inputString, UnixSymLinkPathSeparator, 0);
                result.SymLinkTargetPath = inputString;
            }
            else
            {
                result.Name = inputString;
            }

            return result;
        }

        /// <summary>
        /// Method GetFtpFileStyle.
        /// </summary>
        /// <param name="rawList">Source string array.</param>
        /// <returns>Instance of FtpFileStyle.</returns>
        private static FtpFileStyle GetFtpFileStyle(string[] rawList)
        {
            if (rawList == null || rawList.Length < 1)
            {
                return FtpFileStyle.Unknown;
            }

            foreach (string item in rawList)
            {
                if (item.Length > 8 && Regex.IsMatch(item.Substring(0, 8), "[0-9][0-9]-[0-9][0-9]-[0-9][0-9]"))
                {
                    return FtpFileStyle.WindowsStyle;
                }
                else if (item.Length > 10 && Regex.IsMatch(item.Substring(0, 10), "(-|d)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)"))
                {
                    return FtpFileStyle.UnixStyle;
                }
            }

            return FtpFileStyle.Unknown;
        }

        /// <summary>
        /// Method CutSubstringFromStringWithTrim.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <returns>A substring from source string.</returns>
        private static string CutSubstringFromStringWithTrim(ref string source, string value, int startIndex)
        {
            int index = source.IndexOf(value, startIndex);
            string result = source.Substring(0, index);
            source = source.Substring(index + value.Length).Trim();
            return result;
        }
    }
}
