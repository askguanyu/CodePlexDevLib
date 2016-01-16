//-----------------------------------------------------------------------
// <copyright file="Messages.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Hosting.WebHost20
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web;

    /// <summary>
    /// Messages class.
    /// </summary>
    internal class Messages
    {
        /// <summary>
        /// Formats the error message body.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="appName">Name of the application.</param>
        /// <returns>The message.</returns>
        public static string FormatErrorMessageBody(int statusCode, string appName)
        {
            string statusDescription = HttpWorkerRequest.GetStatusDescription(statusCode);
            string string1 = string.Format(Constants.ServerError, appName);
            string string2 = string.Format(Constants.HTTPError, statusCode, statusDescription);
            string string3 = Constants.VersionInfo;
            string string4 = Constants.VWDName;

            return string.Format(CultureInfo.InvariantCulture, Constants.HttpErrorFormat1, statusDescription)
                + Constants.HttpStyle
                + string.Format(CultureInfo.InvariantCulture, Constants.HttpErrorFormat2, string1, string2, string3, string4);
        }

        /// <summary>
        /// Formats the exception message body.
        /// </summary>
        /// <param name="messageTitle">The message title.</param>
        /// <param name="messageHeader">The message header.</param>
        /// <param name="userMessage">The user message.</param>
        /// <returns>The message.</returns>
        public static string FormatExceptionMessageBody(string messageTitle, string messageHeader, string userMessage)
        {
            string string1 = Constants.VersionInfo;
            string string2 = Constants.VWDName;

            return string.Format(CultureInfo.InvariantCulture, Constants.HttpErrorFormat1, messageTitle)
                + Constants.HttpStyle
                + string.Format(CultureInfo.InvariantCulture, Constants.HttpErrorFormat3, messageTitle, messageHeader, string1, string2, userMessage);
        }

        /// <summary>
        /// Formats the directory listing.
        /// </summary>
        /// <param name="dirPath">The dir path.</param>
        /// <param name="parentPath">The parent path.</param>
        /// <param name="elements">The elements.</param>
        /// <returns>The text.</returns>
        public static string FormatDirectoryListing(string dirPath, string parentPath, FileSystemInfo[] elements)
        {
            StringBuilder stringBuilder = new StringBuilder();

            string string1 = string.Format(Constants.DirListing, dirPath);
            string string2 = Constants.VersionInfo;
            string string3 = Constants.VWDName;

            string value = string.Format(CultureInfo.InvariantCulture, Constants.DirListingTail, string2, string3);

            stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, Constants.DirListingFormat1, string1));
            stringBuilder.Append(Constants.HttpStyle);
            stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, Constants.DirListingFormat2, string1));

            if (parentPath != null)
            {
                if (!parentPath.EndsWith("/", StringComparison.Ordinal))
                {
                    parentPath += "/";
                }

                stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, Constants.DirListingParentFormat, parentPath));
            }

            if (elements != null)
            {
                for (int i = 0; i < elements.Length; i++)
                {
                    if (elements[i] is FileInfo)
                    {
                        FileInfo fileInfo = (FileInfo)elements[i];

                        stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, Constants.DirListingFileFormat, fileInfo.LastWriteTime, fileInfo.Length, fileInfo.Name));
                    }
                    else
                    {
                        if (elements[i] is DirectoryInfo)
                        {
                            DirectoryInfo directoryInfo = (DirectoryInfo)elements[i];

                            stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, Constants.DirListingDirFormat, directoryInfo.LastWriteTime, directoryInfo.Name));
                        }
                    }
                }
            }

            stringBuilder.Append(value);

            return stringBuilder.ToString();
        }
    }
}
