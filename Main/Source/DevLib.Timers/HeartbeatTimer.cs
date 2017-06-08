//-----------------------------------------------------------------------
// <copyright file="HeartbeatTimer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Timers
{
    using System;
    using System.Timers;

    /// <summary>
    /// Class HeartbeatTimer.
    /// </summary>
    public class HeartbeatTimer : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Field _timer.
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Field _autoStop.
        /// </summary>
        private bool _autoStop;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeartbeatTimer" /> class.
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds. The value must be greater than zero and less than or equal to System.Int32.MaxValue.</param>
        /// <param name="startNow">true if immediately start HeartbeatTimer; otherwise, false.</param>
        /// <param name="autoStop">Whether stop current HeartbeatTimer when TimeoutOccurred event raised.</param>
        public HeartbeatTimer(double timeout, bool startNow = false, bool autoStop = true)
        {
            this.Timeout = timeout;
            this._autoStop = autoStop;
            this._timer = new Timer(timeout);
            this._timer.Elapsed += new ElapsedEventHandler(this.TimerElapsed);

            if (startNow)
            {
                this.Heartbeat();
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="HeartbeatTimer" /> class.
        /// </summary>
        ~HeartbeatTimer()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Timeout Occurred Event.
        /// </summary>
        public event ElapsedEventHandler TimeoutOccurred;

        /// <summary>
        /// Heartbeat Occurred Event.
        /// </summary>
        public event EventHandler HeartbeatOccurred;

        /// <summary>
        /// Gets a value indicating whether this HeartbeatTimer is alive.
        /// </summary>
        public bool IsAlive
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the timeout in milliseconds. The value must be greater than zero and less than or equal to System.Int32.MaxValue.
        /// </summary>
        public double Timeout
        {
            get;
            set;
        }

        /// <summary>
        /// Stop HeartbeatTimer.
        /// </summary>
        public void Stop()
        {
            this.CheckDisposed();
            this._timer.Stop();
            this.IsAlive = false;
        }

        /// <summary>
        /// Sends heartbeat.
        /// </summary>
        public void Heartbeat()
        {
            this.CheckDisposed();
            this._timer.Stop();
            this.OnHeartbeatOccurred();
            this._timer.Interval = this.Timeout;
            this._timer.Start();
            this.IsAlive = true;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="HeartbeatTimer" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="HeartbeatTimer" /> class.
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
        /// Called when timer elapsed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            this._timer.Stop();
            this.IsAlive = false;
            this.OnTimeoutOccurred(e);

            if (!this._autoStop)
            {
                this._timer.Interval = this.Timeout;
                this._timer.Start();
            }
        }

        /// <summary>
        /// Called when timeout occurred.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void OnTimeoutOccurred(ElapsedEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety.
            ElapsedEventHandler safeHandler = System.Threading.Interlocked.CompareExchange(ref this.TimeoutOccurred, null, null);

            if (safeHandler != null)
            {
                safeHandler(this, e);
            }
        }

        /// <summary>
        /// Called when heartbeat occurred.
        /// </summary>
        private void OnHeartbeatOccurred()
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety.
            EventHandler safeHandler = System.Threading.Interlocked.CompareExchange(ref this.HeartbeatOccurred, null, null);

            if (safeHandler != null)
            {
                safeHandler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Checks whether this instance is disposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.Timers.HeartbeatTimer");
            }
        }
    }
}
