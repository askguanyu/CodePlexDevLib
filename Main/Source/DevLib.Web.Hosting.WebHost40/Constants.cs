//-----------------------------------------------------------------------
// <copyright file="Constants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Hosting.WebHost40
{
    using System.Reflection;

    /// <summary>
    /// Constants class.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// Field HttpErrorFormat1.
        /// </summary>
        public const string HttpErrorFormat1 = "<html>\r\n    <head>\r\n        <title>{0}</title>\r\n";

        /// <summary>
        /// Field HttpStyle.
        /// </summary>
        public const string HttpStyle = "        <style>\r\n        \tbody {font-family:\"Verdana\";font-weight:normal;font-size: 8pt;color:black;} \r\n        \tp {font-family:\"Verdana\";font-weight:normal;color:black;margin-top: -5px}\r\n        \tb {font-family:\"Verdana\";font-weight:bold;color:black;margin-top: -5px}\r\n        \th1 { font-family:\"Verdana\";font-weight:normal;font-size:18pt;color:red }\r\n        \th2 { font-family:\"Verdana\";font-weight:normal;font-size:14pt;color:maroon }\r\n        \tpre {font-family:\"Lucida Console\";font-size: 8pt}\r\n        \t.marker {font-weight: bold; color: black;text-decoration: none;}\r\n        \t.version {color: gray;}\r\n        \t.error {margin-bottom: 10px;}\r\n        \t.expandable { text-decoration:underline; font-weight:bold; color:navy; cursor:hand; }\r\n        </style>\r\n";

        /// <summary>
        /// Field DirListing.
        /// </summary>
        public const string DirListing = "Directory Listing - {0}";

        /// <summary>
        /// Field DirListingFormat1.
        /// </summary>
        public const string DirListingFormat1 = "<html>\r\n    <head>\r\n    <title>{0}</title>\r\n";

        /// <summary>
        /// Field DirListingFormat2.
        /// </summary>
        public const string DirListingFormat2 = "    </head>\r\n    <body bgcolor=\"white\">\r\n\r\n    <h2> <i>{0}</i> </h2></span>\r\n\r\n            <hr width=100% size=1 color=silver>\r\n\r\n<PRE>\r\n";

        /// <summary>
        /// Field DirListingParentFormat.
        /// </summary>
        public const string DirListingParentFormat = "<A href=\"{0}\">[To Parent Directory]</A>\r\n\r\n";

        /// <summary>
        /// Field DirListingFileFormat.
        /// </summary>
        public const string DirListingFileFormat = "{0,38:dddd, MMMM dd, yyyy hh:mm tt} {1,12:n0} <A href=\"{2}\">{2}</A>\r\n";

        /// <summary>
        /// Field DirListingDirFormat.
        /// </summary>
        public const string DirListingDirFormat = "{0,38:dddd, MMMM dd, yyyy hh:mm tt}        &lt;dir&gt; <A href=\"{1}/\">{1}</A>\r\n";

        /// <summary>
        /// Field HTTPError.
        /// </summary>
        public const string HTTPError = "HTTP Error {0} - {1}.";

        /// <summary>
        /// Field ServerError.
        /// </summary>
        public const string ServerError = "Server Error in '{0}' Application.";

        /// <summary>
        /// Field UnhandledException.
        /// </summary>
        public const string UnhandledException = "An unhandled {0} exception occurred while processing the request.";

        /// <summary>
        /// Field VersionInfo.
        /// </summary>
        public const string VersionInfo = "Version Information";

        /// <summary>
        /// Field VWDName.
        /// </summary>
        public const string VWDName = "DevLib.Web.Hosting.WebHost40";

        /// <summary>
        /// Field VersionString.
        /// </summary>
        public static readonly string VersionString = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// Field HttpErrorFormat2.
        /// </summary>
        public static readonly string HttpErrorFormat2 = "    </head>\r\n    <body bgcolor=\"white\">\r\n\r\n            <span><h1>{0}<hr width=100% size=1 color=silver></h1>\r\n\r\n            <h2> <i>{1}</i> </h2></span>\r\n\r\n            <hr width=100% size=1 color=silver>\r\n\r\n            <b>{2}:</b>&nbsp;{3} " + Constants.VersionString + "\r\n\r\n            </font>\r\n\r\n    </body>\r\n</html>\r\n";

        /// <summary>
        /// Field HttpErrorFormat3.
        /// </summary>
        public static readonly string HttpErrorFormat3 = "    </head>\r\n    <body bgcolor=\"white\">\r\n\r\n            <span><h1>{0}<hr width=100% size=1 color=silver></h1>\r\n\r\n            <h2> <i>{1}</i> </h2></span>\r\n\r\n            <table width=100% bgcolor=\"#ffffcc\">\r\n               <tr>\r\n                  <td>\r\n                      <code><pre>\r\n{4}\r\n                      </pre></code>\r\n\r\n                  </td>\r\n               </tr>\r\n            </table>\r\n\r\n            <hr width=100% size=1 color=silver>\r\n\r\n            <b>{2}:</b>&nbsp;{3} " + Constants.VersionString + "\r\n\r\n            </font>\r\n\r\n    </body>\r\n</html>\r\n";

        /// <summary>
        /// Field DirListingTail.
        /// </summary>
        public static readonly string DirListingTail = "</PRE>\r\n            <hr width=100% size=1 color=silver>\r\n\r\n              <b>{0}:</b>&nbsp;{1} " + Constants.VersionString + "\r\n\r\n            </font>\r\n\r\n    </body>\r\n</html>\r\n";
    }
}
