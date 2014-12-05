//-----------------------------------------------------------------------
// <copyright file="DelayedCall.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Animation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Timers;

    /// <summary>
    /// DelayedCall class.
    /// </summary>
    internal class DelayedCall : IDisposable
    {
        /// <summary>
        /// Field DelayedCallList.
        /// </summary>
        private static readonly List<DelayedCall> DelayedCallList = new List<DelayedCall>();

        /// <summary>
        /// Field TimerSyncRoot.
        /// </summary>
        private readonly object _timerSyncRoot = new object();

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Field _timer.
        /// </summary>
        private System.Timers.Timer _timer;

        /// <summary>
        /// Field _callback.
        /// </summary>
        private Callback _callback;

        /// <summary>
        /// Field _oldCallback.
        /// </summary>
        private DelayedCall<object>.Callback _oldCallback = null;

        /// <summary>
        /// Field _oldData.
        /// </summary>
        private object _oldData = null;

        /// <summary>
        /// Finalizes an instance of the <see cref="DelayedCall" /> class.
        /// </summary>
        ~DelayedCall()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Callback delegate.
        /// </summary>
        public delegate void Callback();

        /// <summary>
        /// Gets registered count.
        /// </summary>
        public static int RegisteredCount
        {
            get
            {
                lock (((IList)DelayedCallList).SyncRoot)
                {
                    return DelayedCallList.Count;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether is anyone waiting.
        /// </summary>
        public static bool IsAnyWaiting
        {
            get
            {
                lock (((IList)DelayedCallList).SyncRoot)
                {
                    foreach (DelayedCall delayedCall in DelayedCallList)
                    {
                        if (delayedCall.IsWaiting)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether is waiting.
        /// </summary>
        public bool IsWaiting
        {
            get
            {
                lock (this._timerSyncRoot)
                {
                    return this._timer.Enabled && !this.Cancelled;
                }
            }
        }

        /// <summary>
        /// Gets or sets timer interval in milliseconds.
        /// </summary>
        public int Interval
        {
            get
            {
                lock (this._timerSyncRoot)
                {
                    return (int)this._timer.Interval;
                }
            }

            set
            {
                lock (this._timerSyncRoot)
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("Interval", "The new timeout must be zero or greater than zero.");
                    }
                    else if (value == 0)
                    {
                        this.Cancel();
                        this.FireNow();
                        Unregister(this);
                    }
                    else
                    {
                        this._timer.Interval = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets Timer SyncRoot.
        /// </summary>
        protected object TimerSyncRoot
        {
            get
            {
                return this._timerSyncRoot;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether cancelled.
        /// </summary>
        protected bool Cancelled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets SynchronizationContext instance.
        /// </summary>
        protected SynchronizationContext SyncContext
        {
            get;
            set;
        }

        /// <summary>
        /// Create DelayedCall.
        /// </summary>
        /// <param name="callback">Callback delegate.</param>
        /// <param name="interval">Timer interval in milliseconds.</param>
        /// <returns>DelayedCall instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static DelayedCall Create(Callback callback, int interval)
        {
            DelayedCall result = new DelayedCall();
            PrepareDelayedCallObject(result, interval, false);
            result._callback = callback;
            return result;
        }

        /// <summary>
        /// Create DelayedCall Async.
        /// </summary>
        /// <param name="callback">Callback delegate.</param>
        /// <param name="interval">Timer interval in milliseconds.</param>
        /// <returns>DelayedCall instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static DelayedCall CreateAsync(Callback callback, int interval)
        {
            DelayedCall result = new DelayedCall();
            PrepareDelayedCallObject(result, interval, true);
            result._callback = callback;
            return result;
        }

        /// <summary>
        /// Start DelayedCall.
        /// </summary>
        /// <param name="callback">Callback delegate.</param>
        /// <param name="interval">Timer interval in milliseconds.</param>
        /// <returns>DelayedCall instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static DelayedCall Start(Callback callback, int interval)
        {
            DelayedCall result = Create(callback, interval);

            if (interval > 0)
            {
                result.Start();
            }
            else if (interval == 0)
            {
                result.FireNow();
            }

            return result;
        }

        /// <summary>
        /// Start DelayedCall Async.
        /// </summary>
        /// <param name="callback">Callback delegate.</param>
        /// <param name="interval">Timer interval in milliseconds.</param>
        /// <returns>DelayedCall instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static DelayedCall StartAsync(Callback callback, int interval)
        {
            DelayedCall result = CreateAsync(callback, interval);

            if (interval > 0)
            {
                result.Start();
            }
            else if (interval == 0)
            {
                result.FireNow();
            }

            return result;
        }

        /// <summary>
        /// Cancel all.
        /// </summary>
        public static void CancelAll()
        {
            lock (((IList)DelayedCallList).SyncRoot)
            {
                foreach (DelayedCall delayedCall in DelayedCallList)
                {
                    delayedCall.Cancel();
                }
            }
        }

        /// <summary>
        /// Fire all.
        /// </summary>
        public static void FireAll()
        {
            lock (((IList)DelayedCallList).SyncRoot)
            {
                foreach (DelayedCall delayedCall in DelayedCallList)
                {
                    delayedCall.Fire();
                }
            }
        }

        /// <summary>
        /// Dispose all.
        /// </summary>
        public static void DisposeAll()
        {
            lock (((IList)DelayedCallList).SyncRoot)
            {
                while (DelayedCallList.Count > 0)
                {
                    DelayedCallList[0].Dispose();
                }
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="DelayedCall" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Start this instance.
        /// </summary>
        public void Start()
        {
            lock (this._timerSyncRoot)
            {
                this.Cancelled = false;
                this._timer.Start();
                Register(this);
            }
        }

        /// <summary>
        /// Cancel this instance.
        /// </summary>
        public void Cancel()
        {
            lock (this._timerSyncRoot)
            {
                this.Cancelled = true;
                Unregister(this);
                this._timer.Stop();
            }
        }

        /// <summary>
        /// Fire this instance.
        /// </summary>
        public void Fire()
        {
            lock (this._timerSyncRoot)
            {
                if (!this.IsWaiting)
                {
                    return;
                }

                this._timer.Stop();
            }

            this.FireNow();
        }

        /// <summary>
        /// Fire immediately.
        /// </summary>
        public void FireNow()
        {
            this.OnFire();
            Unregister(this);
        }

        /// <summary>
        /// Reset this instance.
        /// </summary>
        public void Reset()
        {
            lock (this._timerSyncRoot)
            {
                this.Cancel();
                this.Start();
            }
        }

        /// <summary>
        /// Reset this instance.
        /// </summary>
        /// <param name="interval">Timer interval in milliseconds.</param>
        public void Reset(int interval)
        {
            lock (this._timerSyncRoot)
            {
                this.Cancel();
                this.Interval = interval;
                this.Start();
            }
        }

        /// <summary>
        /// PrepareDelayedCallObject method.
        /// </summary>
        /// <param name="delayedCall">DelayedCall instance.</param>
        /// <param name="interval">Timer interval in milliseconds.</param>
        /// <param name="async">Whether use async or not.</param>
        protected static void PrepareDelayedCallObject(DelayedCall delayedCall, int interval, bool async)
        {
            if (interval < 0)
            {
                throw new ArgumentOutOfRangeException("interval", "The new timeout must be zero or greater than zero.");
            }

            delayedCall.SyncContext = null;

            if (!async)
            {
                delayedCall.SyncContext = SynchronizationContext.Current;

                if (delayedCall.SyncContext == null)
                {
                    throw new InvalidOperationException("Cannot delay calls synchronously on a non-UI thread. Use the *Async methods instead.");
                }
            }

            if (delayedCall.SyncContext == null)
            {
                delayedCall.SyncContext = new SynchronizationContext();
            }

            delayedCall._timer = new System.Timers.Timer();

            if (interval > 0)
            {
                delayedCall._timer.Interval = interval;
            }

            delayedCall._timer.AutoReset = false;
            delayedCall._timer.Elapsed += delayedCall.TimerElapsed;

            Register(delayedCall);
        }

        /// <summary>
        /// Register DelayedCall instance.
        /// </summary>
        /// <param name="delayedCall">DelayedCall instance.</param>
        protected static void Register(DelayedCall delayedCall)
        {
            lock (((IList)DelayedCallList).SyncRoot)
            {
                if (!DelayedCallList.Contains(delayedCall))
                {
                    DelayedCallList.Add(delayedCall);
                }
            }
        }

        /// <summary>
        /// Unregister DelayedCall instance.
        /// </summary>
        /// <param name="delayedCall">DelayedCall instance.</param>
        protected static void Unregister(DelayedCall delayedCall)
        {
            lock (((IList)DelayedCallList).SyncRoot)
            {
                DelayedCallList.Remove(delayedCall);
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="DelayedCall" /> class.
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

                Unregister(this);

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
        /// TimerElapsed method.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">ElapsedEventArgs instance.</param>
        protected virtual void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.FireNow();
            Unregister(this);
        }

        /// <summary>
        /// OnFire method.
        /// </summary>
        protected virtual void OnFire()
        {
            this.SyncContext.Post(
                delegate
                {
                    lock (this._timerSyncRoot)
                    {
                        if (this.Cancelled)
                        {
                            return;
                        }
                    }

                    if (this._callback != null)
                    {
                        this._callback();
                    }

                    if (this._oldCallback != null)
                    {
                        this._oldCallback(this._oldData);
                    }
                },
                null);
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.DesignPatterns.DisposePattern");
            }
        }
    }

    /// <summary>
    /// DelayedCall{T} class.
    /// </summary>
    /// <typeparam name="T">Type parameter.</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed.")]
    internal class DelayedCall<T> : DelayedCall
    {
        /// <summary>
        /// Field _callback.
        /// </summary>
        private Callback _callback;

        /// <summary>
        /// Field _data.
        /// </summary>
        private T _data;

        /// <summary>
        /// Callback delegate.
        /// </summary>
        /// <param name="data">Instance of T.</param>
        public new delegate void Callback(T data);

        /// <summary>
        /// Create DelayedCall{T}.
        /// </summary>
        /// <param name="callback">Callback delegate.</param>
        /// <param name="data">Instance of T.</param>
        /// <param name="interval">Timer interval in milliseconds.</param>
        /// <returns>DelayedCall{T} instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static DelayedCall<T> Create(Callback callback, T data, int interval)
        {
            DelayedCall<T> result = new DelayedCall<T>();
            DelayedCall.PrepareDelayedCallObject(result, interval, false);
            result._callback = callback;
            result._data = data;
            return result;
        }

        /// <summary>
        /// Create DelayedCall{T} Async.
        /// </summary>
        /// <param name="callback">Callback delegate.</param>
        /// <param name="data">Instance of T.</param>
        /// <param name="interval">Timer interval in milliseconds.</param>
        /// <returns>DelayedCall{T} instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static DelayedCall<T> CreateAsync(Callback callback, T data, int interval)
        {
            DelayedCall<T> result = new DelayedCall<T>();
            DelayedCall.PrepareDelayedCallObject(result, interval, true);
            result._callback = callback;
            result._data = data;
            return result;
        }

        /// <summary>
        /// Start DelayedCall{T}.
        /// </summary>
        /// <param name="callback">Callback delegate.</param>
        /// <param name="data">Instance of T.</param>
        /// <param name="interval">Timer interval in milliseconds.</param>
        /// <returns>DelayedCall{T} instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static DelayedCall<T> Start(Callback callback, T data, int interval)
        {
            DelayedCall<T> result = Create(callback, data, interval);
            result.Start();
            return result;
        }

        /// <summary>
        /// Start DelayedCall{T} Async.
        /// </summary>
        /// <param name="callback">Callback delegate.</param>
        /// <param name="data">Instance of T.</param>
        /// <param name="interval">Timer interval in milliseconds.</param>
        /// <returns>DelayedCall{T} instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static DelayedCall<T> StartAsync(Callback callback, T data, int interval)
        {
            DelayedCall<T> result = CreateAsync(callback, data, interval);
            result.Start();
            return result;
        }

        /// <summary>
        /// Reset this instance.
        /// </summary>
        /// <param name="data">Instance of T.</param>
        /// <param name="interval">Timer interval in milliseconds.</param>
        public void Reset(T data, int interval)
        {
            lock (this.TimerSyncRoot)
            {
                this.Cancel();
                this._data = data;
                this.Interval = interval;
                this.Start();
            }
        }

        /// <summary>
        /// OnFire method.
        /// </summary>
        protected override void OnFire()
        {
            this.SyncContext.Post(
            delegate
            {
                lock (this.TimerSyncRoot)
                {
                    if (this.Cancelled)
                    {
                        return;
                    }
                }

                if (this._callback != null)
                {
                    this._callback(this._data);
                }
            },
            null);
        }
    }
}