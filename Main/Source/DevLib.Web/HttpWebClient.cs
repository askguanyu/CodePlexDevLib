//-----------------------------------------------------------------------
// <copyright file="HttpWebClient.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Provides a class for sending HTTP requests and receiving HTTP responses from a resource identified by a URI.
    /// </summary>
    [Serializable]
    public class HttpWebClient : MarshalByRefObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpWebClient"/> class.
        /// </summary>
        public HttpWebClient()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpWebClient"/> class.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        public HttpWebClient(string baseUri)
        {
            this.BaseUri = new Uri(baseUri);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpWebClient"/> class.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        public HttpWebClient(Uri baseUri)
        {
            this.BaseUri = baseUri;
        }

        /// <summary>
        /// Gets the base URI.
        /// </summary>
        public Uri BaseUri
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the URI.
        /// </summary>
        public Uri Uri
        {
            get
            {
                return this.BaseUri;
            }
        }

        /// <summary>
        /// Gets the http verb to use (GET, PUT, POST, DELETE...)
        /// </summary>
        public string Method
        {
            get;
            private set;
        }

        /// <summary>
        /// Sets the base URI.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <returns>Current instance.</returns>
        public HttpWebClient SetBaseUri(string baseUri)
        {
            this.BaseUri = new Uri(baseUri);

            return this;
        }

        /// <summary>
        /// Sets the base URI.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <returns>Current instance.</returns>
        public HttpWebClient SetBaseUri(Uri baseUri)
        {
            this.BaseUri = baseUri;

            return this;
        }

        /// <summary>
        /// Appends a resource path to base URI.
        /// </summary>
        /// <param name="value">The resource path.</param>
        /// <returns>Current instance.</returns>
        public HttpWebClient AppendPath(string value)
        {
            return this;
        }

        /// <summary>
        /// Appends a resource path to base URI.
        /// </summary>
        /// <param name="value">The resource path.</param>
        /// <returns>Current instance.</returns>
        public HttpWebClient AppendPath(Uri value)
        {
            return this;
        }

        /// <summary>
        /// Appends the slash ( / ).
        /// </summary>
        /// <returns>Current instance.</returns>
        public HttpWebClient AppendSlash()
        {
            return this;
        }

        /// <summary>
        /// Appends the double slash ( // ).
        /// </summary>
        /// <returns>Current instance.</returns>
        public HttpWebClient AppendDoubleSlash()
        {
            return this;
        }

        /// <summary>
        /// Appends the backslash ( \ ).
        /// </summary>
        /// <returns>Current instance.</returns>
        public HttpWebClient AppendBackslash()
        {
            return this;
        }

        /// <summary>
        /// Appends the double backslash ( \\ ).
        /// </summary>
        /// <returns>Current instance.</returns>
        public HttpWebClient AppendDoubleBackslash()
        {
            return this;
        }
    }
}
