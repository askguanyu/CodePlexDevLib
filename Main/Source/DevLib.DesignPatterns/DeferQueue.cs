//-----------------------------------------------------------------------
// <copyright file="DeferQueue.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Producer Consumer Pattern.
    /// </summary>
    /// <typeparam name="T">The type of item to be produced and consumed.</typeparam>
    /// <seealso cref="System.IDisposable" />
    public class DeferQueue<T> : IDisposable
    {
        /// <summary>
        /// The <see cref="Action{T}" /> that is executed in the consumer thread.
        /// </summary>
        private readonly Action<T[]> _consumerAction;

        /// <summary>
        /// Prevents more than one thread from modifying <see cref="IsRunning" /> at a time.
        /// </summary>
        private readonly object _isRunningSyncRoot = new object();

        /// <summary>
        /// Synchronizes access to <see cref="_queue" />.
        /// </summary>
        private readonly object _queueSyncRoot = new object();

        /// <summary>
        /// The <see cref="IProducerConsumerQueue{T}" /> that contains the data items.
        /// </summary>
        private readonly Queue<T> _queue;

        /// <summary>
        /// The maximum backlogs.
        /// </summary>
        private readonly int _maxBacklogs;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Field _produceAccumulation.
        /// </summary>
        private long _produceAccumulation = 0;

        /// <summary>
        /// Field _consumeAccumulation.
        /// </summary>
        private long _consumeAccumulation = 0;

        /// <summary>
        /// Whether <see cref="Enqueue(T)" /> or <see cref="Enqueue(IEnumerable{T})" /> should add data items when <see cref="IsRunning" /> is false.
        /// </summary>
        private bool _enableQueueWhenStopped = true;

        /// <summary>
        /// Whether to call <see cref="Clear" /> when <see cref="IsRunning" /> is set to false.
        /// </summary>
        private bool _clearQueueOnStop = false;

        /// <summary>
        /// Whether the consumer thread is processing data items.
        /// </summary>
        private volatile bool _isRunning;

        /// <summary>
        /// The defer timer.
        /// </summary>
        private System.Timers.Timer _deferTimer;

        /// <summary>
        /// The queue timer.
        /// </summary>
        private System.Timers.Timer _queueTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferQueue{T}" /> class.
        /// </summary>
        /// <param name="consumerAction">The <see cref="Action{T}" /> that will be executed when the consumer thread processes a data item.</param>
        /// <param name="deferTimeout">The defer timeout in milliseconds. If the value less than or equal to zero, there is no defer timeout.</param>
        /// <param name="queueTimeout">The queue timeout in milliseconds. If the value less than or equal to zero, there is no queue timeout.</param>
        /// <param name="maxBacklogs">The threshold of maximum items in the queue. If the value less than or equal to zero, do not check maximum items.</param>
        /// <param name="startNow">Whether to start the consumer thread immediately.</param>
        public DeferQueue(Action<T[]> consumerAction, int deferTimeout, int queueTimeout, int maxBacklogs = 0, bool startNow = true)
        {
            if (consumerAction == null)
            {
                throw new ArgumentNullException("consumerAction");
            }

            this._queue = new Queue<T>();
            this._consumerAction = consumerAction;
            this._maxBacklogs = maxBacklogs;

            if (deferTimeout > 0)
            {
                this._deferTimer = new System.Timers.Timer(deferTimeout);
                this._deferTimer.Elapsed += (s, e) => this.OnThreshold();
                this._deferTimer.AutoReset = false;
                this._deferTimer.Stop();
            }

            if (queueTimeout > 0)
            {
                this._queueTimer = new System.Timers.Timer(queueTimeout);
                this._queueTimer.Elapsed += (s, e) => this.OnThreshold();
                this._queueTimer.AutoReset = false;
                this._queueTimer.Stop();
            }

            this.IsRunning = startNow;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="DeferQueue{T}" /> class.
        /// </summary>
        ~DeferQueue()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the current number of items contained in the queue.
        /// </summary>
        /// <value>The queue count.</value>
        public long QueueCount
        {
            get
            {
                lock (this._queueSyncRoot)
                {
                    return this._queue.Count;
                }
            }
        }

        /// <summary>
        /// Gets the accumulation count of produced items.
        /// </summary>
        /// <value>The produce accumulation.</value>
        public long ProduceAccumulation
        {
            get
            {
                return Interlocked.Read(ref this._produceAccumulation);
            }
        }

        /// <summary>
        /// Gets the accumulation count of consumed items.
        /// </summary>
        /// <value>The consume accumulation.</value>
        public long ConsumeAccumulation
        {
            get
            {
                return Interlocked.Read(ref this._consumeAccumulation);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the consumer thread is running.
        /// </summary>
        /// <value><c>true</c> if this instance is running; otherwise, <c>false</c>.</value>
        public bool IsRunning
        {
            get
            {
                return this._isRunning;
            }

            private set
            {
                lock (this._isRunningSyncRoot)
                {
                    if (this._isRunning == value)
                    {
                        return;
                    }

                    this._isRunning = value;

                    if (!this._isRunning)
                    {
                        if (this._deferTimer != null)
                        {
                            this._deferTimer.Stop();
                        }

                        if (this._queueTimer != null)
                        {
                            this._queueTimer.Stop();
                        }

                        if (this._clearQueueOnStop)
                        {
                            this.Clear();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Start the consumer thread.
        /// </summary>
        public void Start()
        {
            this.CheckDisposed();

            this.IsRunning = true;
        }

        /// <summary>
        /// Stop the consumer thread.
        /// </summary>
        /// <param name="enableQueue">Whether <see cref="Enqueue(T)" /> or <see cref="Enqueue(IEnumerable{T})" /> is able to add data items when stopped.</param>
        /// <param name="clearQueue">Whether to call <see cref="Clear" /> when stopped.</param>
        public void Stop(bool enableQueue = true, bool clearQueue = false)
        {
            this.CheckDisposed();

            this._enableQueueWhenStopped = enableQueue;
            this._clearQueueOnStop = clearQueue;

            this.IsRunning = false;
        }

        /// <summary>
        /// Clear all data items from the queue.
        /// </summary>
        public void Clear()
        {
            lock (this._queueSyncRoot)
            {
                this._queue.Clear();
            }
        }

        /// <summary>
        /// Enqueue a data item.
        /// </summary>
        /// <param name="item">The data item to enqueue.</param>
        public void Enqueue(T item)
        {
            this.CheckDisposed();

            this.ResetQueueTimer();

            lock (this._queueSyncRoot)
            {
                if (this.IsRunning || this._enableQueueWhenStopped)
                {
                    this._queue.Enqueue(item);
                    Interlocked.Increment(ref this._produceAccumulation);
                    this.ResetDeferTimer();
                    this.OnReachMaxBacklogs();
                }
            }
        }

        /// <summary>
        /// Enqueue data items.
        /// </summary>
        /// <param name="items">The data items to enqueue.</param>
        public void Enqueue(IEnumerable<T> items)
        {
            this.CheckDisposed();

            this.ResetQueueTimer();

            lock (this._queueSyncRoot)
            {
                if (this.IsRunning || this._enableQueueWhenStopped)
                {
                    foreach (var item in items)
                    {
                        this._queue.Enqueue(item);
                        Interlocked.Increment(ref this._produceAccumulation);
                    }

                    this.ResetDeferTimer();
                    this.OnReachMaxBacklogs();
                }
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ProducerConsumer{T}" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ProducerConsumer{T}" /> class.
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

                if (this._deferTimer != null)
                {
                    this._deferTimer.Stop();
                    this._deferTimer.Dispose();
                    this._deferTimer = null;
                }

                if (this._queueTimer != null)
                {
                    this._queueTimer.Stop();
                    this._queueTimer.Dispose();
                    this._queueTimer = null;
                }

                this._queue.Clear();

                this._isRunning = false;
            }

            // free native resources
            ////if (nativeResource != IntPtr.Zero)
            ////{
            ////    Marshal.FreeHGlobal(nativeResource);
            ////    nativeResource = IntPtr.Zero;
            ////}
        }

        /// <summary>
        /// Called when reached timeout or backlogs threshold.
        /// </summary>
        private void OnThreshold()
        {
            T[] nextItems = null;

            bool itemExists;

            lock (this._queueSyncRoot)
            {
                itemExists = this._queue.Count > 0;

                if (itemExists)
                {
                    nextItems = this._queue.ToArray();
                    this._queue.Clear();
                }
            }

            if (itemExists && nextItems != null)
            {
                try
                {
                    this._consumerAction(nextItems);
                    Interlocked.Add(ref this._consumeAccumulation, nextItems.LongLength);
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }
            }
        }

        /// <summary>
        /// Resets the defer timer.
        /// </summary>
        private void ResetDeferTimer()
        {
            if (this.IsRunning && this._deferTimer != null)
            {
                this._deferTimer.Stop();
                this._deferTimer.Start();
            }
        }

        /// <summary>
        /// Resets the queue timer.
        /// </summary>
        private void ResetQueueTimer()
        {
            if (this.IsRunning && this._queueTimer != null && !this._queueTimer.Enabled)
            {
                this._queueTimer.Stop();
                this._queueTimer.Start();
            }
        }

        /// <summary>
        /// Called when the queue is reached maximum backlogs.
        /// </summary>
        private void OnReachMaxBacklogs()
        {
            if (this.IsRunning && this._maxBacklogs > 0 && this._queue.Count >= this._maxBacklogs)
            {
                this.OnThreshold();
            }
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.DesignPatterns.DeferQueue{T}");
            }
        }
    }
}
