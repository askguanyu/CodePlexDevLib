//-----------------------------------------------------------------------
// <copyright file="ThreadTimer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Timers
{
    using System;
    using System.Threading;

    /// <summary>
    /// Provides a mechanism for executing a method at specified intervals.
    /// </summary>
    public class ThreadTimer : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Field _timer.
        /// </summary>
        private System.Threading.Timer _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadTimer" /> class.
        /// </summary>
        public ThreadTimer()
            : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadTimer" /> class.
        /// </summary>
        /// <param name="interval">The time, in milliseconds, between events. If interval is zero (0), the event is raised once.</param>
        /// <param name="firstStartTime">The time to delay before event is raised. If less than or equal to DateTime.Now, the timer will start immediately.</param>
        public ThreadTimer(uint interval, DateTime firstStartTime = default(DateTime))
        {
            this.Interval = interval;
            this.FirstStartTime = firstStartTime;
            this.IsRunning = false;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ThreadTimer" /> class.
        /// </summary>
        ~ThreadTimer()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Occurs when the interval elapses.
        /// </summary>
        public event EventHandler Elapsed;

        /// <summary>
        /// Gets or sets the interval in milliseconds at which to raise the <see cref="E:Elapsed" /> event. If interval is zero (0), the event is raised once.
        /// </summary>
        public uint Interval
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time to delay before event is raised. If less than or equal to DateTime.Now, the timer will start immediately.
        /// </summary>
        public DateTime FirstStartTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether current ThreadTimer is running or not.
        /// </summary>
        public bool IsRunning
        {
            get;
            private set;
        }

        /// <summary>
        /// Start ThreadTimer.
        /// </summary>
        public void Start()
        {
            this.CheckDisposed();

            if (!this.IsRunning)
            {
                try
                {
                    double totalMilliseconds = (this.FirstStartTime - DateTime.Now).TotalMilliseconds;

                    uint dueTime = totalMilliseconds > 0 ? (uint)totalMilliseconds : 0;

                    if (this._timer == null)
                    {
                        this._timer = new System.Threading.Timer(new TimerCallback(this.OnTimerElapsed), null, dueTime, this.Interval);
                    }
                    else
                    {
                        if (!this._timer.Change(dueTime, this.Interval))
                        {
                            this._timer.Dispose();

                            this._timer = null;

                            this._timer = new System.Threading.Timer(new TimerCallback(this.OnTimerElapsed), null, dueTime, this.Interval);
                        }
                    }

                    this.IsRunning = true;
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
        /// Start ThreadTimer immediately.
        /// </summary>
        public void StartNow()
        {
            this.CheckDisposed();

            try
            {
                if (this._timer == null)
                {
                    this._timer = new System.Threading.Timer(new TimerCallback(this.OnTimerElapsed), null, 0, this.Interval);
                }
                else
                {
                    if (!this._timer.Change(0, this.Interval))
                    {
                        this._timer.Dispose();

                        this._timer = null;

                        this._timer = new System.Threading.Timer(new TimerCallback(this.OnTimerElapsed), null, 0, this.Interval);
                    }
                }

                this.IsRunning = true;
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
        /// Restart ThreadTimer.
        /// </summary>
        public void Restart()
        {
            this.CheckDisposed();

            try
            {
                double totalMilliseconds = (this.FirstStartTime - DateTime.Now).TotalMilliseconds;

                uint dueTime = totalMilliseconds > 0 ? (uint)totalMilliseconds : 0;

                if (this._timer == null)
                {
                    this._timer = new System.Threading.Timer(new TimerCallback(this.OnTimerElapsed), null, dueTime, this.Interval);
                }
                else
                {
                    if (!this._timer.Change(dueTime, this.Interval))
                    {
                        this._timer.Dispose();

                        this._timer = null;

                        this._timer = new System.Threading.Timer(new TimerCallback(this.OnTimerElapsed), null, dueTime, this.Interval);
                    }
                }

                this.IsRunning = true;
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
        /// Stop ThreadTimer.
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
                    }
                }
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ThreadTimer" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ThreadTimer" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ThreadTimer" /> class.
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
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler temp = Interlocked.CompareExchange(ref this.Elapsed, null, null);

            if (temp != null)
            {
                temp(this, null);
            }
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.Timers.ThreadTimer");
            }
        }
    }
}
