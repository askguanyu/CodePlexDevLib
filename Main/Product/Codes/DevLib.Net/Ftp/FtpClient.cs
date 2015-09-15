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
    using System.Text;
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
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Connect(bool throwOnError = false)
        {
            try
            {
                return this.GetFtpWebResponseRawString(this.CreateFtpWebRequest(WebRequestMethods.Ftp.PrintWorkingDirectory, "/", false)) != null;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return false;
            }
        }

        /// <summary>
        /// Prints the name of the current working directory.
        /// </summary>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>Current working directory string.</returns>
        public string PrintWorkingDirectory(bool throwOnError = false)
        {
            try
            {
                return this.GetFtpWebResponseRawString(this.CreateFtpWebRequest(WebRequestMethods.Ftp.PrintWorkingDirectory, null, false));
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a list of the files and folders on an FTP server.
        /// </summary>
        /// <param name="remotePath">The path on an FTP server.</param>
        /// <param name="recursive">true to get directories, subdirectories, and files in <paramref name="remotePath" />; otherwise, false.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>List of FtpFileInfo, or null if path does not exist.</returns>
        public List<FtpFileInfo> GetFullDirectoryList(string remotePath = null, bool recursive = true, bool throwOnError = false)
        {
            if (recursive)
            {
                List<FtpFileInfo> result = new List<FtpFileInfo>();

                try
                {
                    List<FtpFileInfo> input = this.GetFullDirectoryList(remotePath, false, true);

                    if (input == null || input.Count < 1)
                    {
                        return result;
                    }

                    result.AddRange(input);

                    foreach (FtpFileInfo item in input)
                    {
                        if (item.IsDirectory)
                        {
                            result.AddRange(this.GetFullDirectoryList(item.FullPath, true, true));
                        }
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);

                    if (throwOnError)
                    {
                        throw;
                    }
                }

                return result;
            }
            else
            {
                List<FtpFileInfo> result = new List<FtpFileInfo>();

                try
                {
                    FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.ListDirectoryDetails, remotePath, true);
                    string rawString = this.GetFtpWebResponseRawString(request);
                    result = FtpFileParser.GetFullDirectoryList(rawString, request.RequestUri.LocalPath);

                    foreach (FtpFileInfo item in result)
                    {
                        if (!item.IsDirectory)
                        {
                            try
                            {
                                item.LastModifiedTime = this.GetFileDateTimestamp(item.FullPath, true);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);

                    if (throwOnError)
                    {
                        throw;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets a list of the folders on an FTP server.
        /// </summary>
        /// <param name="remotePath">The path on an FTP server.</param>
        /// <param name="recursive">true to get directories and subdirectories in <paramref name="remotePath" />; otherwise, false.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>List of FtpFileInfo, or null if path does not exist.</returns>
        public List<FtpFileInfo> GetDirectoryList(string remotePath = null, bool recursive = true, bool throwOnError = false)
        {
            if (recursive)
            {
                List<FtpFileInfo> result = new List<FtpFileInfo>();

                try
                {
                    List<FtpFileInfo> input = this.GetDirectoryList(remotePath, false, true);

                    if (input == null || input.Count < 1)
                    {
                        return result;
                    }

                    result.AddRange(input);

                    foreach (FtpFileInfo item in input)
                    {
                        result.AddRange(this.GetDirectoryList(item.FullPath, true, true));
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);

                    if (throwOnError)
                    {
                        throw;
                    }
                }

                return result;
            }
            else
            {
                List<FtpFileInfo> result = new List<FtpFileInfo>();

                try
                {
                    FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.ListDirectoryDetails, remotePath, true);
                    string rawString = this.GetFtpWebResponseRawString(request);
                    result = FtpFileParser.GetDirectoryList(rawString, request.RequestUri.LocalPath);
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);

                    if (throwOnError)
                    {
                        throw;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets a list of the files on an FTP server.
        /// </summary>
        /// <param name="remotePath">The path on an FTP server.</param>
        /// <param name="recursive">true to get all files from directories and subdirectories in <paramref name="remotePath" />; otherwise, false.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>List of FtpFileInfo, or null if path does not exist.</returns>
        public List<FtpFileInfo> GetFileList(string remotePath = null, bool recursive = true, bool throwOnError = false)
        {
            if (recursive)
            {
                List<FtpFileInfo> result = new List<FtpFileInfo>();

                try
                {
                    result = this.GetFullDirectoryList(remotePath, true, true);
                    result.RemoveAll(delegate(FtpFileInfo ftpFileInfo) { return ftpFileInfo.IsDirectory; });
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);

                    if (throwOnError)
                    {
                        throw;
                    }
                }

                return result;
            }
            else
            {
                List<FtpFileInfo> result = new List<FtpFileInfo>();

                try
                {
                    FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.ListDirectoryDetails, remotePath, true);
                    string rawString = this.GetFtpWebResponseRawString(request);
                    result = FtpFileParser.GetFileList(rawString, request.RequestUri.LocalPath);

                    foreach (FtpFileInfo item in result)
                    {
                        try
                        {
                            item.LastModifiedTime = this.GetFileDateTimestamp(item.FullPath, true);
                        }
                        catch
                        {
                        }
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);

                    if (throwOnError)
                    {
                        throw;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Determines whether the specified file exists on ftp.
        /// </summary>
        /// <param name="remoteFile">The file to check.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if the specified file exists on ftp; otherwise, false.</returns>
        public bool ExistsFile(string remoteFile, bool throwOnError = false)
        {
            FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.GetFileSize, remoteFile, false);

            FtpWebResponse response = null;

            try
            {
                response = request.GetResponse() as FtpWebResponse;
                this.UpdateFtpInfo(response);

                return true;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

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
                    try
                    {
                        response.Close();
                    }
                    catch
                    {
                    }

                    response = null;
                }

                request = null;
            }
        }

        /// <summary>
        /// Retrieve the DateTime of a file on an FTP server.
        /// </summary>
        /// <param name="remoteFile">The full path on an FTP server.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>File last modified time if succeeded; otherwise, DateTime.MinValue.</returns>
        public DateTime GetFileDateTimestamp(string remoteFile, bool throwOnError = false)
        {
            FtpWebResponse response = null;

            try
            {
                FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.GetDateTimestamp, remoteFile, false);
                response = request.GetResponse() as FtpWebResponse;
                this.UpdateFtpInfo(response);

                return response.LastModified;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

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
                    try
                    {
                        response.Close();
                    }
                    catch
                    {
                    }

                    response = null;
                }
            }
        }

        /// <summary>
        /// Retrieve the size of a file on an FTP server.
        /// </summary>
        /// <param name="remoteFile">The full path on an FTP server.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>File size if succeeded; otherwise, -1.</returns>
        public long GetFileSize(string remoteFile, bool throwOnError = false)
        {
            FtpWebResponse response = null;

            try
            {
                FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.GetFileSize, remoteFile, false);
                response = request.GetResponse() as FtpWebResponse;
                this.UpdateFtpInfo(response);

                return response.ContentLength;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

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
                    try
                    {
                        response.Close();
                    }
                    catch
                    {
                    }

                    response = null;
                }
            }
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

            FtpWebResponse response = null;
            FileStream fileStream = null;
            Stream responseStream = null;

            try
            {
                FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.DownloadFile, remoteFile, false);
                response = request.GetResponse() as FtpWebResponse;
                this.UpdateFtpInfo(response);
                responseStream = response.GetResponseStream();

                if (!Directory.Exists(fullDirectoryPath))
                {
                    try
                    {
                        Directory.CreateDirectory(fullDirectoryPath);
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);

                        if (throwOnError)
                        {
                            throw;
                        }

                        return false;
                    }
                }

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
                InternalLogger.Log(e);

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

                if (response != null)
                {
                    if (response != null)
                    {
                        try
                        {
                            response.Close();
                        }
                        catch
                        {
                        }

                        response = null;
                    }
                }

                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream = null;
                }
            }
        }

        /// <summary>
        /// Download a file from an FTP server.
        /// </summary>
        /// <param name="remoteFile">The source file on an FTP server.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public byte[] DownloadFile(string remoteFile, bool throwOnError = false)
        {
            FtpWebResponse response = null;
            Stream responseStream = null;

            try
            {
                FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.DownloadFile, remoteFile, false);
                response = request.GetResponse() as FtpWebResponse;
                this.UpdateFtpInfo(response);
                responseStream = response.GetResponseStream();

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    byte[] buffer = new byte[81920];

                    int count;

                    while ((count = responseStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        memoryStream.Write(buffer, 0, count);
                    }

                    return memoryStream.ToArray();
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return null;
            }
            finally
            {
                if (responseStream != null)
                {
                    responseStream.Close();
                    responseStream = null;
                }

                if (response != null)
                {
                    if (response != null)
                    {
                        try
                        {
                            response.Close();
                        }
                        catch
                        {
                        }

                        response = null;
                    }
                }
            }
        }

        /// <summary>
        /// Download a file from an FTP server.
        /// </summary>
        /// <param name="remoteFile">The source file on an FTP server.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public Stream DownloadFileStream(string remoteFile, bool throwOnError = false)
        {
            FtpWebResponse response = null;
            Stream responseStream = null;

            try
            {
                FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.DownloadFile, remoteFile, false);
                response = request.GetResponse() as FtpWebResponse;
                this.UpdateFtpInfo(response);
                responseStream = response.GetResponseStream();
                return responseStream;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return null;
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

            if (string.IsNullOrEmpty(remoteFile))
            {
                if (throwOnError)
                {
                    throw new ArgumentNullException("remoteFile");
                }

                return false;
            }

            string localFullPath = Path.GetFullPath(localFile);

            if (!File.Exists(localFullPath))
            {
                if (throwOnError)
                {
                    throw new FileNotFoundException("The specified file does not exist.", localFullPath);
                }

                return false;
            }

            try
            {
                this.UploadFileHelper(localFullPath, remoteFile, false);

                return true;
            }
            catch (Exception e)
            {
                WebException webException = e as WebException;

                if (webException != null)
                {
                    FtpWebResponse ftpWebResponse = webException.Response as FtpWebResponse;

                    if (ftpWebResponse != null)
                    {
                        if (ftpWebResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            if (ftpWebResponse.StatusDescription.Contains("cannot find the path specified"))
                            {
                                this.MakeDirectory(FtpFileInfo.GetDirectoryName(remoteFile), false);

                                try
                                {
                                    this.UploadFileHelper(localFullPath, remoteFile, false);

                                    return true;
                                }
                                catch
                                {
                                    InternalLogger.Log(e);

                                    if (throwOnError)
                                    {
                                        throw;
                                    }

                                    return false;
                                }
                            }
                            else if (ftpWebResponse.StatusDescription.Contains("file already exists") && overwrite)
                            {
                                this.DeleteFile(remoteFile, false);

                                try
                                {
                                    this.UploadFileHelper(localFullPath, remoteFile, false);

                                    return true;
                                }
                                catch
                                {
                                    InternalLogger.Log(e);

                                    if (throwOnError)
                                    {
                                        throw;
                                    }

                                    return false;
                                }
                            }
                        }
                    }
                }

                InternalLogger.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return false;
            }
        }

        /// <summary>
        /// Uploads a file to an FTP server.
        /// </summary>
        /// <param name="stream">The source stream to upload.</param>
        /// <param name="remoteFile">The destination file on an FTP server.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool UploadFile(Stream stream, string remoteFile, bool overwrite = false, bool throwOnError = false)
        {
            if (stream == null)
            {
                if (throwOnError)
                {
                    throw new ArgumentNullException("stream");
                }

                return false;
            }

            if (string.IsNullOrEmpty(remoteFile))
            {
                if (throwOnError)
                {
                    throw new ArgumentNullException("remoteFile");
                }

                return false;
            }

            try
            {
                this.UploadFileHelper(stream, remoteFile, false);

                return true;
            }
            catch (Exception e)
            {
                WebException webException = e as WebException;

                if (webException != null)
                {
                    FtpWebResponse ftpWebResponse = webException.Response as FtpWebResponse;

                    if (ftpWebResponse != null)
                    {
                        if (ftpWebResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            if (ftpWebResponse.StatusDescription.Contains("cannot find the path specified"))
                            {
                                this.MakeDirectory(FtpFileInfo.GetDirectoryName(remoteFile), false);

                                try
                                {
                                    this.UploadFileHelper(stream, remoteFile, false);

                                    return true;
                                }
                                catch
                                {
                                    InternalLogger.Log(e);

                                    if (throwOnError)
                                    {
                                        throw;
                                    }

                                    return false;
                                }
                            }
                            else if (ftpWebResponse.StatusDescription.Contains("file already exists") && overwrite)
                            {
                                this.DeleteFile(remoteFile, false);

                                try
                                {
                                    this.UploadFileHelper(stream, remoteFile, false);

                                    return true;
                                }
                                catch
                                {
                                    InternalLogger.Log(e);

                                    if (throwOnError)
                                    {
                                        throw;
                                    }

                                    return false;
                                }
                            }
                        }
                    }
                }

                InternalLogger.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return false;
            }
        }

        /// <summary>
        /// Uploads a file to an FTP server.
        /// </summary>
        /// <param name="bytes">The source bytes to upload.</param>
        /// <param name="remoteFile">The destination file on an FTP server.</param>
        /// <param name="overwrite">Whether overwrite exists file.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool UploadFile(byte[] bytes, string remoteFile, bool overwrite = false, bool throwOnError = false)
        {
            if (bytes == null)
            {
                if (throwOnError)
                {
                    throw new ArgumentNullException("bytes");
                }

                return false;
            }

            if (string.IsNullOrEmpty(remoteFile))
            {
                if (throwOnError)
                {
                    throw new ArgumentNullException("remoteFile");
                }

                return false;
            }

            try
            {
                this.UploadFileHelper(bytes, remoteFile, false);

                return true;
            }
            catch (Exception e)
            {
                WebException webException = e as WebException;

                if (webException != null)
                {
                    FtpWebResponse ftpWebResponse = webException.Response as FtpWebResponse;

                    if (ftpWebResponse != null)
                    {
                        if (ftpWebResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            if (ftpWebResponse.StatusDescription.Contains("cannot find the path specified"))
                            {
                                this.MakeDirectory(FtpFileInfo.GetDirectoryName(remoteFile), false);

                                try
                                {
                                    this.UploadFileHelper(bytes, remoteFile, false);

                                    return true;
                                }
                                catch
                                {
                                    InternalLogger.Log(e);

                                    if (throwOnError)
                                    {
                                        throw;
                                    }

                                    return false;
                                }
                            }
                            else if (ftpWebResponse.StatusDescription.Contains("file already exists") && overwrite)
                            {
                                this.DeleteFile(remoteFile, false);

                                try
                                {
                                    this.UploadFileHelper(bytes, remoteFile, false);

                                    return true;
                                }
                                catch
                                {
                                    InternalLogger.Log(e);

                                    if (throwOnError)
                                    {
                                        throw;
                                    }

                                    return false;
                                }
                            }
                        }
                    }
                }

                InternalLogger.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return false;
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

            if (string.IsNullOrEmpty(remoteFile))
            {
                if (throwOnError)
                {
                    throw new ArgumentNullException("remoteFile");
                }

                return false;
            }

            string localFullPath = Path.GetFullPath(localFile);

            if (!File.Exists(localFullPath))
            {
                if (throwOnError)
                {
                    throw new FileNotFoundException("The specified file does not exist.", localFullPath);
                }

                return false;
            }

            try
            {
                this.UploadFileHelper(localFullPath, remoteFile, true);

                return true;
            }
            catch (Exception e)
            {
                WebException webException = e as WebException;

                if (webException != null)
                {
                    FtpWebResponse ftpWebResponse = webException.Response as FtpWebResponse;

                    if (ftpWebResponse != null)
                    {
                        if (ftpWebResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            if (ftpWebResponse.StatusDescription.Contains("cannot find the path specified"))
                            {
                                this.MakeDirectory(FtpFileInfo.GetDirectoryName(remoteFile), false);

                                try
                                {
                                    this.UploadFileHelper(localFullPath, remoteFile, true);

                                    return true;
                                }
                                catch
                                {
                                    InternalLogger.Log(e);

                                    if (throwOnError)
                                    {
                                        throw;
                                    }

                                    return false;
                                }
                            }
                        }
                    }
                }

                InternalLogger.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return false;
            }
        }

        /// <summary>
        /// Append a file to an existing file on an FTP server.
        /// </summary>
        /// <param name="stream">The source stream to append.</param>
        /// <param name="remoteFile">The destination file on an FTP server.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool AppendFile(Stream stream, string remoteFile, bool throwOnError = false)
        {
            if (stream == null)
            {
                if (throwOnError)
                {
                    throw new ArgumentNullException("stream");
                }

                return false;
            }

            if (string.IsNullOrEmpty(remoteFile))
            {
                if (throwOnError)
                {
                    throw new ArgumentNullException("remoteFile");
                }

                return false;
            }

            try
            {
                this.UploadFileHelper(stream, remoteFile, true);

                return true;
            }
            catch (Exception e)
            {
                WebException webException = e as WebException;

                if (webException != null)
                {
                    FtpWebResponse ftpWebResponse = webException.Response as FtpWebResponse;

                    if (ftpWebResponse != null)
                    {
                        if (ftpWebResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            if (ftpWebResponse.StatusDescription.Contains("cannot find the path specified"))
                            {
                                this.MakeDirectory(FtpFileInfo.GetDirectoryName(remoteFile), false);

                                try
                                {
                                    this.UploadFileHelper(stream, remoteFile, true);

                                    return true;
                                }
                                catch
                                {
                                    InternalLogger.Log(e);

                                    if (throwOnError)
                                    {
                                        throw;
                                    }

                                    return false;
                                }
                            }
                        }
                    }
                }

                InternalLogger.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return false;
            }
        }

        /// <summary>
        /// Append a file to an existing file on an FTP server.
        /// </summary>
        /// <param name="bytes">The source bytes to append.</param>
        /// <param name="remoteFile">The destination file on an FTP server.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool AppendFile(byte[] bytes, string remoteFile, bool throwOnError = false)
        {
            if (bytes == null)
            {
                if (throwOnError)
                {
                    throw new ArgumentNullException("bytes");
                }

                return false;
            }

            if (string.IsNullOrEmpty(remoteFile))
            {
                if (throwOnError)
                {
                    throw new ArgumentNullException("remoteFile");
                }

                return false;
            }

            try
            {
                this.UploadFileHelper(bytes, remoteFile, true);

                return true;
            }
            catch (Exception e)
            {
                WebException webException = e as WebException;

                if (webException != null)
                {
                    FtpWebResponse ftpWebResponse = webException.Response as FtpWebResponse;

                    if (ftpWebResponse != null)
                    {
                        if (ftpWebResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            if (ftpWebResponse.StatusDescription.Contains("cannot find the path specified"))
                            {
                                this.MakeDirectory(FtpFileInfo.GetDirectoryName(remoteFile), false);

                                try
                                {
                                    this.UploadFileHelper(bytes, remoteFile, true);

                                    return true;
                                }
                                catch
                                {
                                    InternalLogger.Log(e);

                                    if (throwOnError)
                                    {
                                        throw;
                                    }

                                    return false;
                                }
                            }
                        }
                    }
                }

                InternalLogger.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return false;
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
                FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.DeleteFile, remoteFile, false);
                response = request.GetResponse() as FtpWebResponse;
                this.UpdateFtpInfo(response);

                return true;
            }
            catch (Exception e)
            {
                WebException webException = e as WebException;

                if (webException != null)
                {
                    FtpWebResponse ftpWebResponse = webException.Response as FtpWebResponse;

                    if (ftpWebResponse != null)
                    {
                        if (ftpWebResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            if (ftpWebResponse.StatusDescription.Contains("cannot find the file specified"))
                            {
                                return true;
                            }
                        }
                    }
                }

                InternalLogger.Log(e);

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
                    try
                    {
                        response.Close();
                    }
                    catch
                    {
                    }

                    response = null;
                }
            }
        }

        /// <summary>
        /// Move a file on an FTP server.
        /// </summary>
        /// <param name="sourceRemoteFile">The source full path on an FTP server.</param>
        /// <param name="destinationRemoteFile">The destination full path on an FTP server.</param>
        /// <param name="overwrite">true if the destination file can be overwritten; otherwise, false.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool MoveFile(string sourceRemoteFile, string destinationRemoteFile, bool overwrite = false, bool throwOnError = false)
        {
            try
            {
                this.MoveHelper(sourceRemoteFile, destinationRemoteFile);

                return true;
            }
            catch (Exception e)
            {
                WebException webException = e as WebException;

                if (webException != null)
                {
                    FtpWebResponse ftpWebResponse = webException.Response as FtpWebResponse;

                    if (ftpWebResponse != null)
                    {
                        if (ftpWebResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            if (ftpWebResponse.StatusDescription.Contains("cannot find the path specified"))
                            {
                                this.MakeDirectory(FtpFileInfo.GetDirectoryName(destinationRemoteFile), false);

                                try
                                {
                                    this.MoveHelper(sourceRemoteFile, destinationRemoteFile);

                                    return true;
                                }
                                catch
                                {
                                    InternalLogger.Log(e);

                                    if (throwOnError)
                                    {
                                        throw;
                                    }

                                    return false;
                                }
                            }
                            else if (ftpWebResponse.StatusDescription.Contains("file already exists") && overwrite)
                            {
                                this.DeleteFile(destinationRemoteFile, false);

                                try
                                {
                                    this.MoveHelper(sourceRemoteFile, destinationRemoteFile);

                                    return true;
                                }
                                catch
                                {
                                    InternalLogger.Log(e);

                                    if (throwOnError)
                                    {
                                        throw;
                                    }

                                    return false;
                                }
                            }
                        }
                    }
                }

                InternalLogger.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return false;
            }
        }

        /// <summary>
        /// Determines whether the given path refers to an existing directory on ftp.
        /// </summary>
        /// <param name="remotePath">The path to test.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if path refers to an existing directory on ftp; otherwise, false.</returns>
        public bool ExistsDirectory(string remotePath, bool throwOnError = false)
        {
            FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.ListDirectory, remotePath, true);

            FtpWebResponse response = null;

            try
            {
                response = request.GetResponse() as FtpWebResponse;
                this.UpdateFtpInfo(response);

                return true;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

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
                    try
                    {
                        response.Close();
                    }
                    catch
                    {
                    }

                    response = null;
                }

                request = null;
            }
        }

        /// <summary>
        /// Retrieve the DateTime of a Directory on an FTP server.
        /// </summary>
        /// <param name="remotePath">The full path on an FTP server.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>Directory last modified time if succeeded; otherwise, DateTime.MinValue.</returns>
        public DateTime GetDirectoryDateTimestamp(string remotePath, bool throwOnError = false)
        {
            try
            {
                List<FtpFileInfo> input = this.GetDirectoryList(FtpFileInfo.GetDirectoryName(remotePath), false, true);

                foreach (FtpFileInfo item in input)
                {
                    if (remotePath.Equals(item.FullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        return item.LastModifiedTime;
                    }
                }

                return DateTime.MinValue;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Retrieve the size of a directory on an FTP server.
        /// </summary>
        /// <param name="remotePath">The full path on an FTP server.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>Directory recursive size if succeeded; otherwise, -1.</returns>
        public long GetDirectorySize(string remotePath, bool throwOnError = false)
        {
            try
            {
                long result = 0;

                List<FtpFileInfo> input = this.GetFullDirectoryList(remotePath, true, true);

                foreach (FtpFileInfo item in input)
                {
                    if (!item.IsDirectory)
                    {
                        result += item.Size;
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                if (throwOnError)
                {
                    throw;
                }
            }

            return -1;
        }

        /// <summary>
        /// Creates a directory on an FTP server.
        /// </summary>
        /// <param name="remotePath">The full path on an FTP server.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool MakeDirectory(string remotePath, bool throwOnError = false)
        {
            try
            {
                this.MakeDirectoryHelper(remotePath, true);

                return true;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return false;
            }
        }

        /// <summary>
        /// Removes a directory on an FTP server.
        /// </summary>
        /// <param name="remotePath">The full path on an FTP server.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool RemoveDirectory(string remotePath, bool throwOnError = false)
        {
            try
            {
                this.RemoveDirectoryHelper(remotePath, true);

                return true;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return false;
            }
        }

        /// <summary>
        /// Move a directory on an FTP server.
        /// </summary>
        /// <param name="sourceRemotePath">The source full path on an FTP server.</param>
        /// <param name="destinationRemotePath">The destination full path on an FTP server.</param>
        /// <param name="overwrite">true if the destination directory can be overwritten; otherwise, false.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool MoveDirectory(string sourceRemotePath, string destinationRemotePath, bool overwrite = true, bool throwOnError = false)
        {
            try
            {
                this.MoveDirectoryHelper(sourceRemotePath, destinationRemotePath, true, overwrite);

                return true;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                if (throwOnError)
                {
                    throw;
                }

                return false;
            }
        }

        /// <summary>
        /// Renames a directory or a file on an FTP server.
        /// </summary>
        /// <param name="remotePath">The old full path on an FTP server.</param>
        /// <param name="newName">The new name of the file being renamed.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Rename(string remotePath, string newName, bool throwOnError = false)
        {
            FtpWebResponse response = null;

            try
            {
                FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.Rename, remotePath, false);
                request.RenameTo = newName;
                response = request.GetResponse() as FtpWebResponse;
                this.UpdateFtpInfo(response);

                return true;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

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
                    try
                    {
                        response.Close();
                    }
                    catch
                    {
                    }

                    response = null;
                }
            }
        }

        /// <summary>
        /// Uploads a file to an FTP server.
        /// </summary>
        /// <param name="localFile">The local source file to upload.</param>
        /// <param name="remoteFile">The destination file on an FTP server.</param>
        /// <param name="append">true to use append method; false to use upload method.</param>
        private void UploadFileHelper(string localFile, string remoteFile, bool append)
        {
            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(localFile, FileMode.Open, FileAccess.Read);
                this.UploadFileHelper(fileStream, remoteFile, append);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream = null;
                }
            }
        }

        /// <summary>
        /// Uploads a file to an FTP server.
        /// </summary>
        /// <param name="stream">The source stream to upload.</param>
        /// <param name="remoteFile">The destination file on an FTP server.</param>
        /// <param name="append">true to use append method; false to use upload method.</param>
        private void UploadFileHelper(Stream stream, string remoteFile, bool append)
        {
            FtpWebRequest request = null;
            FtpWebResponse response = null;
            Stream requestStream = null;

            try
            {
                request = this.CreateFtpWebRequest(append ? WebRequestMethods.Ftp.AppendFile : WebRequestMethods.Ftp.UploadFile, remoteFile, false);
                requestStream = request.GetRequestStream();

                stream.Position = 0;
                byte[] buffer = new byte[81920];
                int count;

                while ((count = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, count);
                    Interlocked.Add(ref this._totalBytesUploaded, count);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (requestStream != null)
                {
                    requestStream.Close();
                    requestStream = null;
                }

                try
                {
                    response = request.GetResponse() as FtpWebResponse;
                    this.UpdateFtpInfo(response);
                }
                catch
                {
                }
                finally
                {
                    if (response != null)
                    {
                        try
                        {
                            response.Close();
                        }
                        catch
                        {
                        }

                        response = null;
                    }
                }
            }
        }

        /// <summary>
        /// Uploads a file to an FTP server.
        /// </summary>
        /// <param name="bytes">The source bytes to upload.</param>
        /// <param name="remoteFile">The destination file on an FTP server.</param>
        /// <param name="append">true to use append method; false to use upload method.</param>
        private void UploadFileHelper(byte[] bytes, string remoteFile, bool append)
        {
            FtpWebRequest request = null;
            FtpWebResponse response = null;
            Stream requestStream = null;

            try
            {
                request = this.CreateFtpWebRequest(append ? WebRequestMethods.Ftp.AppendFile : WebRequestMethods.Ftp.UploadFile, remoteFile, false);
                requestStream = request.GetRequestStream();

                int count = bytes.Length;

                requestStream.Write(bytes, 0, count);
                Interlocked.Add(ref this._totalBytesUploaded, count);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (requestStream != null)
                {
                    requestStream.Close();
                    requestStream = null;
                }

                try
                {
                    response = request.GetResponse() as FtpWebResponse;
                    this.UpdateFtpInfo(response);
                }
                catch
                {
                }
                finally
                {
                    if (response != null)
                    {
                        try
                        {
                            response.Close();
                        }
                        catch
                        {
                        }

                        response = null;
                    }
                }
            }
        }

        /// <summary>
        /// Move a file on an FTP server.
        /// </summary>
        /// <param name="sourceRemoteFile">The source full path on an FTP server.</param>
        /// <param name="destinationRemoteFile">The destination full path on an FTP server.</param>
        private void MoveHelper(string sourceRemoteFile, string destinationRemoteFile)
        {
            FtpWebResponse response = null;

            try
            {
                int upDirectoryConut = sourceRemoteFile.Split(Path.AltDirectorySeparatorChar).Length + destinationRemoteFile.Split(Path.AltDirectorySeparatorChar).Length + 1;
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Insert(0, "../", upDirectoryConut);
                stringBuilder.Append("..");
                stringBuilder.Append(FtpFileInfo.CombinePath(destinationRemoteFile));

                FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.Rename, sourceRemoteFile, false);
                request.RenameTo = stringBuilder.ToString();
                response = request.GetResponse() as FtpWebResponse;
                this.UpdateFtpInfo(response);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (response != null)
                {
                    try
                    {
                        response.Close();
                    }
                    catch
                    {
                    }

                    response = null;
                }
            }
        }

        /// <summary>
        /// Creates a directory on an FTP server.
        /// </summary>
        /// <param name="remotePath">The full path on an FTP server.</param>
        /// <param name="recursive">true to make recursive directory; otherwise, false.</param>
        private void MakeDirectoryHelper(string remotePath, bool recursive)
        {
            FtpWebResponse response = null;

            if (recursive)
            {
                try
                {
                    if (string.IsNullOrEmpty(remotePath) || remotePath.Equals(Path.AltDirectorySeparatorChar))
                    {
                        return;
                    }

                    string input = remotePath;
                    this.MakeDirectoryHelper(input.Substring(0, input.LastIndexOf(Path.AltDirectorySeparatorChar)), true);
                    this.MakeDirectoryHelper(input, false);
                }
                catch
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.MakeDirectory, remotePath, true);
                    response = request.GetResponse() as FtpWebResponse;
                    this.UpdateFtpInfo(response);
                }
                catch (Exception e)
                {
                    WebException webException = e as WebException;

                    if (webException != null)
                    {
                        FtpWebResponse ftpWebResponse = webException.Response as FtpWebResponse;

                        if (ftpWebResponse != null)
                        {
                            if (ftpWebResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable && ftpWebResponse.StatusDescription.Contains("already exists"))
                            {
                                return;
                            }
                        }
                    }

                    throw;
                }
                finally
                {
                    if (response != null)
                    {
                        try
                        {
                            response.Close();
                        }
                        catch
                        {
                        }

                        response = null;
                    }
                }
            }
        }

        /// <summary>
        /// Removes a directory on an FTP server.
        /// </summary>
        /// <param name="remotePath">The full path on an FTP server.</param>
        /// <param name="recursive">true to delete directories, subdirectories, and files in <paramref name="remotePath" />; otherwise, false.</param>
        private void RemoveDirectoryHelper(string remotePath, bool recursive)
        {
            if (recursive)
            {
                try
                {
                    List<FtpFileInfo> input = this.GetFullDirectoryList(remotePath, false, true);

                    foreach (FtpFileInfo item in input)
                    {
                        if (item.IsDirectory)
                        {
                            this.RemoveDirectoryHelper(item.FullPath, true);
                        }
                        else
                        {
                            this.DeleteFile(item.FullPath, true);
                        }
                    }

                    this.RemoveDirectoryHelper(remotePath, false);
                }
                catch
                {
                    throw;
                }
            }
            else
            {
                FtpWebResponse response = null;

                try
                {
                    FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.RemoveDirectory, remotePath, true);
                    response = request.GetResponse() as FtpWebResponse;
                    this.UpdateFtpInfo(response);
                }
                catch (Exception e)
                {
                    WebException webException = e as WebException;

                    if (webException != null)
                    {
                        FtpWebResponse ftpWebResponse = webException.Response as FtpWebResponse;

                        if (ftpWebResponse != null)
                        {
                            if (ftpWebResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable && ftpWebResponse.StatusDescription.Contains("cannot find the file specified"))
                            {
                                return;
                            }
                        }
                    }

                    throw;
                }
                finally
                {
                    if (response != null)
                    {
                        try
                        {
                            response.Close();
                        }
                        catch
                        {
                        }

                        response = null;
                    }
                }
            }
        }

        /// <summary>
        /// Move a directory on an FTP server.
        /// </summary>
        /// <param name="sourceRemotePath">The source full path on an FTP server.</param>
        /// <param name="destinationRemotePath">The destination full path on an FTP server.</param>
        /// <param name="recursive">true to move directories, subdirectories, and files in <paramref name="sourceRemotePath" />; otherwise, false.</param>
        /// <param name="overwrite">true if the destination directory can be overwritten; otherwise, false.</param>
        private void MoveDirectoryHelper(string sourceRemotePath, string destinationRemotePath, bool recursive, bool overwrite)
        {
            if (recursive)
            {
                try
                {
                    List<FtpFileInfo> input = this.GetFullDirectoryList(sourceRemotePath, false, true);

                    foreach (FtpFileInfo item in input)
                    {
                        if (item.IsDirectory)
                        {
                            this.MoveDirectoryHelper(item.FullPath, item.FullPath.Replace(sourceRemotePath, destinationRemotePath), true, overwrite);
                        }
                        else
                        {
                            this.MoveFile(item.FullPath, item.FullPath.Replace(sourceRemotePath, destinationRemotePath), overwrite, true);
                        }
                    }

                    this.MoveDirectoryHelper(sourceRemotePath, destinationRemotePath, false, overwrite);
                }
                catch
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    this.MoveHelper(sourceRemotePath, destinationRemotePath);
                }
                catch (Exception e)
                {
                    WebException webException = e as WebException;

                    if (webException != null)
                    {
                        FtpWebResponse ftpWebResponse = webException.Response as FtpWebResponse;

                        if (ftpWebResponse != null)
                        {
                            if (ftpWebResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                            {
                                if (ftpWebResponse.StatusDescription.Contains("cannot find the path specified"))
                                {
                                    this.MakeDirectory(FtpFileInfo.GetDirectoryName(destinationRemotePath), false);

                                    try
                                    {
                                        this.MoveHelper(sourceRemotePath, destinationRemotePath);
                                        return;
                                    }
                                    catch
                                    {
                                        throw;
                                    }
                                }
                                else if (ftpWebResponse.StatusDescription.Contains("file already exists"))
                                {
                                    this.RemoveDirectory(sourceRemotePath, true);
                                    return;
                                }
                            }
                        }
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Method CreateFtpWebRequest.
        /// </summary>
        /// <param name="method">Ftp method.</param>
        /// <param name="remotePath">Ftp path.</param>
        /// <param name="isDirectoryOnly">Only treat the <paramref name="remotePath" /> as a directory path or not.</param>
        /// <returns>Instance of FtpWebRequest.</returns>
        private FtpWebRequest CreateFtpWebRequest(string method, string remotePath, bool isDirectoryOnly)
        {
            this.CheckHostName();

            Uri uri = null;

            try
            {
                if (isDirectoryOnly)
                {
                    uri = new Uri(string.Format("{0}{1}/", this.FtpSetupInfo.HostName, FtpFileInfo.CombinePath(remotePath).TrimEnd(Path.AltDirectorySeparatorChar)));
                }
                else
                {
                    uri = new Uri(string.Format("{0}{1}", this.FtpSetupInfo.HostName, FtpFileInfo.CombinePath(remotePath)));
                }
            }
            catch
            {
                throw;
            }

            FtpWebRequest result = FtpWebRequest.Create(uri) as FtpWebRequest;

            if (this.FtpSetupInfo.FtpCredential != null)
            {
                result.Credentials = this.FtpSetupInfo.FtpCredential;
            }

            result.ConnectionGroupName = this.FtpSetupInfo.GetHashCode().ToString();
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
            catch
            {
                throw;
            }
            finally
            {
                if (response != null)
                {
                    try
                    {
                        response.Close();
                    }
                    catch
                    {
                    }

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
