//-----------------------------------------------------------------------
// <copyright file="UrlBuilder.cs" company="Yu Guan Corporation">
//     Copyright (c) Yu Guan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Net;
    using System.Text;

    /// <summary>
    /// Url builder.
    /// A generic Url form:
    /// scheme:[//[user:password@]host[:port]][/]path[?query][#fragment]
    /// Example:
    /// abc://username:password@example.com:123/path/data?key=value&amp;key2=value2#fragid1
    /// </summary>
    [Serializable]
    public class UrlBuilder : MarshalByRefObject
    {
        /// <summary>
        /// Specifies the characters "localhost".
        /// </summary>
        public const string Localhost = "localhost";

        /// <summary>
        /// Specifies the characters that separate the communication protocol scheme from the address portion of the URI.
        /// </summary>
        public const string SchemeDelimiter = "://";

        /// <summary>
        /// Specifies that the URI is accessed through the Hypertext Transfer Protocol (HTTP).
        /// </summary>
        public static readonly Scheme SchemeHttp = new Scheme("http", 80);

        /// <summary>
        /// Specifies that the URI is accessed through the Secure Hypertext Transfer Protocol (HTTPS).
        /// </summary>
        public static readonly Scheme SchemeHttps = new Scheme("https", 443);

        /// <summary>
        /// Specifies that the URI is accessed through the WS.
        /// </summary>
        public static readonly Scheme SchemeWs = new Scheme("ws", 80);

        /// <summary>
        /// Specifies that the URI is accessed through the WSS.
        /// </summary>
        public static readonly Scheme SchemeWss = new Scheme("wss", 443);

        /// <summary>
        /// Specifies that the URI is accessed through the File Transfer Protocol (FTP).
        /// </summary>
        public static readonly Scheme SchemeFtp = new Scheme("ftp", 21);

        /// <summary>
        /// Specifies that the URI is a pointer to a file.
        /// </summary>
        public static readonly Scheme SchemeFile = new Scheme("file");

        /// <summary>
        /// Specifies that the URI is accessed through the Gopher protocol.
        /// </summary>
        public static readonly Scheme SchemeGopher = new Scheme("gopher", 70);

        /// <summary>
        /// Specifies that the URI is an Internet news group and is accessed through the Network News Transport Protocol (NNTP).
        /// </summary>
        public static readonly Scheme SchemeNntp = new Scheme("nntp", 119);

        /// <summary>
        /// Specifies that the URI is an Internet news group and is accessed through the Network News Transport Protocol (NNTP).
        /// </summary>
        public static readonly Scheme SchemeNews = new Scheme("news");

        /// <summary>
        /// Specifies that the URI is an e-mail address and is accessed through the Simple Mail Transport Protocol (SMTP).
        /// </summary>
        public static readonly Scheme SchemeMailTo = new Scheme("mailto", 25);

        /// <summary>
        /// Specifies that the URI is accessed through the UUID.
        /// </summary>
        public static readonly Scheme SchemeUuid = new Scheme("uuid");

        /// <summary>
        /// Specifies that the URI is accessed through the telnet.
        /// </summary>
        public static readonly Scheme SchemeTelnet = new Scheme("telnet", 23);

        /// <summary>
        /// Specifies that the URI is accessed through the LDAP.
        /// </summary>
        public static readonly Scheme SchemeLdap = new Scheme("ldap", 389);

        /// <summary>
        /// Specifies that the URI is accessed through the NetTcp scheme used by Windows Communication Foundation (WCF).
        /// </summary>
        public static readonly Scheme SchemeNetTcp = new Scheme("net.tcp", 808);

        /// <summary>
        /// Specifies that the URI is accessed through the NetPipe scheme used by Windows Communication Foundation (WCF).
        /// </summary>
        public static readonly Scheme SchemeNetPipe = new Scheme("net.pipe");

        /// <summary>
        /// Specifies that the URI is accessed through the VsMacros.
        /// </summary>
        public static readonly Scheme SchemeVsMacros = new Scheme("vsmacros");

        /// <summary>
        /// Field _path.
        /// </summary>
        private readonly List<string> _path = new List<string>();

        /// <summary>
        /// Field _query.
        /// </summary>
        private readonly List<KeyValuePair<string, object>> _query = new List<KeyValuePair<string, object>>();

        /// <summary>
        /// Field _scheme.
        /// </summary>
        private string _scheme;

        /// <summary>
        /// Field _port.
        /// </summary>
        private int? _port;

        /// <summary>
        /// Field _user.
        /// </summary>
        private string _user;

        /// <summary>
        /// Field _password.
        /// </summary>
        private string _password;

        /// <summary>
        /// Field _host.
        /// </summary>
        private string _host;

        /// <summary>
        /// Field _fragment.
        /// </summary>
        private string _fragment;

        /// <summary>
        /// The _hasTrailingSlash
        /// </summary>
        private bool _hasTrailingSlash;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlBuilder"/> class.
        /// </summary>
        public UrlBuilder()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlBuilder"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        public UrlBuilder(string url)
        {
            if (!IsNullOrWhiteSpace(url))
            {
                this.Initialize(url);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlBuilder"/> class.
        /// </summary>
        /// <param name="uri">The URI.</param>
        public UrlBuilder(Uri uri)
        {
            if (uri != null)
            {
                this.Initialize(uri.ToString());
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlBuilder"/> class.
        /// </summary>
        /// <param name="urlBuilder">The UrlBuilder instance.</param>
        public UrlBuilder(UrlBuilder urlBuilder)
        {
            if (urlBuilder != null)
            {
                this.Initialize(urlBuilder.ToString());
            }
        }

        /// <summary>
        /// Returns the root URL of the given full URL, including the scheme, any user info, host, and port (if specified).
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The root URL.</returns>
        public static string GetRoot(string url)
        {
            return new Uri(url).GetComponents(UriComponents.SchemeAndServer | UriComponents.UserInfo, UriFormat.Unescaped);
        }

        /// <summary>
        /// Decodes a URL-encoded query string value.
        /// </summary>
        /// <param name="value">The encoded query string value.</param>
        /// <returns>The decoded query string value.</returns>
        public static string DecodeQueryParamValue(string value)
        {
            return Uri.UnescapeDataString((value ?? string.Empty).Replace("+", " "));
        }

        /// <summary>
        /// Encodes a query string value.
        /// </summary>
        /// <param name="value">The query string value to encode.</param>
        /// <param name="encodeSpaceAsPlus">If true, spaces will be encoded as + signs. Otherwise, it is encoded as %20.</param>
        /// <returns>The encoded query string value.</returns>
        public static string EncodeQueryParamValue(object value, bool encodeSpaceAsPlus = true)
        {
            var result = Uri.EscapeDataString(ToInvariantString(value ?? string.Empty));
            return encodeSpaceAsPlus ? result.Replace("%20", "+") : result;
        }

        /// <summary>
        /// Encodes characters that are illegal in a URL path, including '?'. Does not encode reserved characters, i.e. '/', '+', etc.
        /// </summary>
        /// <param name="segment">The path segment.</param>
        /// <returns>The encoded URL path segment.</returns>
        public static string CleanSegment(string segment)
        {
            var unescaped = Uri.UnescapeDataString(segment);

            return Uri
                .EscapeUriString(unescaped)
                .Replace("?", "%3F")
                .Trim()
                .Trim('/')
                .Trim(' ', '\r', '\n');
        }

        /// <summary>
        /// Encodes characters that are illegal in a URL path, including '?'. Does not encode reserved characters, i.e. '/', '+', etc.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <returns>The encoded URL path segments.</returns>
        public static List<string> CleanSegment(IList<string> segments)
        {
            List<string> result = new List<string>();

            foreach (var item in segments)
            {
                var cleanSegment = CleanSegment(item);

                if (!IsNullOrWhiteSpace(cleanSegment))
                {
                    result.Add(cleanSegment);
                }
            }

            return result;
        }

        /// <summary>
        /// Checks the URL is valid or not.
        /// </summary>
        /// <param name="url">The URL to check.</param>
        /// <returns>true if the specified URL is valid; otherwise, false.</returns>
        public static bool IsValidUrl(string url)
        {
            return !IsNullOrWhiteSpace(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UrlBuilder"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="urlBuilder">The URL builder.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator string(UrlBuilder urlBuilder)
        {
            return urlBuilder.ToString();
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="UrlBuilder"/>.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator UrlBuilder(string url)
        {
            return new UrlBuilder(url);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UrlBuilder"/> to <see cref="Uri"/>.
        /// </summary>
        /// <param name="urlBuilder">The URL builder.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Uri(UrlBuilder urlBuilder)
        {
            return urlBuilder.ToUri();
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Uri"/> to <see cref="UrlBuilder"/>.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator UrlBuilder(Uri uri)
        {
            return new UrlBuilder(uri);
        }

        /// <summary>
        /// Returns the root URL, including the scheme, any user info, host, and port (if specified).
        /// </summary>
        /// <returns>The root URL.</returns>
        public string GetRoot()
        {
            return GetRoot(this.ToString());
        }

        /// <summary>
        /// Sets the URL.
        /// </summary>
        /// <param name="url">The URL to set.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder SetUrl(string url)
        {
            this.CheckNullOrWhiteSpace("url", url);

            this.Initialize(url);

            return this;
        }

        /// <summary>
        /// Sets the URL.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder SetUrl(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            this.Initialize(uri.ToString());

            return this;
        }

        /// <summary>
        /// Sets the URL.
        /// </summary>
        /// <param name="urlBuilder">The UrlBuilder instance.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder SetUrl(UrlBuilder urlBuilder)
        {
            if (urlBuilder == null)
            {
                throw new ArgumentNullException("urlBuilder");
            }

            this.Initialize(urlBuilder.ToString());

            return this;
        }

        /// <summary>
        /// Determines whether the current UrlBuilder instance is valid URL.
        /// </summary>
        /// <returns>true if the current UrlBuilder instance is valid URL; otherwise, false.</returns>
        public bool IsValidUrl()
        {
            return IsValidUrl(this.ToString());
        }

        /// <summary>
        /// Sets the scheme name of the UrlBuilder.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder SetScheme(string scheme)
        {
            var trimScheme = (scheme ?? string.Empty).Trim(' ', '/', '\\', '\r', '\n');

            this.CheckNullOrWhiteSpace("scheme", trimScheme);

            this._scheme = trimScheme.ToLowerInvariant();

            return this;
        }

        /// <summary>
        /// Sets the scheme name of the UrlBuilder.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder SetScheme(Scheme scheme)
        {
            if (scheme == null)
            {
                throw new ArgumentNullException("scheme");
            }

            this._scheme = scheme.Name;
            this._port = scheme.DefaultPort;

            return this;
        }

        /// <summary>
        /// Removes the scheme.
        /// </summary>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder RemoveScheme()
        {
            this._scheme = null;

            return this;
        }

        /// <summary>
        /// Gets the scheme name of the UrlBuilder.
        /// </summary>
        /// <returns>The scheme name.</returns>
        public string GetScheme()
        {
            return this._scheme;
        }

        /// <summary>
        /// Determines whether the current UrlBuilder instance has scheme.
        /// </summary>
        /// <returns>true if the current UrlBuilder instance has scheme; otherwise, false.</returns>
        public bool HasScheme()
        {
            return !IsNullOrWhiteSpace(this._scheme);
        }

        /// <summary>
        /// Sets the port number of the UrlBuilder.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder SetPort(int port)
        {
            this.CheckIsValidPort(port);

            this._port = port;

            return this;
        }

        /// <summary>
        /// Removes the port.
        /// </summary>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder RemovePort()
        {
            this._port = null;

            return this;
        }

        /// <summary>
        /// Gets the port number of the UrlBuilder.
        /// </summary>
        /// <returns>The port number.</returns>
        public int? GetPort()
        {
            return this._port;
        }

        /// <summary>
        /// Determines whether the current UrlBuilder instance has port.
        /// </summary>
        /// <returns>true if the current UrlBuilder instance has port; otherwise, false.</returns>
        public bool HasPort()
        {
            return this._port.HasValue;
        }

        /// <summary>
        /// Sets the authority.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder SetAuthority(string user, string password = null)
        {
            this.CheckNullOrWhiteSpace("user", user);

            this._user = user;
            this._password = password;

            return this;
        }

        /// <summary>
        /// Removes the authority.
        /// </summary>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder RemoveAuthority()
        {
            this._user = null;
            this._password = null;

            return this;
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <returns>The user.</returns>
        public string GetUser()
        {
            return this._user;
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <returns>The password.</returns>
        public string GetPassword()
        {
            return this._password;
        }

        /// <summary>
        /// Determines whether the current UrlBuilder instance has authority.
        /// </summary>
        /// <returns>true if the current UrlBuilder instance has authority; otherwise, false.</returns>
        public bool HasAuthority()
        {
            return !IsNullOrWhiteSpace(this._user);
        }

        /// <summary>
        /// Sets the Domain Name System (DNS) host name or IP address.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder SetHost(string host)
        {
            var trimHost = (host ?? string.Empty).Trim(' ', '\r', '\n');

            this.CheckNullOrWhiteSpace("host", trimHost);

            this._host = trimHost;

            return this;
        }

        /// <summary>
        /// Removes the host.
        /// </summary>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder RemoveHost()
        {
            this._host = null;

            return this;
        }

        /// <summary>
        /// Gets the Domain Name System (DNS) host name or IP address.
        /// </summary>
        /// <returns>The host.</returns>
        public string GetHost()
        {
            return this._host;
        }

        /// <summary>
        /// Determines whether the current UrlBuilder instance has host.
        /// </summary>
        /// <returns>true if the current UrlBuilder instance has host; otherwise, false.</returns>
        public bool HasHost()
        {
            return !IsNullOrWhiteSpace(this._host);
        }

        /// <summary>
        /// Sets the paths of the UrlBuilder.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder SetPath(params string[] paths)
        {
            if (paths == null || paths.Length < 1)
            {
                throw new ArgumentException("paths cannot be null or empty.", "paths");
            }

            this._path.Clear();

            return this.DoAppendPath(paths);
        }

        /// <summary>
        /// Appends the paths of the UrlBuilder.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder AppendPath(params string[] paths)
        {
            if (paths == null || paths.Length < 1)
            {
                throw new ArgumentException("paths cannot be null or empty.", "paths");
            }

            return this.DoAppendPath(paths);
        }

        /// <summary>
        /// Inserts the paths of the UrlBuilder at first index.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder InsertPath(params string[] paths)
        {
            if (paths == null || paths.Length < 1)
            {
                throw new ArgumentException("paths cannot be null or empty.", "paths");
            }

            var segments = this.GetPathSegments(paths);

            int index = 0;

            foreach (var item in segments)
            {
                var cleanSegment = CleanSegment(item);

                if (!IsNullOrWhiteSpace(cleanSegment))
                {
                    this._path.Insert(index, cleanSegment);
                    index++;
                }
            }

            return this;
        }

        /// <summary>
        /// Removes all path.
        /// </summary>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder RemovePath()
        {
            this._path.Clear();

            return this;
        }

        /// <summary>
        /// Removes the first occurrence of path segments.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder RemoveFirstPath(params string[] paths)
        {
            return this.DoRemovePath(true, paths);
        }

        /// <summary>
        /// Removes the last occurrence of path segments.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder RemoveLastPath(params string[] paths)
        {
            return this.DoRemovePath(false, paths);
        }

        /// <summary>
        /// Determines whether the current UrlBuilder instance contains the specified paths.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>true if contains the specified paths; otherwise, false.</returns>
        public bool ContainsPathSegment(params string[] paths)
        {
            if (paths == null || paths.Length < 1)
            {
                throw new ArgumentException("paths cannot be null or empty.", "paths");
            }

            var segments = this.GetPathSegments(paths);

            return this.FindArrayIndex(this._path.ToArray(), segments.ToArray()) > -1;
        }

        /// <summary>
        /// Gets the path string.
        /// </summary>
        /// <returns>The path.</returns>
        public string GetPath()
        {
            var path = string.Join("/", this._path.ToArray()).Trim();

            if (this._path.Count > 0 && this._hasTrailingSlash)
            {
                path += "/";
            }

            return path;
        }

        /// <summary>
        /// Determines whether the current UrlBuilder instance has path.
        /// </summary>
        /// <returns>true if the current UrlBuilder instance has path; otherwise, false.</returns>
        public bool HasPath()
        {
            return this._path.Count > 0 && !this._path.TrueForAll(item => IsNullOrWhiteSpace(item));
        }

        /// <summary>
        /// Appends the query.
        /// </summary>
        /// <param name="name">The query name.</param>
        /// <param name="values">The query values.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder AppendQuery(string name, params object[] values)
        {
            this.CheckNullOrWhiteSpace("name", name);

            if (values == null || values.Length < 1)
            {
                throw new ArgumentException("values cannot be null or empty.", "values");
            }

            return this.DoAppendQuery(name, values);
        }

        /// <summary>
        /// Appends the query.
        /// </summary>
        /// <param name="values">The query key value pairs.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder AppendQuery(params KeyValuePair<string, object>[] values)
        {
            if (values == null || values.Length < 1)
            {
                throw new ArgumentException("values cannot be null or empty.", "values");
            }

            return this.DoAppendQuery(values);
        }

        /// <summary>
        /// Appends the query.
        /// </summary>
        /// <param name="values">The query key value pairs.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder AppendQuery(params KeyValuePair<string, object[]>[] values)
        {
            if (values == null || values.Length < 1)
            {
                throw new ArgumentException("values cannot be null or empty.", "values");
            }

            return this.DoAppendQuery(values);
        }

        /// <summary>
        /// Appends the query string.
        /// </summary>
        /// <param name="queryStrings">The query strings.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder AppendQueryString(params string[] queryStrings)
        {
            if (queryStrings == null || queryStrings.Length < 1)
            {
                throw new ArgumentException("queryStrings cannot be null or empty.", "queryStrings");
            }

            return this.DoAppendQueryString(queryStrings);
        }

        /// <summary>
        /// Sets a parameter to the query string, overwriting the value if name exists.
        /// </summary>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <param name="name">The name of query string parameter.</param>
        /// <param name="values">The query values.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder SetQuery(bool ignoreCase, string name, params object[] values)
        {
            this.CheckNullOrWhiteSpace("name", name);

            if (values == null || values.Length < 1)
            {
                throw new ArgumentException("values cannot be null or empty.", "values");
            }

            if (ignoreCase)
            {
                this._query.RemoveAll(item => item.Key.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                this._query.RemoveAll(item => item.Key.Equals(name));
            }

            return this.DoAppendQuery(name, values);
        }

        /// <summary>
        /// Sets the query, overwriting the value if name exists.
        /// </summary>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <param name="values">The query key value pairs.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder SetQuery(bool ignoreCase, params KeyValuePair<string, object>[] values)
        {
            if (values == null || values.Length < 1)
            {
                throw new ArgumentException("values cannot be null or empty.", "values");
            }

            StringComparison stringComparison =
                ignoreCase
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            foreach (var pair in values)
            {
                if (IsNullOrWhiteSpace(pair.Key))
                {
                    this._query.RemoveAll(item => item.Key.Equals(pair.Key, stringComparison));
                }
            }

            return this.DoAppendQuery(values);
        }

        /// <summary>
        /// Sets the query, overwriting the value if name exists.
        /// </summary>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <param name="values">The query key value pairs.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder SetQuery(bool ignoreCase, params KeyValuePair<string, object[]>[] values)
        {
            if (values == null || values.Length < 1)
            {
                throw new ArgumentException("values cannot be null or empty.", "values");
            }

            StringComparison stringComparison =
                ignoreCase
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            foreach (var pair in values)
            {
                if (IsNullOrWhiteSpace(pair.Key))
                {
                    this._query.RemoveAll(item => item.Key.Equals(pair.Key, stringComparison));
                }
            }

            return this.DoAppendQuery(values);
        }

        /// <summary>
        /// Sets the query string, overwriting all values.
        /// </summary>
        /// <param name="queryStrings">The query strings.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder SetQueryString(params string[] queryStrings)
        {
            if (queryStrings == null || queryStrings.Length < 1)
            {
                throw new ArgumentException("queryStrings cannot be null or empty.", "queryStrings");
            }

            this._query.Clear();

            return this.DoAppendQueryString(queryStrings);
        }

        /// <summary>
        /// Removes the query.
        /// </summary>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <param name="names">The query names.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder RemoveQuery(bool ignoreCase, params string[] names)
        {
            if (names == null || names.Length < 1)
            {
                throw new ArgumentException("names cannot be null or empty.", "names");
            }

            StringComparison stringComparison =
                ignoreCase
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            foreach (var name in names)
            {
                if (IsNullOrWhiteSpace(name))
                {
                    this._query.RemoveAll(item => item.Key.Equals(name, stringComparison));
                }
            }

            return this;
        }

        /// <summary>
        /// Removes all querys.
        /// </summary>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder RemoveQuery()
        {
            this._query.Clear();

            return this;
        }

        /// <summary>
        /// Determines whether the query contains the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <returns>true if the query contains the specified name; otherwise, false.</returns>
        public bool ContainsQuery(string name, bool ignoreCase = false)
        {
            this.CheckNullOrWhiteSpace("name", name);

            if (ignoreCase)
            {
                return this._query.FindIndex(item => item.Key.Equals(name, StringComparison.OrdinalIgnoreCase)) > -1;
            }
            else
            {
                return this._query.FindIndex(item => item.Key.Equals(name)) > -1;
            }
        }

        /// <summary>
        /// Gets query values by name.
        /// </summary>
        /// <param name="name">The name of query param.</param>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <returns>The query values.</returns>
        public List<object> GetQueryValues(string name, bool ignoreCase = false)
        {
            this.CheckNullOrWhiteSpace("name", name);

            StringComparison stringComparison =
                ignoreCase
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            return this
                ._query
                .FindAll(item => item.Key.Equals(name, stringComparison))
                .ConvertAll(item => item.Value);
        }

        /// <summary>
        /// Gets query names.
        /// </summary>
        /// <returns>The list of query names.</returns>
        public List<string> GetQueryNames()
        {
            return this
                ._query
                .ConvertAll(item => item.Key);
        }

        /// <summary>
        /// Gets all query key value pairs.
        /// </summary>
        /// <returns>The query key value pairs.</returns>
        public ReadOnlyCollection<KeyValuePair<string, object>> GetQuery()
        {
            return this
                ._query
                .AsReadOnly();
        }

        /// <summary>
        /// Gets the query string.
        /// </summary>
        /// <param name="encodeSpaceAsPlus">If true, spaces will be encoded as + signs. Otherwise, it is encoded as %20.</param>
        /// <returns>The query string.</returns>
        public string GetQueryString(bool encodeSpaceAsPlus = true)
        {
            return string.Join(
                "&",
                this._query
                    .ConvertAll(item =>
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "{0}={1}",
                            item.Key,
                            EncodeQueryParamValue(item.Value, encodeSpaceAsPlus)))
                    .ToArray());
        }

        /// <summary>
        /// Determines whether the current UrlBuilder instance has query.
        /// </summary>
        /// <returns>true if the current UrlBuilder instance has query; otherwise, false.</returns>
        public bool HasQuery()
        {
            return this._query.Count > 0 && !this._query.TrueForAll(item => IsNullOrWhiteSpace(item.Key));
        }

        /// <summary>
        /// Set the URL fragment.
        /// </summary>
        /// <param name="fragment">The part of the URL after #.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder SetFragment(string fragment)
        {
            var trimFragment = (fragment ?? string.Empty).Trim(' ', '#', '/', '\\', '\r', '\n');

            this.CheckNullOrWhiteSpace("fragment", trimFragment);

            this._fragment = trimFragment;

            return this;
        }

        /// <summary>
        /// Removes the URL fragment including the #.
        /// </summary>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder RemoveFragment()
        {
            this._fragment = null;

            return this;
        }

        /// <summary>
        /// Gets the fragment.
        /// </summary>
        /// <returns>The fragment.</returns>
        public string GetFragment()
        {
            return this._fragment;
        }

        /// <summary>
        /// Determines whether the current UrlBuilder instance has fragment.
        /// </summary>
        /// <returns>true if the current UrlBuilder instance has fragment; otherwise, false.</returns>
        public bool HasFragment()
        {
            return !IsNullOrWhiteSpace(this._fragment);
        }

        /// <summary>
        /// Resets to the root URL of the current UrlBuilder instance, will keep the scheme, any user info, host, and port (if specified).
        /// </summary>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder ResetToRoot()
        {
            return this
                .RemovePath()
                .RemoveQuery()
                .RemoveFragment();
        }

        /// <summary>
        /// Resets the current UrlBuilder instance to empty URL.
        /// </summary>
        /// <returns>The current UrlBuilder instance.</returns>
        public UrlBuilder Reset()
        {
            return this
                .RemoveScheme()
                .RemoveAuthority()
                .RemoveHost()
                .RemovePort()
                .RemovePath()
                .RemoveQuery()
                .RemoveFragment();
        }

        /// <summary>
        /// Gets Uri from the current UrlBuilder instance.
        /// </summary>
        /// <returns>The Uri instance.</returns>
        public Uri ToUri()
        {
            return new Uri(this.ToString());
        }

        /// <summary>
        /// Gets relative Uri from the current UrlBuilder instance.
        /// </summary>
        /// <returns>The Uri instance.</returns>
        public Uri ToRelativeUri()
        {
            var baseUriBuilder = new UriBuilder
            {
                Scheme = this._scheme,
                Host = this._host,
            };

            if (this._port.HasValue)
            {
                baseUriBuilder.Port = this._port.Value;
            }

            var uri = this.ToUri();

            return baseUriBuilder.Uri.MakeRelativeUri(uri);
        }

        /// <summary>
        /// Gets the relative URL from the current UrlBuilder instance.
        /// </summary>
        /// <returns>The relative URL string.</returns>
        public string ToRelativeString()
        {
            var uri = this.ToRelativeUri();
            return uri.ToString();
        }

        /// <summary>
        /// Returns a URL string that represents this instance.
        /// </summary>
        /// <returns>A URL string that represents this instance.</returns>
        public override string ToString()
        {
            //// scheme:[//[user:password@]host[:port]][/]path[?query][#fragment]
            //// abc://username:password@example.com:123/path/data?key=value&key2=value2#fragid1

            StringBuilder result = new StringBuilder();

            if (this.HasScheme())
            {
                result.Append(this._scheme);
                result.Append(SchemeDelimiter);
            }

            if (this.HasAuthority())
            {
                result.Append(this._user);

                if (!IsNullOrWhiteSpace(this._password))
                {
                    result.Append(":");
                    result.Append(this._password);
                }

                result.Append("@");
            }

            if (this.HasHost())
            {
                result.Append(this._host);
            }

            if (this.HasPort())
            {
                result.Append(":");
                result.Append(this._port.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (this.HasPath())
            {
                var path = this.GetPath();
                result.Append("/");
                result.Append((this.HasQuery() || this.HasFragment()) ? path.Trim('/') : path);
            }

            if (this.HasQuery())
            {
                result.Append("?");
                result.Append(this.GetQueryString());
            }

            if (this.HasFragment())
            {
                result.Append("#");
                result.Append(this._fragment);
            }

            var resultUrl = result.ToString();

            result.Length = 0;

            return resultUrl;
        }

        /// <summary>
        /// Returns a string that represents the object, using CultureInfo.InvariantCulture if possible.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>A URL string that represents the object.</returns>
        private static string ToInvariantString(object obj)
        {
            var convertibleObj = obj as IConvertible;

            if (convertibleObj != null)
            {
                return convertibleObj.ToString(CultureInfo.InvariantCulture);
            }

            var formattableObj = obj as IFormattable;

            if (formattableObj != null)
            {
                return formattableObj.ToString(null, CultureInfo.InvariantCulture);
            }

            return obj.ToString();
        }

        /// <summary>
        /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the value parameter is null or String.Empty, or if value consists exclusively of white-space characters.</returns>
        private static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null)
            {
                return true;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the path segments.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>The list of path segments.</returns>
        private List<string> GetPathSegments(string[] paths)
        {
            var result = new List<string>();

            foreach (var path in paths)
            {
                result.AddRange(path.Trim().Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
            }

            return CleanSegment(result);
        }

        /// <summary>
        /// Appends path.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        private UrlBuilder DoAppendPath(params string[] paths)
        {
            foreach (var path in paths)
            {
                var segments = path.Trim().Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var item in segments)
                {
                    var cleanSegment = CleanSegment(item);

                    if (!IsNullOrWhiteSpace(cleanSegment))
                    {
                        this._path.Add(cleanSegment);
                    }
                }

                this._hasTrailingSlash = path.EndsWith("/");
            }

            return this;
        }

        /// <summary>
        /// Removes path.
        /// </summary>
        /// <param name="firstToLast">true to start with the first occurrence; false to start with the last occurrence.</param>
        /// <param name="paths">The paths.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        private UrlBuilder DoRemovePath(bool firstToLast, params string[] paths)
        {
            if (paths == null || paths.Length < 1)
            {
                throw new ArgumentException("paths cannot be null or empty.", "paths");
            }

            var segments = this.GetPathSegments(paths);

            int index = firstToLast
                ? this.FindArrayIndex(this._path.ToArray(), segments.ToArray())
                : this.FindArrayLastIndex(this._path.ToArray(), segments.ToArray());

            if (index > -1)
            {
                this._path.RemoveRange(index, segments.Count);
            }

            return this;
        }

        /// <summary>
        /// Appends query.
        /// </summary>
        /// <param name="name">The query name.</param>
        /// <param name="values">The query values.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        private UrlBuilder DoAppendQuery(string name, object[] values)
        {
            foreach (var item in values)
            {
                this._query.Add(new KeyValuePair<string, object>(name, item));
            }

            return this;
        }

        /// <summary>
        /// Appends query.
        /// </summary>
        /// <param name="values">The query key value pairs.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        private UrlBuilder DoAppendQuery(KeyValuePair<string, object>[] values)
        {
            foreach (var item in values)
            {
                this.CheckNullOrWhiteSpace("Query parameter name", item.Key);
                this._query.Add(new KeyValuePair<string, object>(item.Key, item.Value));
            }

            return this;
        }

        /// <summary>
        /// Appends query.
        /// </summary>
        /// <param name="values">The query key value pairs.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        private UrlBuilder DoAppendQuery(KeyValuePair<string, object[]>[] values)
        {
            foreach (var item in values)
            {
                this.CheckNullOrWhiteSpace("Query parameter name", item.Key);
                this.DoAppendQuery(item.Key, item.Value);
            }

            return this;
        }

        /// <summary>
        /// Appends query string.
        /// </summary>
        /// <param name="queryStrings">The query strings.</param>
        /// <returns>The current UrlBuilder instance.</returns>
        private UrlBuilder DoAppendQueryString(params string[] queryStrings)
        {
            foreach (var item in queryStrings)
            {
                var querySegments = (item ?? string.Empty).Trim(' ', '?').Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var query in querySegments)
                {
                    var pair = query.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                    if (pair.Length > 0)
                    {
                        var key = pair[0];
                        var value = pair.Length >= 2 ? DecodeQueryParamValue(pair[1]) : string.Empty;

                        this._query.Add(new KeyValuePair<string, object>(key, value));
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// Checks the port is valid or not.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="throwOnCheck">true to throw exception on error; otherwise, false.</param>
        /// <returns>true if port number is valid; otherwise, false.</returns>
        private bool CheckIsValidPort(int port, bool throwOnCheck = true)
        {
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                if (throwOnCheck)
                {
                    throw new ArgumentOutOfRangeException("port", port, "Port number is less than System.Net.IPEndPoint.MinPort.-or- Port number is greater than System.Net.IPEndPoint.MaxPort.");
                }

                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Checks a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        private void CheckNullOrWhiteSpace(string paramName, string value)
        {
            if (IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Parameter value cannot be null or empty.", paramName);
            }
        }

        /// <summary>
        /// Find the first occurrence of string array in another string array.
        /// </summary>
        /// <param name="source">The array to search in.</param>
        /// <param name="pattern">The array to find.</param>
        /// <returns>The first position of the found array or -1 if not found.</returns>
        private int FindArrayIndex(string[] source, string[] pattern)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (pattern == null)
            {
                throw new ArgumentNullException("pattern");
            }

            if (pattern.Length == 0)
            {
                return 0;
            }

            int j = -1;
            int end = source.Length - pattern.Length;

            while (((j = Array.IndexOf(source, pattern[0], j + 1)) <= end) && (j != -1))
            {
                int i = 0;

                while (source[j + i].Equals(pattern[i], StringComparison.OrdinalIgnoreCase))
                {
                    if (++i == pattern.Length)
                    {
                        return j;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Find the last occurrence of string array in another string array.
        /// </summary>
        /// <param name="source">The array to search in.</param>
        /// <param name="pattern">The array to find.</param>
        /// <returns>The last position of the found array or -1 if not found.</returns>
        private int FindArrayLastIndex(string[] source, string[] pattern)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (pattern == null)
            {
                throw new ArgumentNullException("pattern");
            }

            if (pattern.Length == 0)
            {
                return source.Length - 1;
            }

            int end = source.Length - pattern.Length;
            int j = end + 1;

            while (((j = Array.LastIndexOf(source, pattern[0], j - 1)) >= 0) && (j != -1))
            {
                int i = 0;

                while (source[j + i].Equals(pattern[i], StringComparison.OrdinalIgnoreCase))
                {
                    if (++i == pattern.Length)
                    {
                        return j;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Initialize UrlBuilder fields from url string.
        /// </summary>
        /// <param name="url">The URL string.</param>
        private void Initialize(string url)
        {
            var uriBuilder = new UriBuilder(url);

            this._scheme = uriBuilder.Scheme;
            this._user = uriBuilder.UserName;
            this._password = uriBuilder.Password;
            this._host = uriBuilder.Host;
            this._port = this.CheckIsValidPort(uriBuilder.Port, false) ? uriBuilder.Port : default(int?);
            this._fragment = (uriBuilder.Fragment ?? string.Empty).Trim(' ', '#', '/', '\\', '\r', '\n');

            this._path.Clear();
            this.DoAppendPath(uriBuilder.Path ?? string.Empty);

            this._query.Clear();
            this.DoAppendQueryString(uriBuilder.Query ?? string.Empty);
        }

        /// <summary>
        /// Class Scheme.
        /// </summary>
        [Serializable]
        public class Scheme : MarshalByRefObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Scheme"/> class.
            /// </summary>
            /// <param name="name">The scheme name.</param>
            /// <param name="defaultPort">The default port.</param>
            internal Scheme(string name, int? defaultPort = null)
            {
                this.Name = name;
                this.DefaultPort = defaultPort;
            }

            /// <summary>
            /// Prevents a default instance of the <see cref="Scheme"/> class from being created.
            /// </summary>
            private Scheme()
            {
            }

            /// <summary>
            /// Gets the scheme name.
            /// </summary>
            /// <value>The scheme name.</value>
            public string Name
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the default port.
            /// </summary>
            /// <value>The default port.</value>
            public int? DefaultPort
            {
                get;
                private set;
            }
        }
    }
}
