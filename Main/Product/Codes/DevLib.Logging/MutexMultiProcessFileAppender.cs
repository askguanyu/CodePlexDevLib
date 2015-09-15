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
        /// Field _directoryName.
        /// </summary>
        private string _directoryName;

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

                this._directoryName = Path.GetDirectoryName(this._fileName);

                this._fileInfo = new FileInfo(this._fileName);

                this._loggerSetup = loggerSetup;

                this._mutex = this.CreateSharedMutex(this.GetMutexName(this._fileName));
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
            if (!Directory.Exists(this._directoryName))
            {
                try
                {
                    Directory.CreateDirectory(this._directoryName);
                }
                catch
                {
                }
            }

            bool isRolling = this.ProcessRollingFile(bytes.LongLength);

            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(this._fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);

                if (isRolling)
                {
                    fileStream.SetLength(0);
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
        /// Processes the rolling file.
        /// </summary>
        /// <param name="byteCount">The byte count.</param>
        /// <returns>true if created a rolling file; otherwise, false.</returns>
        private bool ProcessRollingFile(long byteCount)
        {
            try
            {
                if (this._loggerSetup.RollingByDate)
                {
                    this._fileInfo.Refresh();

                    if (this._fileInfo.Exists && ((DateTime.Now.Subtract(this._fileInfo.LastWriteTime).Days > 0) || (this._loggerSetup.RollingFileSizeLimit > 0 && this._fileInfo.Length + byteCount > this._loggerSetup.RollingFileSizeLimit)))
                    {
                        this.ProcessRollingFileByDate();

                        return true;
                    }
                }
                else
                {
                    if (this._loggerSetup.RollingFileSizeLimit > 0)
                    {
                        this._fileInfo.Refresh();

                        if (this._fileInfo.Exists && this._fileInfo.Length + byteCount > this._loggerSetup.RollingFileSizeLimit)
                        {
                            this.ProcessRollingFileBySize();

                            return true;
                        }
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        /// <summary>
        /// Processes the rolling file by date.
        /// </summary>
        private void ProcessRollingFileByDate()
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
        /// Processes the rolling file by size.
        /// </summary>
        private void ProcessRollingFileBySize()
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
