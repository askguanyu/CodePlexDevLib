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
    using System.Threading;

    /// <summary>
    /// Class FtpClient.
    /// </summary>
    public class FtpClient : MarshalByRefObject
    {
        /// <summary>
        /// Counter of the total bytes downloaded by FtpClient.
        /// </summary>
        private long _totalBytesDownloaded;

        /// <summary>
        /// Counter of the total bytes uploaded by FtpClient.
        /// </summary>
        private long _totalBytesUploaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpClient" /> class.
        /// </summary>
        public FtpClient()
        {
            this.FtpSetupInfo = new FtpSetup();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpClient" /> class.
        /// </summary>
        /// <param name="ftpSetup">Instance of FtpSetup.</param>
        public FtpClient(FtpSetup ftpSetup)
        {
            this.FtpSetupInfo = ftpSetup;
        }

        /// <summary>
        /// Gets total bytes downloaded by FtpClient.
        /// </summary>
        public long TotalBytesDownloaded
        {
            get
            {
                return Interlocked.Read(ref this._totalBytesDownloaded);
            }
        }

        /// <summary>
        /// Gets total bytes uploaded by FtpClient.
        /// </summary>
        public long TotalBytesUploaded
        {
            get
            {
                return Interlocked.Read(ref this._totalBytesUploaded);
            }
        }

        /// <summary>
        /// Gets or sets instance of FtpSetup.
        /// </summary>
        public FtpSetup FtpSetupInfo
        {
            get;
            set;
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
        /// <returns>List of FtpFileInfo, or null if path does not exist.</returns>
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
        /// <returns>List of FtpFileInfo, or null if path does not exist.</returns>
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
        /// <returns>List of FtpFileInfo, or null if path does not exist.</returns>
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
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool DownloadFile(string remoteFile, string localFile, bool overwrite = false, bool throwOnError = false)
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

            if (!overwrite && File.Exists(fullPath))
            {
                if (throwOnError)
                {
                    throw new ArgumentException("The specified file already exists.", fullPath);
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
                    Interlocked.Add(ref this._totalBytesDownloaded, count);
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
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool UploadFile(string localFile, string remoteFile, bool overwrite = false, bool throwOnError = false)
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
                    throw new FileNotFoundException("The specified file does not exist.", fullPath);
                }

                return false;
            }

            if (!overwrite)
            {
                string checkName = this.GetFtpWebResponseRawString(this.CreateFtpWebRequest(WebRequestMethods.Ftp.ListDirectory, string.IsNullOrEmpty(remoteFile) ? fileName : remoteFile));

                if (checkName != null)
                {
                    if (throwOnError)
                    {
                        throw new ArgumentException("The specified file already exists.", remoteFile);
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
                    Interlocked.Add(ref this._totalBytesUploaded, count);
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
                    throw new FileNotFoundException("The specified file does not exist.", fullPath);
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
        /// <param name="recursive">Make recursive directory.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool MakeDirectory(string fullDirectoryPath, bool recursive = true, bool throwOnError = false)
        {
            FtpWebResponse response = null;

            if (recursive)
            {
                if (string.IsNullOrEmpty(fullDirectoryPath) || fullDirectoryPath.Equals(Path.AltDirectorySeparatorChar))
                {
                    return true;
                }

                string input = fullDirectoryPath;
                this.MakeDirectory(input.Substring(0, input.LastIndexOf(Path.AltDirectorySeparatorChar)), true, throwOnError);
                return this.MakeDirectory(input, false, throwOnError);
            }
            else
            {
                try
                {
                    FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.MakeDirectory, fullDirectoryPath);
                    response = request.GetResponse() as FtpWebResponse;
                    this.UpdateFtpInfo(response);

                    return true;
                }
                catch (WebException e)
                {
                    ExceptionHandler.Log(e);

                    FtpWebResponse ftpWebResponse = e.Response as FtpWebResponse;

                    if (ftpWebResponse != null && !string.IsNullOrEmpty(ftpWebResponse.StatusDescription) && ftpWebResponse.StatusDescription.Contains("file already exists"))
                    {
                        return true;
                    }

                    if (throwOnError)
                    {
                        throw;
                    }

                    return false;
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
        /// Move a file on an FTP server.
        /// </summary>
        /// <param name="sourceFullPath">The source full path on an FTP server.</param>
        /// <param name="destinationFullPath">The destination full path on an FTP server.</param>
        /// <param name="overwrite">true if the destination file can be overwritten; otherwise, false.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool MoveFile(string sourceFullPath, string destinationFullPath, bool overwrite = false, bool throwOnError = false)
        {
            if (this.GetFileSize(destinationFullPath, false) >= 0)
            {
                if (overwrite)
                {
                    try
                    {
                        this.DeleteFile(sourceFullPath, true);
                    }
                    catch
                    {
                        if (throwOnError)
                        {
                            throw;
                        }

                        return false;
                    }
                }
                else
                {
                    if (throwOnError)
                    {
                        throw new ArgumentException("The specified file already exists.", destinationFullPath);
                    }

                    return false;
                }
            }

            if (this.GetDirectoryList(FtpFileInfo.GetDirectoryName(destinationFullPath)) == null)
            {
                this.MakeDirectory(FtpFileInfo.GetDirectoryName(destinationFullPath), true, false);
            }

            return this.Rename(sourceFullPath, destinationFullPath, throwOnError);
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
                uri = new Uri(string.Format("{0}{1}", this.FtpSetupInfo.HostName, FtpFileInfo.CombinePath("/", path)));
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
                throw;
            }

            FtpWebRequest result = FtpWebRequest.Create(uri) as FtpWebRequest;
            if (this.FtpSetupInfo.FtpCredential != null)
            {
                result.Credentials = this.FtpSetupInfo.FtpCredential;
            }

            result.Method = method;
            result.EnableSsl = this.FtpSetupInfo.EnableSSL;
            result.KeepAlive = this.FtpSetupInfo.KeepAlive;
            result.Proxy = this.FtpSetupInfo.Proxy;
            result.ReadWriteTimeout = this.FtpSetupInfo.ReadWriteTimeoutMilliseconds;
            result.UseBinary = this.FtpSetupInfo.UseBinary;
            result.UsePassive = this.FtpSetupInfo.UsePassive;

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
            if (string.IsNullOrEmpty(this.FtpSetupInfo.HostName))
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
