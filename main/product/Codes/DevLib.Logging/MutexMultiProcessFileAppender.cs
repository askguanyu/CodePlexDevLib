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
        private const string MutexNamePrefix = @"Global\DevLibLogging_";

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
        /// Field _fileInfo.
        /// </summary>
        private FileInfo _fileInfo;

        /// <summary>
        /// Field _loggerSetup.
        /// </summary>
        private LoggerSetup _loggerSetup;

        /// <summary>
        /// Initializes a new instance of the <see cref="MutexMultiProcessFileAppender" /> class.
        /// </summary>
        /// <param name="filename">File to write.</param>
        /// <param name="loggerSetup">Log setup.</param>
        public MutexMultiProcessFileAppender(string filename, LoggerSetup loggerSetup)
        {
            try
            {
                this._fileName = LogConfigManager.GetFileFullPath(filename);

                this._fileInfo = new FileInfo(this._fileName);

                this._loggerSetup = loggerSetup;

                this._mutex = this.CreateSharedMutex(this.GetMutexName(this._fileName));

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

                InternalLogger.Log(e);

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
            }
            finally
            {
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
        }

        /// <summary>
        /// Writes the specified string.
        /// </summary>
        /// <param name="text">The string to write.</param>
        public void Write(string text)
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

            try
            {
            }
            finally
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);

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
        }

        /// <summary>
        /// Writes the specified lines.
        /// </summary>
        /// <param name="lines">The lines to write.</param>
        public void Write(string[] lines)
        {
            this.CheckDisposed();

            if (this._mutex == null)
            {
                return;
            }

            if (lines == null)
            {
                return;
            }

            try
            {
            }
            finally
            {
                try
                {
                    this._mutex.WaitOne();
                }
                catch
                {
                }

                try
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(string.Join(string.Empty, lines));
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
            this.LastWriteTime = DateTime.Now;

            FileStream fileStream = null;

            try
            {
                fileStream = this.CreateSharedFileStream(this._fileName);

                if (this._loggerSetup.RollingByDate)
                {
                    this._fileInfo.Refresh();

                    if ((DateTime.Now.Subtract(this._fileInfo.LastWriteTime).Days > 0) || (this._loggerSetup.RollingFileSizeLimit > 0 && this._fileInfo.Length + bytes.LongLength > this._loggerSetup.RollingFileSizeLimit))
                    {
                        this.ProcessRollingByDateFile();

                        fileStream.SetLength(0);
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

                            fileStream.SetLength(0);
                        }
                    }
                }

                fileStream.Seek(0, SeekOrigin.End);
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Flush();
            }
            catch
            {
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Dispose();
                    fileStream = null;
                }
            }
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
                            File.Copy(sourceFile, destFile, true);
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
                            File.Copy(sourceFile, destFile, true);
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
        /// <param name="filename">File to be shared open.</param>
        /// <returns>Instance of FileStream.</returns>
        private FileStream CreateSharedFileStream(string filename)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
            }
            catch
            {
            }

            return new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
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
        /// <param name="filename">File name.</param>
        /// <returns>Mutex name.</returns>
        private string GetMutexName(string filename)
        {
            byte[] hash;

            using (MD5 hasher = MD5.Create())
            {
                hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(Path.GetFullPath(filename).ToLowerInvariant()));
            }

            return MutexNamePrefix + BitConverter.ToString(hash).Replace("-", string.Empty);
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
