//-----------------------------------------------------------------------
// <copyright file="ProducerConsumer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    /// Producer Consumer Pattern.
    /// </summary>
    /// <typeparam name="T">The type of item to be produced and consumed.</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed.")]
    internal class ProducerConsumer<T> : IDisposable
    {
        /// <summary>
        /// The <see cref="Action{T}" /> that is executed in the consumer thread.
        /// </summary>
        private readonly Action<T> _consumerAction;

        /// <summary>
        /// Consumer thread list.
        /// </summary>
        private readonly Thread _consumerThread;

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
        /// Allows the consumer thread to block when no items are available in the <see cref="_queue" />.
        /// </summary>
        private readonly EventWaitHandle _queueWaitHandle = new AutoResetEvent(false);

        /// <summary>
        /// Allows the consumer thread to block when <see cref="IsRunning" /> is false.
        /// </summary>
        private readonly EventWaitHandle _isRunningWaitHandle = new ManualResetEvent(false);

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
        /// Initializes a new instance of the <see cref="ProducerConsumer{T}" /> class.
        /// </summary>
        /// <param name="consumerAction">The <see cref="Action{T}" /> that will be executed when the consumer thread processes a data item.</param>
        /// <param name="startNow">Whether to start the consumer thread immediately.</param>
        public ProducerConsumer(Action<T> consumerAction, bool startNow = true)
        {
            if (consumerAction == null)
            {
                throw new ArgumentNullException("consumerAction");
            }

            this._queue = new Queue<T>();

            Interlocked.Exchange(ref this._produceAccumulation, this._queue.Count);

            this._consumerAction = consumerAction;

            this.IsRunning = startNow;

            this._consumerThread = new Thread(this.ConsumerThread);
            this._consumerThread.IsBackground = true;
            this._consumerThread.Start();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ProducerConsumer{T}" /> class.
        /// </summary>
        ~ProducerConsumer()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the current number of items contained in the queue.
        /// </summary>
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
        public long ConsumeAccumulation
        {
            get
            {
                return Interlocked.Read(ref this._consumeAccumulation);
            }
        }

        /// <summary>
        /// Gets a value indicating whether all consumer threads are idle or not. Only when queue is empty and all threads are running and idle return true; otherwise, false.
        /// </summary>
        public bool IsIdle
        {
            get
            {
                this.CheckDisposed();

                if (!this.IsRunning)
                {
                    return false;
                }

                lock (this._queueSyncRoot)
                {
                    if (this._queue.Count < 1)
                    {
                        if ((this._consumerThread.ThreadState & ThreadState.WaitSleepJoin) != ThreadState.WaitSleepJoin)
                        {
                            return false;
                        }

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the consumer thread is running.
        /// </summary>
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
                    if (value == this._isRunning)
                    {
                        return;
                    }

                    if (value)
                    {
                        this._queueWaitHandle.Reset();

                        this._isRunning = true;

                        this._isRunningWaitHandle.Set();
                    }
                    else
                    {
                        this._isRunningWaitHandle.Reset();

                        this._isRunning = false;

                        this._queueWaitHandle.Set();
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

            lock (this._queueSyncRoot)
            {
                if (this.IsRunning || this._enableQueueWhenStopped)
                {
                    this._queue.Enqueue(item);
                    Interlocked.Increment(ref this._produceAccumulation);
                    this._queueWaitHandle.Set();
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

            lock (this._queueSyncRoot)
            {
                if (this.IsRunning || this._enableQueueWhenStopped)
                {
                    foreach (var item in items)
                    {
                        this._queue.Enqueue(item);
                        Interlocked.Add(ref this._produceAccumulation, 1);
                    }

                    this._queueWaitHandle.Set();
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

                try
                {
                    this._consumerThread.Abort();
                }
                catch
                {
                }

                this._queueWaitHandle.Close();

                this._isRunningWaitHandle.Close();

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
        /// The consumer thread.
        /// </summary>
        private void ConsumerThread()
        {
            while (!this._disposed)
            {
                if (this.IsRunning)
                {
                    T nextItem = default(T);

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
                            Interlocked.Increment(ref this._consumeAccumulation);
                            this._consumerAction(nextItem);
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
                else
                {
                    if (this._clearQueueOnStop)
                    {
                        this.Clear();
                    }

                    this._isRunningWaitHandle.WaitOne();
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
                throw new ObjectDisposedException("DevLib.ServiceBus.ProducerConsumer{T}");
            }
        }
    }
}
