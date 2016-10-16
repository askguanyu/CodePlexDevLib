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
    using System.Net;

    /// <summary>
    /// Provides a class for sending HTTP requests and receiving HTTP responses from a resource identified by a URI.
    /// </summary>
    [Serializable]
    public class HttpWebClient : MarshalByRefObject
    {
        /// <summary>
        /// Field _urlBuilder.
        /// </summary>
        private readonly UrlBuilder _urlBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpWebClient"/> class.
        /// </summary>
        public HttpWebClient()
        {
            this._urlBuilder = new UrlBuilder();
            this.Method = HttpMethods.Get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpWebClient"/> class.
        /// </summary>
        /// <param name="url">The url to use.</param>
        public HttpWebClient(string url)
        {
            this._urlBuilder = new UrlBuilder(url);
            this.Method = HttpMethods.Get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpWebClient"/> class.
        /// </summary>
        /// <param name="uri">The uri to use.</param>
        public HttpWebClient(Uri uri)
        {
            this._urlBuilder = new UrlBuilder(uri);
            this.Method = HttpMethods.Get;
        }

        /// <summary>
        /// Gets the root URL, including the scheme, any user info, host, and port (if specified).
        /// </summary>
        public string BaseAddress
        {
            get
            {
                return this._urlBuilder.GetRoot();
            }
        }

        /// <summary>
        /// Gets the http verb to use (HTTP 1.1 protocol verbs: GET, HEAD, POST, PUT, DELETE, CONNECT, OPTIONS, TRACE, PATCH).
        /// </summary>
        public string Method
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the value of the Accept HTTP header.
        /// </summary>
        public string Accept { get; private set; }

        /// <summary>
        /// Gets the value of the Content-type HTTP header.
        /// </summary>
        public string ContentType { get; private set; }

        /// <summary>
        /// Gets the value of the User-agent HTTP header.
        /// </summary>
        public string UserAgent { get; private set; }

        /// <summary>
        /// Sets the method for the request.
        /// </summary>
        /// <param name="method">The request method to use to contact the Internet resource.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetMethod(string method)
        {
            this.Method = method;
            return this;
        }

        /// <summary>
        /// Sets http method to GET.
        /// </summary>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient MethodGet()
        {
            this.Method = "GET";
            return this;
        }

        /// <summary>
        /// Sets http method to POST.
        /// </summary>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient MethodPost()
        {
            this.Method = "POST";
            return this;
        }

        /// <summary>
        /// Sets http method to PUT.
        /// </summary>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient MethodPut()
        {
            this.Method = "PUT";
            return this;
        }

        /// <summary>
        /// Sets http method to DELETE.
        /// </summary>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient MethodDelete()
        {
            this.Method = "DELETE";
            return this;
        }

        /// <summary>
        /// Sets http method to HEAD.
        /// </summary>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient MethodHead()
        {
            this.Method = "HEAD";
            return this;
        }

        /// <summary>
        /// Sets http method to TRACE.
        /// </summary>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient MethodTrace()
        {
            this.Method = "TRACE";
            return this;
        }

        /// <summary>
        /// Sets http method to OPTIONS.
        /// </summary>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient MethodOptions()
        {
            this.Method = "OPTIONS";
            return this;
        }

        /// <summary>
        /// Sets http method to CONNECT.
        /// </summary>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient MethodConnect()
        {
            this.Method = "CONNECT";
            return this;
        }

        /// <summary>
        /// Sets http method to PATCH.
        /// </summary>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient MethodPatch()
        {
            this.Method = "PATCH";
            return this;
        }

        /// <summary>
        /// Sets the value of the Accept HTTP header.
        /// </summary>
        /// <param name="accept">The value of the Accept HTTP header.</param>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetAccept(Evaluate<string> accept, Evaluate<bool> canProceed = null)
        {
            if (accept != null && (canProceed == null || canProceed()))
            {
                this.Accept = accept();
            }

            return this;
        }

        /// <summary>
        /// Sets the value of the Content-type HTTP header.
        /// </summary>
        /// <param name="contentType">The value of the Content-type HTTP header.</param>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetContentType(Evaluate<string> contentType, Evaluate<bool> canProceed = null)
        {
            if (contentType != null && (canProceed == null || canProceed()))
            {
                this.ContentType = contentType();
            }

            return this;
        }

        /// <summary>
        /// Sets the value of the User-agent HTTP header.
        /// </summary>
        /// <param name="userAgent">The value of the User-agent HTTP header.</param>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetUserAgent(Evaluate<string> userAgent, Evaluate<bool> canProceed = null)
        {
            if (userAgent != null && (canProceed == null || canProceed()))
            {
                this.UserAgent = userAgent();
            }

            return this;
        }

        /// <summary>
        /// Sets the URL.
        /// </summary>
        /// <param name="url">The URL to set.</param>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetUrl(Evaluate<string> url, Evaluate<bool> canProceed = null)
        {
            this._urlBuilder.SetUrl(url, canProceed);
            return this;
        }

        /// <summary>
        /// Sets the URL.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetUrl(Evaluate<Uri> uri, Evaluate<bool> canProceed = null)
        {
            this._urlBuilder.SetUrl(uri, canProceed);
            return this;
        }

        /// <summary>
        /// Sets the URL.
        /// </summary>
        /// <param name="urlBuilder">The UrlBuilder instance.</param>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetUrl(Evaluate<UrlBuilder> urlBuilder, Evaluate<bool> canProceed = null)
        {
            this._urlBuilder.SetUrl(urlBuilder, canProceed);
            return this;
        }

        /// <summary>
        /// Sets the scheme name of the UrlBuilder.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetScheme(Evaluate<string> scheme, Evaluate<bool> canProceed = null)
        {
            this._urlBuilder.SetScheme(scheme, canProceed);
            return this;
        }

        /// <summary>
        /// Sets the scheme name of the UrlBuilder.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetScheme(Evaluate<UrlBuilder.Scheme> scheme, Evaluate<bool> canProceed = null)
        {
            this._urlBuilder.SetScheme(scheme, canProceed);
            return this;
        }

        /// <summary>
        /// Removes the scheme.
        /// </summary>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient RemoveScheme(Evaluate<bool> canProceed = null)
        {
            this._urlBuilder.RemoveScheme(canProceed);
            return this;
        }

        /// <summary>
        /// Sets the port number of the UrlBuilder.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetPort(Evaluate<int> port, Evaluate<bool> canProceed = null)
        {
            this._urlBuilder.SetPort(port, canProceed);
            return this;
        }

        /// <summary>
        /// Removes the port.
        /// </summary>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient RemovePort(Evaluate<bool> canProceed = null)
        {
            this._urlBuilder.RemovePort(canProceed);
            return this;
        }

        /// <summary>
        /// Sets the authority.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetAuthority(Evaluate<string> user, Evaluate<string> password, Evaluate<bool> canProceed = null)
        {
            this._urlBuilder.SetAuthority(user, password, canProceed);
            return this;
        }

        /// <summary>
        /// Removes the authority.
        /// </summary>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient RemoveAuthority(Evaluate<bool> canProceed = null)
        {
            this._urlBuilder.RemoveAuthority(canProceed);
            return this;
        }

        /// <summary>
        /// Sets the Domain Name System (DNS) host name or IP address.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetHost(Evaluate<string> host, Evaluate<bool> canProceed = null)
        {
            this._urlBuilder.SetHost(host, canProceed);
            return this;
        }

        /// <summary>
        /// Removes the host.
        /// </summary>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient RemoveHost(Evaluate<bool> canProceed = null)
        {
            this._urlBuilder.RemoveHost(canProceed);
            return this;
        }

        /// <summary>
        /// Sets the paths of the UrlBuilder.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetPath(params string[] paths)
        {
            this._urlBuilder.SetPath(paths);
            return this;
        }

        /// <summary>
        /// Sets the paths of the UrlBuilder.
        /// </summary>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <param name="paths">The paths.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetPath(Evaluate<bool> canProceed, params Evaluate<string>[] paths)
        {
            this._urlBuilder.SetPath(canProceed, paths);
            return this;
        }

        /// <summary>
        /// Appends the paths of the UrlBuilder.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient AppendPath(params string[] paths)
        {
            this._urlBuilder.AppendPath(paths);
            return this;
        }

        /// <summary>
        /// Appends the paths of the UrlBuilder.
        /// </summary>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <param name="paths">The paths.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient AppendPath(Evaluate<bool> canProceed, params Evaluate<string>[] paths)
        {
            this._urlBuilder.AppendPath(canProceed, paths);
            return this;
        }

        /// <summary>
        /// Inserts the paths of the UrlBuilder at first index.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient InsertPath(params string[] paths)
        {
            this._urlBuilder.InsertPath(paths);
            return this;
        }

        /// <summary>
        /// Inserts the paths of the UrlBuilder at first index.
        /// </summary>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <param name="paths">The paths.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient InsertPath(Evaluate<bool> canProceed, params Evaluate<string>[] paths)
        {
            this._urlBuilder.InsertPath(canProceed, paths);
            return this;
        }

        /// <summary>
        /// Removes all path.
        /// </summary>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient RemovePath(Evaluate<bool> canProceed = null)
        {
            this._urlBuilder.RemovePath(canProceed);
            return this;
        }

        /// <summary>
        /// Removes the first occurrence of path segments.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient RemoveFirstPath(params string[] paths)
        {
            this._urlBuilder.RemoveFirstPath(paths);
            return this;
        }

        /// <summary>
        /// Removes the first occurrence of path segments.
        /// </summary>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <param name="paths">The paths.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient RemoveFirstPath(Evaluate<bool> canProceed, params Evaluate<string>[] paths)
        {
            this._urlBuilder.RemoveFirstPath(canProceed, paths);
            return this;
        }

        /// <summary>
        /// Removes the last occurrence of path segments.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient RemoveLastPath(params string[] paths)
        {
            this._urlBuilder.RemoveLastPath(paths);
            return this;
        }

        /// <summary>
        /// Removes the last occurrence of path segments.
        /// </summary>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <param name="paths">The paths.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient RemoveLastPath(Evaluate<bool> canProceed, params Evaluate<string>[] paths)
        {
            this._urlBuilder.RemoveLastPath(canProceed, paths);
            return this;
        }

        /// <summary>
        /// Appends the query.
        /// </summary>
        /// <param name="name">The query name.</param>
        /// <param name="values">The query values.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        /// <exception cref="ArgumentException">values cannot be null or empty. - values</exception>
        public HttpWebClient AppendQuery(string name, params object[] values)
        {
            this._urlBuilder.AppendQuery(name, values);
            return this;
        }

        /// <summary>
        /// Appends the query.
        /// </summary>
        /// <param name="name">The query name.</param>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <param name="values">The query values.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        /// <exception cref="ArgumentException">values cannot be null or empty. - values</exception>
        public HttpWebClient AppendQuery(string name, Evaluate<bool> canProceed, params Evaluate<object>[] values)
        {
            this._urlBuilder.AppendQuery(name, canProceed, values);
            return this;
        }

        /// <summary>
        /// Appends the query.
        /// </summary>
        /// <param name="values">The query key value pairs.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient AppendQuery(params KeyValuePair<string, object>[] values)
        {
            this._urlBuilder.AppendQuery(values);
            return this;
        }

        /// <summary>
        /// Appends the query.
        /// </summary>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <param name="values">The query key value pairs.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient AppendQuery(Evaluate<bool> canProceed, params KeyValuePair<string, Evaluate<object>>[] values)
        {
            this._urlBuilder.AppendQuery(canProceed, values);
            return this;
        }

        /// <summary>
        /// Appends the query.
        /// </summary>
        /// <param name="values">The query key value pairs.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient AppendQuery(params KeyValuePair<string, object[]>[] values)
        {
            this._urlBuilder.AppendQuery(values);
            return this;
        }

        /// <summary>
        /// Appends the query.
        /// </summary>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <param name="values">The query key value pairs.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient AppendQuery(Evaluate<bool> canProceed, params KeyValuePair<string, Evaluate<object>[]>[] values)
        {
            this._urlBuilder.AppendQuery(canProceed, values);
            return this;
        }

        /// <summary>
        /// Appends the query string.
        /// </summary>
        /// <param name="queryStrings">The query strings.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient AppendQueryString(params string[] queryStrings)
        {
            this._urlBuilder.AppendQueryString(queryStrings);
            return this;
        }

        /// <summary>
        /// Appends the query string.
        /// </summary>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <param name="queryStrings">The query strings.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient AppendQueryString(Evaluate<bool> canProceed, params Evaluate<string>[] queryStrings)
        {
            this._urlBuilder.AppendQueryString(canProceed, queryStrings);
            return this;
        }

        /// <summary>
        /// Sets a parameter to the query string, overwriting the value if name exists.
        /// </summary>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <param name="name">The name of query string parameter.</param>
        /// <param name="values">The query values.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetQuery(bool ignoreCase, string name, params object[] values)
        {
            this._urlBuilder.SetQuery(ignoreCase, name, values);
            return this;
        }

        /// <summary>
        /// Sets a parameter to the query string, overwriting the value if name exists.
        /// </summary>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <param name="name">The name of query string parameter.</param>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <param name="values">The query values.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetQuery(bool ignoreCase, string name, Evaluate<bool> canProceed, params Evaluate<object>[] values)
        {
            this._urlBuilder.SetQuery(ignoreCase, name, canProceed, values);
            return this;
        }

        /// <summary>
        /// Sets the query, overwriting the value if name exists.
        /// </summary>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <param name="values">The query key value pairs.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetQuery(bool ignoreCase, params KeyValuePair<string, object>[] values)
        {
            this._urlBuilder.SetQuery(ignoreCase, values);
            return this;
        }

        /// <summary>
        /// Sets the query, overwriting the value if name exists.
        /// </summary>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <param name="values">The query key value pairs.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetQuery(bool ignoreCase, Evaluate<bool> canProceed, params KeyValuePair<string, Evaluate<object>>[] values)
        {
            this._urlBuilder.SetQuery(ignoreCase, canProceed, values);
            return this;
        }

        /// <summary>
        /// Sets the query, overwriting the value if name exists.
        /// </summary>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <param name="values">The query key value pairs.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetQuery(bool ignoreCase, params KeyValuePair<string, object[]>[] values)
        {
            this._urlBuilder.SetQuery(ignoreCase, values);
            return this;
        }

        /// <summary>
        /// Sets the query, overwriting the value if name exists.
        /// </summary>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <param name="values">The query key value pairs.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetQuery(bool ignoreCase, Evaluate<bool> canProceed, params KeyValuePair<string, Evaluate<object>[]>[] values)
        {
            this._urlBuilder.SetQuery(ignoreCase, canProceed, values);
            return this;
        }

        /// <summary>
        /// Sets the query string, overwriting all values.
        /// </summary>
        /// <param name="queryStrings">The query strings.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetQueryString(params string[] queryStrings)
        {
            this._urlBuilder.SetQueryString(queryStrings);
            return this;
        }

        /// <summary>
        /// Sets the query string, overwriting all values.
        /// </summary>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <param name="queryStrings">The query strings.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetQueryString(Evaluate<bool> canProceed, params Evaluate<string>[] queryStrings)
        {
            this._urlBuilder.SetQueryString(canProceed, queryStrings);
            return this;
        }

        /// <summary>
        /// Removes the query.
        /// </summary>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <param name="names">The query names.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient RemoveQuery(bool ignoreCase, params string[] names)
        {
            this._urlBuilder.RemoveQuery(ignoreCase, names);
            return this;
        }

        /// <summary>
        /// Removes the query.
        /// </summary>
        /// <param name="ignoreCase">true to ignore case; otherwise, false.</param>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <param name="names">The query names.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient RemoveQuery(bool ignoreCase, Evaluate<bool> canProceed, params Evaluate<string>[] names)
        {
            this._urlBuilder.RemoveQuery(ignoreCase, canProceed, names);
            return this;
        }

        /// <summary>
        /// Removes all querys.
        /// </summary>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient RemoveQuery(Evaluate<bool> canProceed = null)
        {
            this._urlBuilder.RemoveQuery(canProceed);
            return this;
        }

        /// <summary>
        /// Set the URL fragment.
        /// </summary>
        /// <param name="fragment">The part of the URL after #.</param>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient SetFragment(Evaluate<string> fragment, Evaluate<bool> canProceed = null)
        {
            this._urlBuilder.SetFragment(fragment, canProceed);
            return this;
        }

        /// <summary>
        /// Removes the URL fragment including the #.
        /// </summary>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient RemoveFragment(Evaluate<bool> canProceed = null)
        {
            this._urlBuilder.RemoveFragment(canProceed);
            return this;
        }

        /// <summary>
        /// Resets to the root URL of the current UrlBuilder instance, will keep the scheme, any user info, host, and port (if specified).
        /// </summary>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient ResetToRoot(Evaluate<bool> canProceed = null)
        {
            this._urlBuilder.ResetToRoot(canProceed);
            return this;
        }

        /// <summary>
        /// Resets the current UrlBuilder instance to empty URL.
        /// </summary>
        /// <param name="canProceed">The delegate to check if can proceed or not.</param>
        /// <returns>The current HttpWebClient instance.</returns>
        public HttpWebClient Reset(Evaluate<bool> canProceed = null)
        {
            this._urlBuilder.Reset(canProceed);
            return this;
        }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <returns>WebResponseMessage instance.</returns>
        public WebResponseMessage SendRequest()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this._urlBuilder.ToString());

            request.Accept = this.Accept;
            request.ContentType = this.ContentType;
            request.Method = this.Method;
            request.UserAgent = this.UserAgent;

            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)request.GetResponse();

                WebResponseMessage result = new WebResponseMessage(response);

                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    result.Content = streamReader.ReadToEnd();
                    result.Succeeded = true;

                    return result;
                }
            }
            catch (Exception e)
            {
                WebException webException = e as WebException;

                if (webException != null)
                {
                    HttpWebResponse innerResponse = webException.Response as HttpWebResponse;

                    if (innerResponse != null)
                    {
                        WebResponseMessage result = new WebResponseMessage(innerResponse);

                        result.ErrorMessage = webException.ToString();
                        result.Succeeded = false;

                        StreamReader streamReader = null;

                        try
                        {
                            streamReader = new StreamReader(innerResponse.GetResponseStream());
                            result.Content = streamReader.ReadToEnd();
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

                        if (innerResponse != null)
                        {
                            innerResponse.Close();
                            innerResponse = null;
                        }

                        return result;
                    }
                    else
                    {
                        throw;
                    }
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
                    response.Close();
                    response = null;
                }
            }
        }
    }
}
