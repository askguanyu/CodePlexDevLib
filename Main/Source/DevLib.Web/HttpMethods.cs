//-----------------------------------------------------------------------
// <copyright file="HttpMethods.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web
{
    /// <summary>
    /// Represents the types of HTTP protocol methods that can be used with an HTTP request.
    /// </summary>
    public static class HttpMethods
    {
        /// <summary>
        /// The GET method requests a representation of the specified resource. Requests using GET should only retrieve data and should have no other effect.
        /// </summary>
        public const string Get = "GET";

        /// <summary>
        /// The HEAD method asks for a response identical to that of a GET request, but without the response body. This is useful for retrieving meta-information written in response headers, without having to transport the entire content.
        /// </summary>
        public const string Head = "HEAD";

        /// <summary>
        /// The POST method requests that the server accept the entity enclosed in the request as a new subordinate of the web resource identified by the URI.
        /// </summary>
        public const string Post = "POST";

        /// <summary>
        /// The PUT method requests that the enclosed entity be stored under the supplied URI. If the URI refers to an already existing resource, it is modified; if the URI does not point to an existing resource, then the server can create the resource with that URI.
        /// </summary>
        public const string Put = "PUT";

        /// <summary>
        /// The DELETE method deletes the specified resource.
        /// </summary>
        public const string Delete = "DELETE";

        /// <summary>
        /// The CONNECT method converts the request connection to a transparent TCP/IP tunnel, usually to facilitate SSL-encrypted communication (HTTPS) through an unencrypted HTTP proxy.
        /// </summary>
        public const string Connect = "CONNECT";

        /// <summary>
        /// The OPTIONS method returns the HTTP methods that the server supports for the specified URL.
        /// </summary>
        public const string Options = "OPTIONS";

        /// <summary>
        /// The TRACE method echoes the received request so that a client can see what (if any) changes or additions have been made by intermediate servers.
        /// </summary>
        public const string Trace = "TRACE";

        /// <summary>
        /// The PATCH method applies partial modifications to a resource.
        /// </summary>
        public const string Patch = "PATCH";
    }
}
