//-----------------------------------------------------------------------
// <copyright file="WebResponseMessage.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web
{
    using System;
    using System.Net;

    /// <summary>
    /// Represents a HTTP response message including the status code and data.
    /// </summary>
    [Serializable]
    public class WebResponseMessage : MarshalByRefObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebResponseMessage" /> class.
        /// </summary>
        public WebResponseMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebResponseMessage" /> class.
        /// </summary>
        /// <param name="webResponse">The web response.</param>
        public WebResponseMessage(WebResponse webResponse)
        {
            HttpWebResponse httpWebResponse = webResponse as HttpWebResponse;

            if (httpWebResponse != null)
            {
                this.CharacterSet = httpWebResponse.CharacterSet;
                this.ContentEncoding = httpWebResponse.ContentEncoding;
                this.ContentLength = httpWebResponse.ContentLength;
                this.ContentType = httpWebResponse.ContentType;
                this.Cookies = httpWebResponse.Cookies;
                this.Headers = httpWebResponse.Headers;
                this.IsFromCache = httpWebResponse.IsFromCache;
                this.IsMutuallyAuthenticated = httpWebResponse.IsMutuallyAuthenticated;
                this.LastModified = httpWebResponse.LastModified;
                this.Method = httpWebResponse.Method;
                this.ProtocolVersion = httpWebResponse.ProtocolVersion;
                this.ResponseUri = httpWebResponse.ResponseUri;
                this.Server = httpWebResponse.Server;
                this.StatusCode = httpWebResponse.StatusCode;
                this.StatusDescription = httpWebResponse.StatusDescription;
            }
            else
            {
                this.ContentLength = webResponse.ContentLength;
                this.ContentType = webResponse.ContentType;
                this.Headers = webResponse.Headers;
                this.IsFromCache = webResponse.IsFromCache;
                this.IsMutuallyAuthenticated = webResponse.IsMutuallyAuthenticated;
                this.ResponseUri = webResponse.ResponseUri;
            }
        }

        /// <summary>
        /// Gets or sets the character set of the response.
        /// </summary>
        public string CharacterSet
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the method that is used to encode the body of the response.
        /// </summary>
        public string ContentEncoding
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the length of the content returned by the request.
        /// </summary>
        public long ContentLength
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the content type of the response.
        /// </summary>
        public string ContentType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the cookies that are associated with this response.
        /// </summary>
        public CookieCollection Cookies
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the headers that are associated with this response from the server.
        /// </summary>
        public WebHeaderCollection Headers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this response was obtained from the cache..
        /// </summary>
        public bool IsFromCache
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether both client and server were authenticated.
        /// </summary>
        public bool IsMutuallyAuthenticated
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the last date and time that the contents of the response were modified.
        /// </summary>
        public DateTime LastModified
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the method that is used to return the response.
        /// </summary>
        public string Method
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the version of the HTTP protocol that is used in the response.
        /// </summary>
        public Version ProtocolVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the URI of the Internet resource that responded to the request.
        /// </summary>
        public Uri ResponseUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the server that sent the response.
        /// </summary>
        public string Server
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the status of the response.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the status description returned with the response.
        /// </summary>
        public string StatusDescription
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the content of response.
        /// </summary>
        public string Content
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="WebResponseMessage"/> is succeeded.
        /// </summary>
        public bool Succeeded
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage
        {
            get;
            set;
        }
    }
}
