//-----------------------------------------------------------------------
// <copyright file="SoapClient.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// Represents a client for sending SOAP messages using HttpWebRequest.
    /// </summary>
    [Serializable]
    public class SoapClient : MarshalByRefObject
    {
        /// <summary>
        /// Field HttpHeaderContentType.
        /// </summary>
        public const string HttpHeaderContentType = "text/xml;charset=\"utf-8\"";

        /// <summary>
        /// Field HttpHeaderAccept.
        /// </summary>
        public const string HttpHeaderAccept = "text/xml";

        /// <summary>
        /// Field HttpRequestMethod.
        /// </summary>
        public const string HttpRequestMethod = "POST";

        /// <summary>
        /// Initializes a new instance of the <see cref="SoapClient"/> class.
        /// </summary>
        public SoapClient()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SoapClient"/> class.
        /// </summary>
        /// <param name="uri">The URI that identifies the Internet resource.</param>
        public SoapClient(string uri)
        {
            this.Uri = uri;
        }

        /// <summary>
        /// Gets or sets the URI that identifies the Internet resource.
        /// </summary>
        public string Uri
        {
            get;
            set;
        }

        /// <summary>
        /// Sends the SOAP request.
        /// </summary>
        /// <param name="uri">The URI that identifies the Internet resource.</param>
        /// <param name="filename">The SOAP envelope file.</param>
        /// <param name="removeHeaderAction">true to remove Action node in Header; otherwise, false.</param>
        /// <returns>SOAP response.</returns>
        public static string SendRequestFile(string uri, string filename, bool removeHeaderAction = false)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            string fullPath = Path.GetFullPath(filename);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("The specified file does not exist.", fullPath);
            }

            XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.Load(filename);

            return SendRequest(uri, soapEnvelopeXml, removeHeaderAction);
        }

        /// <summary>
        /// Sends the SOAP request.
        /// </summary>
        /// <param name="uri">The URI that identifies the Internet resource.</param>
        /// <param name="soapEnvelope">The SOAP envelope text.</param>
        /// <param name="removeHeaderAction">true to remove Action node in Header; otherwise, false.</param>
        /// <returns>SOAP response.</returns>
        public static string SendRequestText(string uri, string soapEnvelope, bool removeHeaderAction = false)
        {
            if (string.IsNullOrEmpty(soapEnvelope))
            {
                return string.Empty;
            }

            XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(soapEnvelope);

            return SendRequest(uri, soapEnvelopeXml, removeHeaderAction);
        }

        /// <summary>
        /// Sends the SOAP request.
        /// </summary>
        /// <param name="uri">The URI that identifies the Internet resource.</param>
        /// <param name="soapEnvelopeXml">The SOAP envelope XML.</param>
        /// <param name="removeHeaderAction">true to remove Action node in Header; otherwise, false.</param>
        /// <returns>SOAP response.</returns>
        public static string SendRequest(string uri, XmlDocument soapEnvelopeXml, bool removeHeaderAction = false)
        {
            if (removeHeaderAction)
            {
                foreach (XmlNode childNode in soapEnvelopeXml.DocumentElement.ChildNodes)
                {
                    if (childNode.LocalName.Equals("Header"))
                    {
                        foreach (XmlNode item in childNode.ChildNodes)
                        {
                            if (item.LocalName.Equals("Action"))
                            {
                                childNode.RemoveChild(item);
                                break;
                            }
                        }

                        break;
                    }
                }
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

            request.ContentType = HttpHeaderContentType;
            request.Accept = HttpHeaderAccept;
            request.Method = HttpRequestMethod;

            Stream stream = null;

            try
            {
                stream = request.GetRequestStream();
                soapEnvelopeXml.Save(stream);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                    stream = null;
                }
            }

            WebResponse response = null;

            try
            {
                response = request.GetResponse();

                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    string result = streamReader.ReadToEnd();

                    return result;
                }
            }
            catch (Exception e)
            {
                if (e is WebException)
                {
                    WebException webException = e as WebException;

                    StringBuilder stringBuilder = new StringBuilder();

                    stringBuilder.AppendLine(webException.Message);

                    if (webException.Response != null)
                    {
                        StreamReader streamReader = null;

                        try
                        {
                            streamReader = new StreamReader(webException.Response.GetResponseStream());
                            string innerMessage = streamReader.ReadToEnd();
                            stringBuilder.AppendLine(innerMessage);
                        }
                        catch
                        {
                        }
                        finally
                        {
                            if (streamReader != null)
                            {
                                streamReader.Dispose();
                                streamReader = null;
                            }
                        }
                    }

                    throw new WebException(stringBuilder.ToString(), e);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (response != null)
                {
                    (response as IDisposable).Dispose();
                    response = null;
                }
            }
        }

        /// <summary>
        /// Sends the SOAP request.
        /// </summary>
        /// <param name="filename">The SOAP envelope file.</param>
        /// <param name="removeHeaderAction">true to remove Action node in Header; otherwise, false.</param>
        /// <returns>SOAP response.</returns>
        public string SendRequestFile(string filename, bool removeHeaderAction = false)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            string fullPath = Path.GetFullPath(filename);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("The specified file does not exist.", fullPath);
            }

            XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.Load(filename);

            return SendRequest(this.Uri, soapEnvelopeXml, removeHeaderAction);
        }

        /// <summary>
        /// Sends the SOAP request.
        /// </summary>
        /// <param name="soapEnvelope">The SOAP envelope text.</param>
        /// <param name="removeHeaderAction">true to remove Action node in Header; otherwise, false.</param>
        /// <returns>SOAP response.</returns>
        public string SendRequestText(string soapEnvelope, bool removeHeaderAction = false)
        {
            if (string.IsNullOrEmpty(soapEnvelope))
            {
                return string.Empty;
            }

            XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(soapEnvelope);

            return SendRequest(this.Uri, soapEnvelopeXml, removeHeaderAction);
        }

        /// <summary>
        /// Sends the SOAP  request.
        /// </summary>
        /// <param name="soapEnvelopeXml">The SOAP envelope XML.</param>
        /// <param name="removeHeaderAction">true to remove Action node in Header; otherwise, false.</param>
        /// <returns>SOAP response.</returns>
        public string SendRequest(XmlDocument soapEnvelopeXml, bool removeHeaderAction = false)
        {
            return SendRequest(this.Uri, soapEnvelopeXml, removeHeaderAction);
        }
    }
}
