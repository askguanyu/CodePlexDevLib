﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace DevLib.ExtensionMethods
{
    public static class WebExtensions
    {
        /// <summary>
        /// Downloads data from a url
        /// </summary>
        /// <param name="url">Url to retrieve the data</param>
        /// <returns>Byte array of data from the url</returns>
        public static byte[] DownloadData(this string url)
        {
            byte[] downloadedData = new byte[0];
            Stream stream = null;
            MemoryStream memoryStream = null;

            try
            {
                WebRequest req = WebRequest.Create(url);
                WebResponse response = req.GetResponse();

                stream = response.GetResponseStream();

                // Download in chunks
                byte[] buffer = new byte[1024];

                int dataLength = (int)response.ContentLength;

                // Download to memory
                memoryStream = new MemoryStream();

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

                downloadedData = memoryStream.ToArray();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }

                if (memoryStream != null)
                {
                    memoryStream.Close();
                }
            }

            return downloadedData;
        }
    }
}