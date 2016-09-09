//-----------------------------------------------------------------------
// <copyright file="WebExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System.IO;
    using System.Net;

    /// <summary>
    /// Web Extensions.
    /// </summary>
    public static class WebExtensions
    {
        /// <summary>
        /// Downloads data from a url.
        /// </summary>
        /// <param name="url">Url to retrieve the data.</param>
        /// <param name="useSystemWebProxy">true to use "IE Proxy" based on the currently impersonated user's proxy settings; false to not use proxy.</param>
        /// <returns>Byte array of data from the url.</returns>
        public static byte[] DownloadData(this string url, bool useSystemWebProxy = false)
        {
            WebRequest webRequest = WebRequest.Create(url);

            if (useSystemWebProxy)
            {
                webRequest.Proxy = WebRequest.GetSystemWebProxy();
            }

            using (WebResponse webResponse = webRequest.GetResponse())
            {
                using (Stream stream = webResponse.GetResponseStream())
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        byte[] buffer = new byte[1024];

                        while (true)
                        {
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);

                            if (bytesRead == 0)
                            {
                                break;
                            }
                            else
                            {
                                memoryStream.Write(buffer, 0, bytesRead);
                            }
                        }

                        memoryStream.Flush();

                        return memoryStream.ToArray();
                    }
                }
            }
        }
    }
}
