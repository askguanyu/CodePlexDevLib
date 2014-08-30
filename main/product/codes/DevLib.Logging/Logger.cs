//-----------------------------------------------------------------------
// <copyright file="Logger.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    /// Class Logger. Provides logging functions.
    /// </summary>
    public sealed class Logger : IDisposable
    {
        /// <summary>
        /// Field _logFile.
        /// </summary>
        private readonly string _logFile;

        /// <summary>
        /// Field _loggerSetup.
        /// </summary>
        private readonly LoggerSetup _loggerSetup;

        /// <summary>
        /// Field _fileAppender.
        /// </summary>
        private readonly MutexMultiProcessFileAppender _fileAppender;

        /// <summary>
        /// Synchronizes access to <see cref="_queue" />.
        /// </summary>
        private readonly object _queueSyncRoot = new object();

        /// <summary>
        /// The <see cref="Queue{T}" /> that contains the data items.
        /// </summary>
        private readonly Queue<string> _queue;

        /// <summary>
        /// Allows the consumer thread to block when no items are available in the <see cref="_queue" />.
        /// </summary>
        private readonly EventWaitHandle _queueWaitHandle;

        /// <summary>
        /// Field _consumerThread.
        /// </summary>
        private readonly Thread _consumerThread;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger" /> class.
        /// </summary>
        /// <param name="logFile">Log file.</param>
        /// <param name="loggerSetup">Log setup.</param>
        internal Logger(string logFile, LoggerSetup loggerSetup)
        {
            this._logFile = logFile;

            this._loggerSetup = loggerSetup;

            if (this._loggerSetup.WriteToFile)
            {
                try
                {
                    this._fileAppender = new MutexMultiProcessFileAppender(this._logFile, this._loggerSetup);

                    this._queueWaitHandle = new AutoResetEvent(false);

                    this._queue = new Queue<string>();

                    this._consumerThread = new Thread(this.ConsumerThread);

                    this._consumerThread.IsBackground = true;

                    this._consumerThread.Start();
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Logger" /> class.
        /// </summary>
        ~Logger()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="Logger" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="Logger" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Writes the diagnostic message at INFO level.
        /// </summary>
        /// <param name="objs">Diagnostic messages or objects to log.</param>
        public void Log(params object[] objs)
        {
            this.Log(1, LogLevel.INFO, objs);
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <param name="logLevel">Log level.</param>
        /// <param name="objs">Diagnostic messages or objects to log.</param>
        public void Log(LogLevel logLevel, params object[] objs)
        {
            this.Log(1, logLevel, objs);
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <param name="skipFrames">The number of frames up the stack to skip.</param>
        /// <param name="logLevel">Log level.</param>
        /// <param name="objs">Diagnostic messages or objects to log.</param>
        public void Log(int skipFrames, LogLevel logLevel, params object[] objs)
        {
            if (((this._loggerSetup.WriteToConsole && Environment.UserInteractive) || this._fileAppender != null) && logLevel >= this._loggerSetup.Level)
            {
                try
                {
                    string message = LogLayout.Render(skipFrames, logLevel, this._loggerSetup.UseBracket, objs);

                    if (this._loggerSetup.WriteToConsole && Environment.UserInteractive)
                    {
                        this.WriteColoredConsole(logLevel, message);
                    }

                    if (this._fileAppender != null)
                    {
                        lock (this._queueSyncRoot)
                        {
                            this._queue.Enqueue(message);

                            this._queueWaitHandle.Set();
                        }
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="Logger" /> class.
        /// protected virtual for non-sealed class; private for sealed class.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
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

                if (this._queueWaitHandle != null)
                {
                    this._queueWaitHandle.Close();
                }

                if (this._fileAppender != null)
                {
                    this._fileAppender.Dispose();
                }

                if (this._consumerThread != null)
                {
                    try
                    {
                        this._consumerThread.Abort();
                    }
                    catch
                    {
                    }
                }

                if (this._queue != null)
                {
                    this._queue.Clear();
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
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.Logging");
            }
        }

        /// <summary>
        /// Writes the specified string value with foreground color, followed by the current line terminator, to the standard output stream.
        /// </summary>
        /// <param name="logLevel">Log level.</param>
        /// <param name="message">Message to write.</param>
        private void WriteColoredConsole(LogLevel logLevel, string message)
        {
            ConsoleColor originalForeColor = Console.ForegroundColor;

            switch (logLevel)
            {
                case LogLevel.DBUG:
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    break;
                case LogLevel.INFO:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case LogLevel.EXCP:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogLevel.WARN:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.ERRO:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.FAIL:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                default:
                    break;
            }

            Console.WriteLine(message);

            Console.ForegroundColor = originalForeColor;
        }

        /// <summary>
        /// The consumer thread.
        /// </summary>
        private void ConsumerThread()
        {
            while (true)
            {
                string nextItem = null;

                bool itemExists;

                lock (this._queueSyncRoot)
                {
                    itemExists = this._queue.Count > 0;

                    if (itemExists)
                    {
                        nextItem = this._queue.Dequeue();
                    }
                }

                if (itemExists)
                {
                    try
                    {
                        this._fileAppender.Write(nextItem);
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                    }
                }
                else
                {
                    this._queueWaitHandle.WaitOne();
                }
            }
        }
    }
}
