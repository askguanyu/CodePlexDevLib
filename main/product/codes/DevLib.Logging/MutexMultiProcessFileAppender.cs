//-----------------------------------------------------------------------
// <copyright file="MutexMultiProcessFileAppender.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Logging
{
    using System;
    using System.IO;
    using System.Security.AccessControl;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Class MutexMultiProcessFileAppender.
    /// </summary>
    internal class MutexMultiProcessFileAppender : IDisposable
    {
        /// <summary>
        /// Field MutexNamePrefix.
        /// </summary>
        private const string MutexNamePrefix = @"Global\DevLib.Logging/";

        /// <summary>
        /// Field MaxMutexNameLength.
        /// </summary>
        private const int MaxMutexNameLength = 260;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Field _mutex.
        /// </summary>
        private Mutex _mutex;

        /// <summary>
        /// Field _fileName.
        /// </summary>
        private string _fileName;

        /// <summary>
        /// Field _fileDirectory.
        /// </summary>
        private DirectoryInfo _fileDirectory;

        /// <summary>
        /// Field _fileInfo.
        /// </summary>
        private FileInfo _fileInfo;

        /// <summary>
        /// Field _fileStream.
        /// </summary>
        private FileStream _fileStream;

        /// <summary>
        /// Field _loggerSetup.
        /// </summary>
        private LoggerSetup _loggerSetup;

        /// <summary>
        /// Initializes a new instance of the <see cref="MutexMultiProcessFileAppender" /> class.
        /// </summary>
        /// <param name="fileName">File to write.</param>
        /// <param name="loggerSetup">Log setup.</param>
        public MutexMultiProcessFileAppender(string fileName, LoggerSetup loggerSetup)
        {
            try
            {
                this._fileName = Path.GetFullPath(fileName);

                this._fileDirectory = new DirectoryInfo(Path.GetDirectoryName(this._fileName));

                this._fileInfo = new FileInfo(this._fileName);

                this._loggerSetup = loggerSetup;

                this._mutex = this.CreateSharedMutex(this.GetMutexName(this._fileName));

                this._fileStream = this.CreateSharedFileStream(this._fileName);

                this.OpenTime = DateTime.Now;

                this.LastWriteTime = DateTime.MinValue;
            }
            catch (Exception e)
            {
                if (this._mutex != null)
                {
                    this._mutex.Close();

                    this._mutex = null;
                }

                if (this._fileStream != null)
                {
                    this._fileStream.Dispose();

                    this._fileStream = null;
                }

                ExceptionHandler.Log(e);

                throw;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="MutexMultiProcessFileAppender" /> class.
        /// </summary>
        ~MutexMultiProcessFileAppender()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the last write time of the file.
        /// </summary>
        public DateTime LastWriteTime
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the open time of the file.
        /// </summary>
        public DateTime OpenTime
        {
            get;
            private set;
        }

        /// <summary>
        /// Writes the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes to write.</param>
        public void Write(byte[] bytes)
        {
            this.CheckDisposed();

            if (this._mutex == null)
            {
                return;
            }

            if (bytes == null || bytes.Length < 1)
            {
                return;
            }

            try
            {
                this._mutex.WaitOne();
            }
            catch
            {
            }

            try
            {
                this.InternalWrite(bytes);
            }
            catch
            {
            }
            finally
            {
                this._mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Writes the specified string.
        /// </summary>
        /// <param name="text">The string to write.</param>
        /// <param name="encoding">Instance of Encoding.</param>
        public void Write(string text, Encoding encoding = null)
        {
            this.CheckDisposed();

            if (this._mutex == null)
            {
                return;
            }

            if (text == null)
            {
                return;
            }

            byte[] bytes = (encoding ?? Encoding.Default).GetBytes(text);

            try
            {
                this._mutex.WaitOne();
            }
            catch
            {
            }

            try
            {
                this.InternalWrite(bytes);
            }
            catch
            {
            }
            finally
            {
                this._mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="MutexMultiProcessFileAppender" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="MutexMultiProcessFileAppender" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="MutexMultiProcessFileAppender" /> class.
        /// protected virtual for non-sealed class; private for sealed class.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;

            if (disposing)
            {
                // dispose managed resources
                ////if (managedResource != null)
                ////{
                ////    managedResource.Dispose();
                ////    managedResource = null;
                ////}

                if (this._mutex != null)
                {
                    this._mutex.Close();

                    this._mutex = null;
                }

                if (this._fileStream != null)
                {
                    this._fileStream.Dispose();

                    this._fileStream = null;
                }
            }

            // free native resources
            ////if (nativeResource != IntPtr.Zero)
            ////{
            ////    Marshal.FreeHGlobal(nativeResource);
            ////    nativeResource = IntPtr.Zero;
            ////}
        }

        /// <summary>
        /// Writes the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes to write.</param>
        private void InternalWrite(byte[] bytes)
        {
            if (this._fileStream == null || !this._fileStream.CanWrite)
            {
                return;
            }

            if (this._loggerSetup.RollingByDate)
            {
                this._fileInfo.Refresh();

                if ((DateTime.Now.Subtract(this._fileInfo.LastWriteTime).Days > 0) || (this._loggerSetup.RollingFileSizeLimit > 0 && this._fileInfo.Length + bytes.LongLength > this._loggerSetup.RollingFileSizeLimit))
                {
                    this.ProcessRollingByDateFile();

                    this._fileStream.SetLength(0);
                }
            }
            else
            {
                if (this._loggerSetup.RollingFileSizeLimit > 0)
                {
                    this._fileInfo.Refresh();

                    if (this._fileInfo.Length + bytes.LongLength > this._loggerSetup.RollingFileSizeLimit)
                    {
                        this.ProcessRollingFile();

                        this._fileStream.SetLength(0);
                    }
                }
            }

            this._fileStream.Seek(0, SeekOrigin.End);
            this._fileStream.Write(bytes, 0, bytes.Length);
            this._fileStream.Flush();
            this.LastWriteTime = DateTime.Now;
        }

        /// <summary>
        /// Method ProcessRollingByDateFile.
        /// </summary>
        private void ProcessRollingByDateFile()
        {
            if (this._loggerSetup.RollingFileCountLimit == 0)
            {
                try
                {
                    File.Copy(this._fileName, string.Format("{0}.{1}.{2}", this._fileName, this._fileInfo.LastWriteTime.ToString("yyyyMMdd"), "0"), true);
                }
                catch
                {
                }

                return;
            }

            if (this._loggerSetup.RollingFileCountLimit < 0)
            {
                int count = 1;

                while (File.Exists(string.Format("{0}.{1}.{2}", this._fileName, this._fileInfo.LastWriteTime.ToString("yyyyMMdd"), count.ToString("0000"))))
                {
                    count++;
                }

                try
                {
                    File.Copy(this._fileName, string.Format("{0}.{1}.{2}", this._fileName, this._fileInfo.LastWriteTime.ToString("yyyyMMdd"), count.ToString("0000")), true);
                }
                catch
                {
                }
            }
            else if (this._loggerSetup.RollingFileCountLimit > 0)
            {
                int count = 1;

                while (File.Exists(string.Format("{0}.{1}.{2}", this._fileName, this._fileInfo.LastWriteTime.ToString("yyyyMMdd"), count.ToString("0000"))))
                {
                    count++;
                }

                if (count <= this._loggerSetup.RollingFileCountLimit)
                {
                    try
                    {
                        File.Copy(this._fileName, string.Format("{0}.{1}.{2}", this._fileName, this._fileInfo.LastWriteTime.ToString("yyyyMMdd"), count.ToString("0000")), true);
                    }
                    catch
                    {
                    }
                }
                else
                {
                    for (int i = 1; i < this._loggerSetup.RollingFileCountLimit; i++)
                    {
                        string sourceFile = string.Format("{0}.{1}.{2}", this._fileName, this._fileInfo.LastWriteTime.ToString("yyyyMMdd"), (i + 1).ToString("0000"));

                        string destFile = string.Format("{0}.{1}.{2}", this._fileName, this._fileInfo.LastWriteTime.ToString("yyyyMMdd"), i.ToString("0000"));

                        try
                        {
                            File.Delete(destFile);
                        }
                        catch
                        {
                        }

                        try
                        {
                            File.Move(sourceFile, destFile);
                        }
                        catch
                        {
                        }
                    }

                    try
                    {
                        File.Copy(this._fileName, string.Format("{0}.{1}.{2}", this._fileName, this._fileInfo.LastWriteTime.ToString("yyyyMMdd"), this._loggerSetup.RollingFileCountLimit.ToString("0000")), true);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Method ProcessRollingFile.
        /// </summary>
        private void ProcessRollingFile()
        {
            if (this._loggerSetup.RollingFileCountLimit == 0)
            {
                try
                {
                    File.Copy(this._fileName, string.Format("{0}.{1}", this._fileName, "0"), true);
                }
                catch
                {
                }

                return;
            }

            if (this._loggerSetup.RollingFileCountLimit < 0)
            {
                int count = 1;

                while (File.Exists(string.Format("{0}.{1}", this._fileName, count.ToString("0000"))))
                {
                    count++;
                }

                try
                {
                    File.Copy(this._fileName, string.Format("{0}.{1}", this._fileName, count.ToString("0000")), true);
                }
                catch
                {
                }
            }
            else if (this._loggerSetup.RollingFileCountLimit > 0)
            {
                int count = 1;

                while (File.Exists(string.Format("{0}.{1}", this._fileName, count.ToString("0000"))))
                {
                    count++;
                }

                if (count <= this._loggerSetup.RollingFileCountLimit)
                {
                    try
                    {
                        File.Copy(this._fileName, string.Format("{0}.{1}", this._fileName, count.ToString("0000")), true);
                    }
                    catch
                    {
                    }
                }
                else
                {
                    for (int i = 1; i < this._loggerSetup.RollingFileCountLimit; i++)
                    {
                        string sourceFile = string.Format("{0}.{1}", this._fileName, (i + 1).ToString("0000"));

                        string destFile = string.Format("{0}.{1}", this._fileName, i.ToString("0000"));

                        try
                        {
                            File.Delete(destFile);
                        }
                        catch
                        {
                        }

                        try
                        {
                            File.Move(sourceFile, destFile);
                        }
                        catch
                        {
                        }
                    }

                    try
                    {
                        File.Copy(this._fileName, string.Format("{0}.{1}", this._fileName, this._loggerSetup.RollingFileCountLimit.ToString("0000")), true);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Method CreateSharedFileStream.
        /// </summary>
        /// <param name="fileName">File to be shared open.</param>
        /// <returns>Instance of FileStream.</returns>
        private FileStream CreateSharedFileStream(string fileName)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            }
            catch
            {
            }

            return new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        /// <summary>
        /// Method CreateSharedMutex.
        /// </summary>
        /// <param name="mutexName">The mutex name.</param>
        /// <returns>Instance of Mutex.</returns>
        private Mutex CreateSharedMutex(string mutexName)
        {
            MutexSecurity mutexSecurity = new MutexSecurity();

            mutexSecurity.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow));

            bool createdNew;

            return new Mutex(false, mutexName, out createdNew, mutexSecurity);
        }

        /// <summary>
        /// Method GetMutexName.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <returns>Mutex name.</returns>
        private string GetMutexName(string fileName)
        {
            string mutexName = new Uri(Path.GetFullPath(fileName)).AbsolutePath.ToLowerInvariant();

            if (MutexNamePrefix.Length + mutexName.Length <= MaxMutexNameLength)
            {
                return MutexNamePrefix + mutexName;
            }
            else
            {
                string hash;

                using (MD5 md5 = MD5.Create())
                {
                    byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(mutexName));

                    hash = Convert.ToBase64String(bytes);
                }

                int index = mutexName.Length - (MaxMutexNameLength - MutexNamePrefix.Length - hash.Length);

                return MutexNamePrefix + hash + mutexName.Substring(index);
            }
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.Logging.MutexMultiProcessFileAppender");
            }
        }
    }
}
