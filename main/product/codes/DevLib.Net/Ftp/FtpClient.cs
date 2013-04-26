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
                return this.GetFtpWebResponseRawString(this.CreateFtpWebRequest(WebRequestMethods.Ftp.PrintWorkingDirectory, "/", false), throwOnError) != null;
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
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
            return this.GetFtpWebResponseRawString(this.CreateFtpWebRequest(WebRequestMethods.Ftp.PrintWorkingDirectory, null, false), throwOnError);
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
                    ExceptionHandler.Log(e);

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
                    string rawString = this.GetFtpWebResponseRawString(request, true);
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
                    ExceptionHandler.Log(e);

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
                    ExceptionHandler.Log(e);

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
                    string rawString = this.GetFtpWebResponseRawString(request, true);
                    result = FtpFileParser.GetDirectoryList(rawString, request.RequestUri.LocalPath);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);

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
                    ExceptionHandler.Log(e);

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
                    string rawString = this.GetFtpWebResponseRawString(request, true);
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
                    ExceptionHandler.Log(e);

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

            try
            {
                if (!this.ExistsFile(remoteFile, throwOnError))
                {
                    if (throwOnError)
                    {
                        throw new ArgumentException("The specified file does not exist.", remoteFile);
                    }

                    return false;
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);

                if (throwOnError)
                {
                    throw;
                }
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
                        ExceptionHandler.Log(e);

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
                if (this.ExistsFile(remoteFile, false))
                {
                    if (throwOnError)
                    {
                        throw new ArgumentException("The specified file already exists.", remoteFile);
                    }

                    return false;
                }
            }

            if (!this.ExistsDirectory(FtpFileInfo.GetDirectoryName(remoteFile)))
            {
                this.MakeDirectory(FtpFileInfo.GetDirectoryName(remoteFile), throwOnError);
            }

            FtpWebRequest request = null;
            FtpWebResponse response = null;
            FileStream fileStream = null;
            Stream requestStream = null;

            try
            {
                request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.UploadFile, remoteFile, false);
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

            if (!this.ExistsDirectory(FtpFileInfo.GetDirectoryName(remoteFile)))
            {
                this.MakeDirectory(FtpFileInfo.GetDirectoryName(remoteFile), throwOnError);
            }

            FtpWebRequest request = null;
            FtpWebResponse response = null;
            FileStream fileStream = null;
            Stream requestStream = null;

            try
            {
                request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.AppendFile, remoteFile, false);
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
        /// Delete a file on an FTP server.
        /// </summary>
        /// <param name="remoteFile">The target file on an FTP server.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool DeleteFile(string remoteFile, bool throwOnError = false)
        {
            if (!this.ExistsFile(remoteFile))
            {
                if (throwOnError)
                {
                    throw new ArgumentException("The specified file does not exist.", remoteFile);
                }

                return true;
            }

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
            if (!this.ExistsFile(sourceRemoteFile))
            {
                if (throwOnError)
                {
                    throw new ArgumentException("The specified file does not exist.", sourceRemoteFile);
                }

                return false;
            }

            if (this.ExistsFile(destinationRemoteFile))
            {
                if (overwrite)
                {
                    try
                    {
                        this.DeleteFile(destinationRemoteFile, true);
                    }
                    catch
                    {
                        if (throwOnError)
                        {
                            throw new ArgumentException("Could not overwrite the specified file.", destinationRemoteFile);
                        }

                        return false;
                    }
                }
                else
                {
                    if (throwOnError)
                    {
                        throw new ArgumentException("The specified file already exists.", destinationRemoteFile);
                    }

                    return false;
                }
            }

            if (!this.ExistsDirectory(FtpFileInfo.GetDirectoryName(destinationRemoteFile)))
            {
                try
                {
                    this.MakeDirectory(FtpFileInfo.GetDirectoryName(destinationRemoteFile), true);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);

                    if (throwOnError)
                    {
                        throw;
                    }
                }
            }

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
                ExceptionHandler.Log(e);

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
                ExceptionHandler.Log(e);

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
            return this.MakeDirectoryHelper(remotePath, true, throwOnError);
        }

        /// <summary>
        /// Removes a directory on an FTP server.
        /// </summary>
        /// <param name="remotePath">The full path on an FTP server.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool RemoveDirectory(string remotePath, bool throwOnError = false)
        {
            return this.RemoveDirectoryHelper(remotePath, true, throwOnError);
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
            return this.MoveDirectoryHelper(sourceRemotePath, destinationRemotePath, true, overwrite, throwOnError);
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
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        private bool MakeDirectoryHelper(string remotePath, bool recursive, bool throwOnError)
        {
            if (this.ExistsDirectory(remotePath))
            {
                return true;
            }

            FtpWebResponse response = null;

            if (recursive)
            {
                try
                {
                    if (string.IsNullOrEmpty(remotePath) || remotePath.Equals(Path.AltDirectorySeparatorChar))
                    {
                        return true;
                    }

                    string input = remotePath;
                    this.MakeDirectoryHelper(input.Substring(0, input.LastIndexOf(Path.AltDirectorySeparatorChar)), true, true);
                    return this.MakeDirectoryHelper(input, false, true);
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
            else
            {
                try
                {
                    FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.MakeDirectory, remotePath, true);
                    response = request.GetResponse() as FtpWebResponse;
                    this.UpdateFtpInfo(response);

                    return true;
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);

                    WebException webException = e as WebException;

                    if (webException != null)
                    {
                        FtpWebResponse ftpWebResponse = webException.Response as FtpWebResponse;

                        if (ftpWebResponse != null)
                        {
                            if (ftpWebResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable && ftpWebResponse.StatusDescription.Contains("already exists"))
                            {
                                return true;
                            }
                        }
                    }

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
        }

        /// <summary>
        /// Removes a directory on an FTP server.
        /// </summary>
        /// <param name="remotePath">The full path on an FTP server.</param>
        /// <param name="recursive">true to delete directories, subdirectories, and files in <paramref name="remotePath" />; otherwise, false.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        private bool RemoveDirectoryHelper(string remotePath, bool recursive, bool throwOnError)
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
                            this.RemoveDirectoryHelper(item.FullPath, true, true);
                        }
                        else
                        {
                            this.DeleteFile(item.FullPath, true);
                        }
                    }

                    this.RemoveDirectoryHelper(remotePath, false, true);

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
            }
            else
            {
                if (!this.ExistsDirectory(remotePath))
                {
                    return true;
                }

                FtpWebResponse response = null;

                try
                {
                    FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.RemoveDirectory, remotePath, true);
                    response = request.GetResponse() as FtpWebResponse;
                    this.UpdateFtpInfo(response);

                    return true;
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);

                    WebException webException = e as WebException;

                    if (webException != null)
                    {
                        FtpWebResponse ftpWebResponse = webException.Response as FtpWebResponse;

                        if (ftpWebResponse != null)
                        {
                            if (ftpWebResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable && ftpWebResponse.StatusDescription.Contains("cannot find the file specified"))
                            {
                                return true;
                            }
                        }
                    }

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
        }

        /// <summary>
        /// Move a directory on an FTP server.
        /// </summary>
        /// <param name="sourceRemotePath">The source full path on an FTP server.</param>
        /// <param name="destinationRemotePath">The destination full path on an FTP server.</param>
        /// <param name="recursive">true to move directories, subdirectories, and files in <paramref name="remotePath" />; otherwise, false.</param>
        /// <param name="overwrite">true if the destination directory can be overwritten; otherwise, false.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        private bool MoveDirectoryHelper(string sourceRemotePath, string destinationRemotePath, bool recursive, bool overwrite, bool throwOnError)
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
                            this.MoveDirectoryHelper(item.FullPath, item.FullPath.Replace(sourceRemotePath, destinationRemotePath), true, overwrite, true);
                        }
                        else
                        {
                            this.MoveFile(item.FullPath, item.FullPath.Replace(sourceRemotePath, destinationRemotePath), overwrite, true);
                        }
                    }

                    this.MoveDirectoryHelper(sourceRemotePath, destinationRemotePath, false, overwrite, true);

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
            }
            else
            {
                if (!this.ExistsDirectory(sourceRemotePath))
                {
                    if (throwOnError)
                    {
                        throw new ArgumentException("The specified directory does not exist.", sourceRemotePath);
                    }

                    return false;
                }

                if (this.ExistsDirectory(destinationRemotePath))
                {
                    try
                    {
                        this.RemoveDirectory(sourceRemotePath, true);
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
                }

                if (!this.ExistsDirectory(FtpFileInfo.GetDirectoryName(destinationRemotePath)))
                {
                    try
                    {
                        this.MakeDirectory(FtpFileInfo.GetDirectoryName(destinationRemotePath), true);
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);

                        if (throwOnError)
                        {
                            throw;
                        }
                    }
                }

                FtpWebResponse response = null;

                try
                {
                    int upDirectoryConut = sourceRemotePath.Split(Path.AltDirectorySeparatorChar).Length + destinationRemotePath.Split(Path.AltDirectorySeparatorChar).Length + 1;
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Insert(0, "../", upDirectoryConut);
                    stringBuilder.Append("..");
                    stringBuilder.Append(FtpFileInfo.CombinePath(destinationRemotePath));

                    FtpWebRequest request = this.CreateFtpWebRequest(WebRequestMethods.Ftp.Rename, sourceRemotePath, false);
                    request.RenameTo = stringBuilder.ToString();
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
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>String from FtpWebResponse's stream.</returns>
        private string GetFtpWebResponseRawString(FtpWebRequest request, bool throwOnError)
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

                if (throwOnError)
                {
                    throw;
                }
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
