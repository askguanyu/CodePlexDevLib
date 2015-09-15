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
        /// Field HttpErrorFormat1.
        /// </summary>
        private const string HttpErrorFormat1 = "<html>\r\n    <head>\r\n        <title>{0}</title>\r\n";

        /// <summary>
        /// Field HttpStyle.
        /// </summary>
        private const string HttpStyle = "        <style>\r\n        \tbody {font-family:\"Verdana\";font-weight:normal;font-size: 8pt;color:black;} \r\n        \tp {font-family:\"Verdana\";font-weight:normal;color:black;margin-top: -5px}\r\n        \tb {font-family:\"Verdana\";font-weight:bold;color:black;margin-top: -5px}\r\n        \th1 { font-family:\"Verdana\";font-weight:normal;font-size:18pt;color:red }\r\n        \th2 { font-family:\"Verdana\";font-weight:normal;font-size:14pt;color:maroon }\r\n        \tpre {font-family:\"Lucida Console\";font-size: 8pt}\r\n        \t.marker {font-weight: bold; color: black;text-decoration: none;}\r\n        \t.version {color: gray;}\r\n        \t.error {margin-bottom: 10px;}\r\n        \t.expandable { text-decoration:underline; font-weight:bold; color:navy; cursor:hand; }\r\n        </style>\r\n";

        /// <summary>
        /// Field DirListingFormat1.
        /// </summary>
        private const string DirListingFormat1 = "<html>\r\n    <head>\r\n    <title>{0}</title>\r\n";

        /// <summary>
        /// Field DirListingFormat2.
        /// </summary>
        private const string DirListingFormat2 = "    </head>\r\n    <body bgcolor=\"white\">\r\n\r\n    <h2> <i>{0}</i> </h2></span>\r\n\r\n            <hr width=100% size=1 color=silver>\r\n\r\n<PRE>\r\n";

        /// <summary>
        /// Field DirListingParentFormat.
        /// </summary>
        private const string DirListingParentFormat = "<A href=\"{0}\">[To Parent Directory]</A>\r\n\r\n";

        /// <summary>
        /// Field DirListingFileFormat.
        /// </summary>
        private const string DirListingFileFormat = "{0,38:dddd, MMMM dd, yyyy hh:mm tt} {1,12:n0} <A href=\"{2}\">{3}</A>\r\n";

        /// <summary>
        /// Field DirListingDirFormat.
        /// </summary>
        private const string DirListingDirFormat = "{0,38:dddd, MMMM dd, yyyy hh:mm tt}        &lt;dir&gt; <A href=\"{1}/\">{2}</A>\r\n";

        /// <summary>
        /// Field HttpErrorFormat2.
        /// </summary>
        private static readonly string HttpErrorFormat2 = "    </head>\r\n    <body bgcolor=\"white\">\r\n\r\n            <span><h1>{0}<hr width=100% size=1 color=silver></h1>\r\n\r\n            <h2> <i>{1}</i> </h2></span>\r\n\r\n            <hr width=100% size=1 color=silver>\r\n\r\n            <b>{2}:</b>&nbsp;{3} " + Constants.VersionString + "\r\n\r\n            </font>\r\n\r\n    </body>\r\n</html>\r\n";

        /// <summary>
        /// Field HttpErrorFormat3.
        /// </summary>
        private static readonly string HttpErrorFormat3 = "    </head>\r\n    <body bgcolor=\"white\">\r\n\r\n            <span><h1>{0}<hr width=100% size=1 color=silver></h1>\r\n\r\n            <h2> <i>{1}</i> </h2></span>\r\n\r\n            <table width=100% bgcolor=\"#ffffcc\">\r\n               <tr>\r\n                  <td>\r\n                      <code><pre>\r\n{4}\r\n                      </pre></code>\r\n\r\n                  </td>\r\n               </tr>\r\n            </table>\r\n\r\n            <hr width=100% size=1 color=silver>\r\n\r\n            <b>{2}:</b>&nbsp;{3} " + Constants.VersionString + "\r\n\r\n            </font>\r\n\r\n    </body>\r\n</html>\r\n";

        /// <summary>
        /// Field DirListingTail.
        /// </summary>
        private static readonly string DirListingTail = "</PRE>\r\n            <hr width=100% size=1 color=silver>\r\n\r\n              <b>{0}:</b>&nbsp;{1} " + Constants.VersionString + "\r\n\r\n            </font>\r\n\r\n    </body>\r\n</html>\r\n";

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

            return string.Format(CultureInfo.InvariantCulture, "<html>\r\n    <head>\r\n        <title>{0}</title>\r\n", statusDescription) +
                "        <style>\r\n        \tbody {font-family:\"Verdana\";font-weight:normal;font-size: 8pt;color:black;} \r\n        \tp {font-family:\"Verdana\";font-weight:normal;color:black;margin-top: -5px}\r\n        \tb {font-family:\"Verdana\";font-weight:bold;color:black;margin-top: -5px}\r\n        \th1 { font-family:\"Verdana\";font-weight:normal;font-size:18pt;color:red }\r\n        \th2 { font-family:\"Verdana\";font-weight:normal;font-size:14pt;color:maroon }\r\n        \tpre {font-family:\"Lucida Console\";font-size: 8pt}\r\n        \t.marker {font-weight: bold; color: black;text-decoration: none;}\r\n        \t.version {color: gray;}\r\n        \t.error {margin-bottom: 10px;}\r\n        \t.expandable { text-decoration:underline; font-weight:bold; color:navy; cursor:hand; }\r\n        </style>\r\n" +
                string.Format(CultureInfo.InvariantCulture, Messages.HttpErrorFormat2, string1, string2, string3, string4);
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

            return string.Format(CultureInfo.InvariantCulture, "<html>\r\n    <head>\r\n        <title>{0}</title>\r\n", messageTitle) +
                "        <style>\r\n        \tbody {font-family:\"Verdana\";font-weight:normal;font-size: 8pt;color:black;} \r\n        \tp {font-family:\"Verdana\";font-weight:normal;color:black;margin-top: -5px}\r\n        \tb {font-family:\"Verdana\";font-weight:bold;color:black;margin-top: -5px}\r\n        \th1 { font-family:\"Verdana\";font-weight:normal;font-size:18pt;color:red }\r\n        \th2 { font-family:\"Verdana\";font-weight:normal;font-size:14pt;color:maroon }\r\n        \tpre {font-family:\"Lucida Console\";font-size: 8pt}\r\n        \t.marker {font-weight: bold; color: black;text-decoration: none;}\r\n        \t.version {color: gray;}\r\n        \t.error {margin-bottom: 10px;}\r\n        \t.expandable { text-decoration:underline; font-weight:bold; color:navy; cursor:hand; }\r\n        </style>\r\n" +
                string.Format(CultureInfo.InvariantCulture, Messages.HttpErrorFormat3, messageTitle, messageHeader, string1, string2, userMessage);
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

            string value = string.Format(CultureInfo.InvariantCulture, Messages.DirListingTail, string2, string3);

            stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "<html>\r\n    <head>\r\n    <title>{0}</title>\r\n", string1));
            stringBuilder.Append("        <style>\r\n        \tbody {font-family:\"Verdana\";font-weight:normal;font-size: 8pt;color:black;} \r\n        \tp {font-family:\"Verdana\";font-weight:normal;color:black;margin-top: -5px}\r\n        \tb {font-family:\"Verdana\";font-weight:bold;color:black;margin-top: -5px}\r\n        \th1 { font-family:\"Verdana\";font-weight:normal;font-size:18pt;color:red }\r\n        \th2 { font-family:\"Verdana\";font-weight:normal;font-size:14pt;color:maroon }\r\n        \tpre {font-family:\"Lucida Console\";font-size: 8pt}\r\n        \t.marker {font-weight: bold; color: black;text-decoration: none;}\r\n        \t.version {color: gray;}\r\n        \t.error {margin-bottom: 10px;}\r\n        \t.expandable { text-decoration:underline; font-weight:bold; color:navy; cursor:hand; }\r\n        </style>\r\n");
            stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "    </head>\r\n    <body bgcolor=\"white\">\r\n\r\n    <h2> <i>{0}</i> </h2></span>\r\n\r\n            <hr width=100% size=1 color=silver>\r\n\r\n<PRE>\r\n", string1));

            if (parentPath != null)
            {
                if (!parentPath.EndsWith("/", StringComparison.Ordinal))
                {
                    parentPath += "/";
                }

                stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "<A href=\"{0}\">[To Parent Directory]</A>\r\n\r\n", parentPath));
            }

            if (elements != null)
            {
                for (int i = 0; i < elements.Length; i++)
                {
                    if (elements[i] is FileInfo)
                    {
                        FileInfo fileInfo = (FileInfo)elements[i];

                        stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0,38:dddd, MMMM dd, yyyy hh:mm tt} {1,12:n0} <A href=\"{2}\">{2}</A>\r\n", fileInfo.LastWriteTime, fileInfo.Length, fileInfo.Name));
                    }
                    else
                    {
                        if (elements[i] is DirectoryInfo)
                        {
                            DirectoryInfo directoryInfo = (DirectoryInfo)elements[i];

                            stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0,38:dddd, MMMM dd, yyyy hh:mm tt}        &lt;dir&gt; <A href=\"{1}/\">{1}</A>\r\n", directoryInfo.LastWriteTime, directoryInfo.Name));
                        }
                    }
                }
            }

            stringBuilder.Append(value);

            return stringBuilder.ToString();
        }
    }
}
