//-----------------------------------------------------------------------
// <copyright file="FtpClient.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.Ftp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    /// <summary>
    /// Ftp Client Class.
    /// </summary>
    public class FtpClient : MarshalByRefObject
    {
        /// <summary>
        /// Field _networkCredential.
        /// </summary>
        private NetworkCredential _networkCredential;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpClient" /> class.
        /// </summary>
        public FtpClient()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpClient" /> class.
        /// </summary>
        /// <param name="hostName">Ftp host name.</param>
        /// <param name="username">Ftp username.</param>
        /// <param name="password">Ftp password.</param>
        public FtpClient(string hostName, string username, string password)
        {
            this.HostName = hostName;

            if (this._networkCredential == null)
            {
                this._networkCredential = new NetworkCredential();
            }

            this._networkCredential.UserName = username;
            this._networkCredential.Password = password;
        }

        /// <summary>
        /// Gets or sets a value indicating whether using SSL.
        /// </summary>
        public bool EnableSSL
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets ftp host name.
        /// </summary>
        public string HostName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets ftp username.
        /// </summary>
        public string UserName
        {
            get
            {
                return this._networkCredential != null ? this._networkCredential.UserName : string.Empty;
            }

            set
            {
                if (this._networkCredential == null)
                {
                    this._networkCredential = new NetworkCredential();
                }

                this._networkCredential.UserName = value;
            }
        }

        /// <summary>
        /// Gets or sets ftp password
        /// </summary>
        public string Password
        {
            get
            {
                return this._networkCredential != null ? this._networkCredential.Password : string.Empty;
            }

            set
            {
                if (this._networkCredential == null)
                {
                    this._networkCredential = new NetworkCredential();
                }

                this._networkCredential.Password = value;
            }
        }

        /// <summary>
        /// Gets ftp welcome message.
        /// </summary>
        public string WelcomeMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets ftp banner message.
        /// </summary>
        public string BannerMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets ftp exit message.
        /// </summary>
        public string ExitMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets ftp status code.
        /// </summary>
        public FtpStatusCode StatusCode
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets ftp status description.
        /// </summary>
        public string StatusDescription
        {
            get;
            private set;
        }

        /// <summary>
        /// Try to connect to ftp.
        /// </summary>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Connect()
        {
            try
            {
                return this.GetFtpWebResponseRawString(this.CreateFtpWebRequest(WebRequestMethods.Ftp.PrintWorkingDirectory, "/")) != null;
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Prints the name of the current working directory.
        /// </summary>
        /// <returns>Current working directory string.</returns>
        public string PrintWorkingDirectory()
        {
            return this.GetFtpWebResponseRawString(this.CreateFtpWebRequest(WebRequestMethods.Ftp.PrintWorkingDirectory, null));
        }

        /// <summary>
        /// Gets a list of the files and folders on an FTP server.
        /// </summary>
        /// <param name="remotePath">The path on an FTP server.</param>
        /// <returns>List of FtpFileInfo.</returns>
        public List<FtpFileInfo> GetFullDirectoryList(string remotePath = null)
        {
            FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.ListDirectoryDetails, remotePath);
            string rawString = this.GetFtpWebResponseRawString(request);
            return FtpFileParser.GetFullDirectoryList(rawString, request.RequestUri.LocalPath);
        }

        /// <summary>
        /// Gets a list of the folders on an FTP server.
        /// </summary>
        /// <param name="remotePath">The path on an FTP server.</param>
        /// <returns>List of FtpFileInfo.</returns>
        public List<FtpFileInfo> GetDirectoryList(string remotePath = null)
        {
            FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.ListDirectoryDetails, remotePath);
            string rawString = this.GetFtpWebResponseRawString(request);
            return FtpFileParser.GetDirectoryList(rawString, request.RequestUri.LocalPath);
        }

        /// <summary>
        /// Gets a list of the files on an FTP server.
        /// </summary>
        /// <param name="remotePath">The path on an FTP server.</param>
        /// <returns>List of FtpFileInfo.</returns>
        public List<FtpFileInfo> GetFileList(string remotePath = null)
        {
            FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.ListDirectoryDetails, remotePath);
            string rawString = this.GetFtpWebResponseRawString(request);
            return FtpFileParser.GetFileList(rawString, request.RequestUri.LocalPath);
        }

        /// <summary>
        /// Download a file from an FTP server.
        /// </summary>
        /// <param name="remoteFile">The source file on an FTP server.</param>
        /// <param name="localFile">The local destination file.</param>
        /// <param name="overwritten">Whether overwrite exists file.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool DownloadFile(string remoteFile, string localFile, bool overwritten = true, bool throwOnError = false)
        {
            if (string.IsNullOrEmpty(localFile))
            {
                if (throwOnError)
                {
                    throw new ArgumentNullException("localFile");
                }

                return false;
            }

            string fullPath = Path.GetFullPath(localFile);
            string fullDirectoryPath = Path.GetDirectoryName(localFile);

            if (!overwritten && File.Exists(fullPath))
            {
                if (throwOnError)
                {
                    throw new ArgumentException("The file exists.", fullPath);
                }

                return false;
            }

            if (!Directory.Exists(fullDirectoryPath))
            {
                try
                {
                    Directory.CreateDirectory(fullDirectoryPath);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);

                    if (throwOnError)
                    {
                        throw;
                    }

                    return false;
                }
            }

            FtpWebResponse response = null;
            FileStream fileStream = null;
            Stream responseStream = null;

            try
            {
                FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.DownloadFile, remoteFile);
                response = request.GetResponse() as FtpWebResponse;
                this.UpdateFtpInfo(response);

                responseStream = response.GetResponseStream();
                fileStream = new FileStream(localFile, FileMode.Create, FileAccess.Write);

                byte[] buffer = new byte[81920];
                int count;
                while ((count = responseStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    fileStream.Write(buffer, 0, count);
                }

                return true;
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return false;
            }
            finally
            {
                if (responseStream != null)
                {
                    responseStream.Close();
                    responseStream = null;
                }

                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream = null;
                }

                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }

        /// <summary>
        /// Uploads a file to an FTP server.
        /// </summary>
        /// <param name="localFile">The local source file.</param>
        /// <param name="remoteFile">The destination file on an FTP server.</param>
        /// <param name="overwritten">Whether overwrite exists file.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool UploadFile(string localFile, string remoteFile, bool overwritten = true, bool throwOnError = false)
        {
            if (string.IsNullOrEmpty(localFile))
            {
                if (throwOnError)
                {
                    throw new ArgumentNullException("localFile");
                }

                return false;
            }

            string fullPath = Path.GetFullPath(localFile);
            string fileName = Path.GetFileName(localFile);

            if (!File.Exists(fullPath))
            {
                if (throwOnError)
                {
                    throw new FileNotFoundException("The file does not exist.", fullPath);
                }

                return false;
            }

            if (!overwritten)
            {
                string checkName = this.GetFtpWebResponseRawString(this.CreateFtpWebRequest(WebRequestMethods.Ftp.ListDirectory, string.IsNullOrEmpty(remoteFile) ? fileName : remoteFile));

                if (checkName != null)
                {
                    if (throwOnError)
                    {
                        throw new ArgumentException("The file exists.", remoteFile);
                    }

                    return false;
                }
            }

            FtpWebRequest request = null;
            FtpWebResponse response = null;
            FileStream fileStream = null;
            Stream requestStream = null;

            try
            {
                request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.UploadFile, string.IsNullOrEmpty(remoteFile) ? fileName : remoteFile);
                requestStream = request.GetRequestStream();
                fileStream = new FileStream(localFile, FileMode.Open, FileAccess.Read);

                byte[] buffer = new byte[81920];
                int count;
                while ((count = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, count);
                }

                return true;
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return false;
            }
            finally
            {
                if (requestStream != null)
                {
                    requestStream.Close();
                    requestStream = null;
                }

                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream = null;
                }

                try
                {
                    response = request.GetResponse() as FtpWebResponse;
                    this.UpdateFtpInfo(response);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
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

        /// <summary>
        /// Append a file to an existing file on an FTP server.
        /// </summary>
        /// <param name="localFile">The local source file.</param>
        /// <param name="remoteFile">The destination file on an FTP server.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool AppendFile(string localFile, string remoteFile, bool throwOnError = false)
        {
            if (string.IsNullOrEmpty(localFile))
            {
                if (throwOnError)
                {
                    throw new ArgumentNullException("localFile");
                }

                return false;
            }

            string fullPath = Path.GetFullPath(localFile);
            string fileName = Path.GetFileName(localFile);

            if (!File.Exists(fullPath))
            {
                if (throwOnError)
                {
                    throw new FileNotFoundException("The file does not exist.", fullPath);
                }

                return false;
            }

            FtpWebRequest request = null;
            FtpWebResponse response = null;
            FileStream fileStream = null;
            Stream requestStream = null;

            try
            {
                request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.AppendFile, string.IsNullOrEmpty(remoteFile) ? fileName : remoteFile);
                requestStream = request.GetRequestStream();
                fileStream = new FileStream(localFile, FileMode.Open, FileAccess.Read);

                byte[] buffer = new byte[81920];
                int count;
                while ((count = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, count);
                }

                return true;
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return false;
            }
            finally
            {
                if (requestStream != null)
                {
                    requestStream.Close();
                    requestStream = null;
                }

                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream = null;
                }

                try
                {
                    response = request.GetResponse() as FtpWebResponse;
                    this.UpdateFtpInfo(response);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
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

        /// <summary>
        /// Delete a file on an FTP server.
        /// </summary>
        /// <param name="remoteFile">The target file on an FTP server.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool DeleteFile(string remoteFile, bool throwOnError = false)
        {
            FtpWebResponse response = null;

            try
            {
                FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.DeleteFile, remoteFile);
                response = request.GetResponse() as FtpWebResponse;
                this.UpdateFtpInfo(response);

                return true;
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return false;
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

        /// <summary>
        /// Creates a directory on an FTP server.
        /// </summary>
        /// <param name="fullDirectoryPath">The full path on an FTP server.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool MakeDirectory(string fullDirectoryPath, bool throwOnError = false)
        {
            FtpWebResponse response = null;

            try
            {
                FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.MakeDirectory, fullDirectoryPath);
                response = request.GetResponse() as FtpWebResponse;
                this.UpdateFtpInfo(response);

                return true;
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return false;
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

        /// <summary>
        /// Removes a directory on an FTP server.
        /// </summary>
        /// <param name="fullDirectoryPath">The full path on an FTP server.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool RemoveDirectory(string fullDirectoryPath, bool throwOnError = false)
        {
            FtpWebResponse response = null;

            try
            {
                FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.RemoveDirectory, fullDirectoryPath);
                response = request.GetResponse() as FtpWebResponse;
                this.UpdateFtpInfo(response);

                return true;
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return false;
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

        /// <summary>
        /// Retrieve the DateTime of a file on an FTP server.
        /// </summary>
        /// <param name="fullDirectoryPath">The full path on an FTP server.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public DateTime GetDateTimestamp(string fullDirectoryPath, bool throwOnError = false)
        {
            FtpWebResponse response = null;

            try
            {
                FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.GetDateTimestamp, fullDirectoryPath);
                response = request.GetResponse() as FtpWebResponse;
                this.UpdateFtpInfo(response);

                return response.LastModified;
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return DateTime.MinValue;
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

        /// <summary>
        /// Retrieve the size of a file on an FTP server.
        /// </summary>
        /// <param name="fullDirectoryPath">The full path on an FTP server.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public long GetFileSize(string fullDirectoryPath, bool throwOnError = false)
        {
            FtpWebResponse response = null;

            try
            {
                FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.GetFileSize, fullDirectoryPath);
                response = request.GetResponse() as FtpWebResponse;
                this.UpdateFtpInfo(response);

                return response.ContentLength;
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return -1;
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

        /// <summary>
        /// Renames a directory or a file on an FTP server.
        /// </summary>
        /// <param name="fullDirectoryPath">The old full path on an FTP server.</param>
        /// <param name="newName">The new name of the file being renamed.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Rename(string fullDirectoryPath, string newName, bool throwOnError = false)
        {
            FtpWebResponse response = null;

            try
            {
                FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.Rename, fullDirectoryPath);
                request.RenameTo = newName;
                response = request.GetResponse() as FtpWebResponse;
                this.UpdateFtpInfo(response);

                return true;
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return false;
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

        /// <summary>
        /// Method CreateFtpWebRequest.
        /// </summary>
        /// <param name="method">Ftp method.</param>
        /// <param name="path">Ftp path.</param>
        /// <returns>Instance of FtpWebRequest.</returns>
        private FtpWebRequest CreateFtpWebRequest(string method, string path)
        {
            this.CheckHostName();

            Uri uri = null;

            try
            {
                uri = new Uri(string.Format("ftp://{0}/{1}", this.HostName, string.IsNullOrEmpty(path) ? string.Empty : path.Trim('/')));
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
                throw;
            }

            FtpWebRequest result = FtpWebRequest.Create(uri) as FtpWebRequest;
            result.Proxy = null;
            if (this._networkCredential != null)
            {
                result.Credentials = this._networkCredential;
            }

            result.EnableSsl = this.EnableSSL;
            result.Method = method;
            result.KeepAlive = true;
            result.UsePassive = true;
            return result;
        }

        /// <summary>
        /// Method GetFtpWebResponseRawString.
        /// </summary>
        /// <param name="request">Instance of FtpWebRequest.</param>
        /// <returns>String from FtpWebResponse's stream.</returns>
        private string GetFtpWebResponseRawString(FtpWebRequest request)
        {
            string result = null;

            FtpWebResponse response = null;

            try
            {
                response = request.GetResponse() as FtpWebResponse;

                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }

                this.UpdateFtpInfo(response);
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }

                request = null;
            }

            return result;
        }

        /// <summary>
        /// Method CheckHostName.
        /// </summary>
        private void CheckHostName()
        {
            if (string.IsNullOrEmpty(this.HostName))
            {
                throw new ArgumentNullException("HostName");
            }
        }

        /// <summary>
        /// Method UpdateFtpInfo.
        /// </summary>
        /// <param name="response">Instance of FtpWebResponse.</param>
        private void UpdateFtpInfo(FtpWebResponse response)
        {
            if (response != null)
            {
                this.BannerMessage = response.BannerMessage;
                this.WelcomeMessage = response.WelcomeMessage;
                this.ExitMessage = response.ExitMessage;
                this.StatusCode = response.StatusCode;
                this.StatusDescription = response.StatusDescription;
            }
        }
    }
}
