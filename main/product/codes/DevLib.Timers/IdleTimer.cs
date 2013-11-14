﻿//-----------------------------------------------------------------------
// <copyright file="IdleTimer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Timers
{
    using System;
    using System.Threading;
    using DevLib.Timers.NativeAPI;

    /// <summary>
    /// Represents a timer to detect user idle state of an application.
    /// </summary>
    public class IdleTimer : IDisposable
    {
        /// <summary>
        /// Field PollingInterval.
        /// </summary>
        private const long PollingInterval = 250;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Field _timer.
        /// </summary>
        private System.Threading.Timer _timer;

        /// <summary>
        /// Field _idleTimeout.
        /// </summary>
        private long _idleTimeout;

        /// <summary>
        /// Field _autoStop.
        /// </summary>
        private bool _autoStop;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdleTimer" /> class.
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds when user is idle state.</param>
        /// <param name="autoStart">true if immediately start IdleTimer; otherwise, false.</param>
        /// <param name="autoStop">Whether stop current IdleTimer when IdleOccurred event raised.</param>
        public IdleTimer(long timeout, bool autoStart = false, bool autoStop = true)
        {
            if (timeout < 1)
            {
                throw new ArgumentOutOfRangeException("timeout", "The value of the timeout parameter is less than or equal to zero");
            }

            this._idleTimeout = timeout;

            this._autoStop = autoStop;

            this.IsRunning = false;

            if (autoStart)
            {
                this.Start();
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="IdleTimer" /> class.
        /// </summary>
        ~IdleTimer()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Idle Occurred Event.
        /// </summary>
        public event EventHandler IdleOccurred;

        /// <summary>
        /// Gets a value indicating whether current IdleTimer is running or not.
        /// </summary>
        public bool IsRunning
        {
            get;
            private set;
        }

        /// <summary>
        /// Start IdleTimer.
        /// </summary>
        public void Start()
        {
            this.CheckDisposed();

            if (!this.IsRunning)
            {
                try
                {
                    if (this._timer == null)
                    {
                        this._timer = new System.Threading.Timer(new TimerCallback(this.OnTimerElapsed), null, 0, PollingInterval);

                        this.IsRunning = true;
                    }
                    else
                    {
                        if (this._timer.Change(0, PollingInterval))
                        {
                            this.IsRunning = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);

                    if (this._timer != null)
                    {
                        this._timer.Dispose();

                        this._timer = null;
                    }

                    this.IsRunning = false;

                    throw;
                }
            }
        }

        /// <summary>
        /// Restart IdleTimer.
        /// </summary>
        public void Restart()
        {
            this.CheckDisposed();

            try
            {
                if (this._timer == null)
                {
                    this._timer = new System.Threading.Timer(new TimerCallback(this.OnTimerElapsed), null, 0, PollingInterval);

                    this.IsRunning = true;
                }
                else
                {
                    if (this._timer.Change(0, PollingInterval))
                    {
                        this.IsRunning = true;
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);

                if (this._timer != null)
                {
                    this._timer.Dispose();

                    this._timer = null;
                }

                this.IsRunning = false;

                throw;
            }
        }

        /// <summary>
        /// Stop IdleTimer.
        /// </summary>
        public void Stop()
        {
            this.CheckDisposed();

            if (this.IsRunning)
            {
                this.IsRunning = false;

                if (this._timer != null)
                {
                    try
                    {
                        if (!this._timer.Change(Timeout.Infinite, Timeout.Infinite))
                        {
                            this._timer.Dispose();

                            this._timer = null;
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);

                        if (this._timer != null)
                        {
                            this._timer.Dispose();

                            this._timer = null;
                        }

                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="IdleTimer" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="IdleTimer" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="IdleTimer" /> class.
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

                if (this._timer != null)
                {
                    this._timer.Dispose();
                    this._timer = null;
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
        /// Method OnTimerElapsed
        /// </summary>
        /// <param name="obj">An object containing application-specific information relevant to the method invoked by this delegate, or null.</param>
        private void OnTimerElapsed(object obj)
        {
            if (NativeMethodsHelper.GetLastInputTime() >= this._idleTimeout)
            {
                if (this._autoStop)
                {
                    this.Stop();
                }

                // Copy a reference to the delegate field now into a temporary field for thread safety
                EventHandler temp = Interlocked.CompareExchange(ref this.IdleOccurred, null, null);

                if (temp != null)
                {
                    temp(this, null);
                }
            }
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.Timers.IdleTimer");
            }
        }
    }
}