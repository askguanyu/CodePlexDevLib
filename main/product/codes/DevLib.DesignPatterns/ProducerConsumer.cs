//-----------------------------------------------------------------------
// <copyright file="ProducerConsumer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Producer Consumer Pattern.
    /// </summary>
    public class ProducerConsumer : IDisposable
    {
        /// <summary>
        /// The <see cref="Action{T}" /> that is executed in the consumer thread.
        /// </summary>
        private readonly Action<object> _consumerAction;

        /// <summary>
        /// Number of consumer threads.
        /// </summary>
        private readonly int _consumerThreads;

        /// <summary>
        /// Consumer thread list.
        /// </summary>
        private readonly List<Thread> _consumerThreadList;

        /// <summary>
        /// Prevents more than one thread from modifying <see cref="IsRunning" /> at a time.
        /// </summary>
        private readonly object _isRunningSyncRoot = new object();

        /// <summary>
        /// Synchronizes access to <see cref="_queue" />.
        /// </summary>
        private readonly object _queueSyncRoot = new object();

        /// <summary>
        /// The <see cref="IProducerConsumerQueue" /> that contains the data items.
        /// </summary>
        private readonly IProducerConsumerQueue _queue;

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
        /// Whether <see cref="Enqueue(object)" /> or <see cref="Enqueue(IEnumerable)" /> should add data items when <see cref="IsRunning" /> is false.
        /// </summary>
        private bool _enqueueWhenStopped = true;

        /// <summary>
        /// Whether to call <see cref="Clear" /> when <see cref="IsRunning" /> is set to false.
        /// </summary>
        private bool _clearQueueOnStop = false;

        /// <summary>
        /// Whether the consumer thread is processing data items.
        /// </summary>
        private volatile bool _isRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerConsumer" /> class.
        /// </summary>
        /// <param name="consumerAction">The <see cref="Action{T}" /> that will be executed when the consumer thread processes a data item.</param>
        /// <param name="consumerThreads">Number of consumer threads.</param>
        /// <param name="startImmediately">Whether to start the consumer thread immediately.</param>
        public ProducerConsumer(Action<object> consumerAction, int consumerThreads = 1, bool startImmediately = true)
        {
            if (consumerAction == null)
            {
                throw new ArgumentNullException("consumerAction");
            }

            if (consumerThreads < 1)
            {
                throw new ArgumentOutOfRangeException("consumerThreads", consumerThreads, "Must be greater than 0");
            }

            this._queue = new ProducerConsumerQueue();

            this._consumerAction = consumerAction;

            this._consumerThreads = consumerThreads;

            this.IsRunning = startImmediately;

            this._consumerThreadList = new List<Thread>(consumerThreads);

            for (int i = 0; i < this._consumerThreads; i++)
            {
                Thread consumerThread = new Thread(this.ConsumerThread);
                consumerThread.IsBackground = true;
                this._consumerThreadList.Add(consumerThread);
            }

            foreach (Thread item in this._consumerThreadList)
            {
                item.Start();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerConsumer" /> class.
        /// </summary>
        /// <param name="queue">An instance of <see cref="IProducerConsumerQueue" />.</param>
        /// <param name="consumerAction">The <see cref="Action{T}" /> that will be executed when the consumer thread processes a data item.</param>
        /// <param name="consumerThreads">Number of consumer threads.</param>
        /// <param name="startImmediately">Whether to start the consumer thread immediately.</param>
        public ProducerConsumer(IProducerConsumerQueue queue, Action<object> consumerAction, int consumerThreads = 1, bool startImmediately = true)
        {
            if (consumerAction == null)
            {
                throw new ArgumentNullException("consumerAction");
            }

            if (consumerThreads < 1)
            {
                throw new ArgumentOutOfRangeException("consumerThreads", consumerThreads, "Must be greater than 0");
            }

            this._queue = queue;

            this._consumerAction = consumerAction;

            this._consumerThreads = consumerThreads;

            this.IsRunning = startImmediately;

            this._consumerThreadList = new List<Thread>(consumerThreads);

            for (int i = 0; i < this._consumerThreads; i++)
            {
                Thread worker = new Thread(this.ConsumerThread);
                worker.IsBackground = true;
                this._consumerThreadList.Add(worker);
            }

            foreach (Thread item in this._consumerThreadList)
            {
                item.Start();
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ProducerConsumer" /> class.
        /// </summary>
        ~ProducerConsumer()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the number of items contained in the queue.
        /// </summary>
        public long QueueCount
        {
            get
            {
                lock (this._queueSyncRoot)
                {
                    return this._queue.Count();
                }
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

                if (this._consumerThreadList == null || this._consumerThreadList.Count < 1)
                {
                    return false;
                }

                lock (this._queueSyncRoot)
                {
                    if (this._queue.Count() < 1)
                    {
                        foreach (Thread item in this._consumerThreadList)
                        {
                            if ((item.ThreadState & ThreadState.WaitSleepJoin) != ThreadState.WaitSleepJoin)
                            {
                                return false;
                            }
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
        /// <param name="enqueueWhenStopped">Whether <see cref="Enqueue(object)" /> or <see cref="Enqueue(IEnumerable)" /> is able to add data items when stopped.</param>
        /// <param name="clearQueueOnStop">Whether to call <see cref="Clear" /> when stopped.</param>
        public void Stop(bool enqueueWhenStopped = true, bool clearQueueOnStop = false)
        {
            this.CheckDisposed();

            this._enqueueWhenStopped = enqueueWhenStopped;

            this._clearQueueOnStop = clearQueueOnStop;

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
        public void Enqueue(object item)
        {
            this.CheckDisposed();

            lock (this._queueSyncRoot)
            {
                if (this.IsRunning || this._enqueueWhenStopped)
                {
                    this._queue.Enqueue(item);

                    this._queueWaitHandle.Set();
                }
            }
        }

        /// <summary>
        /// Enqueue data items.
        /// </summary>
        /// <param name="items">The data items to enqueue.</param>
        public void Enqueue(IEnumerable items)
        {
            this.CheckDisposed();

            lock (this._queueSyncRoot)
            {
                if (this.IsRunning || this._enqueueWhenStopped)
                {
                    this._queue.Enqueue(items);

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

                if (this._consumerThreadList != null)
                {
                    foreach (Thread item in this._consumerThreadList)
                    {
                        try
                        {
                            item.Abort();
                        }
                        catch
                        {
                        }
                    }

                    this._consumerThreadList.Clear();
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
                    object nextItem = null;

                    bool itemExists;

                    lock (this._queueSyncRoot)
                    {
                        itemExists = this._queue.Count() > 0;

                        if (itemExists)
                        {
                            nextItem = this._queue.Dequeue();
                        }
                    }

                    if (itemExists)
                    {
                        try
                        {
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
                throw new ObjectDisposedException("DevLib.DesignPatterns.ProducerConsumer");
            }
        }
    }

    /// <summary>
    /// Producer Consumer Pattern.
    /// </summary>
    /// <typeparam name="T">The type of item to be produced and consumed.</typeparam>
    public class ProducerConsumer<T> : IDisposable
    {
        /// <summary>
        /// The <see cref="Action{T}" /> that is executed in the consumer thread.
        /// </summary>
        private readonly Action<T> _consumerAction;

        /// <summary>
        /// Number of consumer threads.
        /// </summary>
        private readonly int _consumerThreads;

        /// <summary>
        /// Consumer thread list.
        /// </summary>
        private readonly List<Thread> _consumerThreadList;

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
        private readonly IProducerConsumerQueue<T> _queue;

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
        /// Whether <see cref="Enqueue(T)" /> or <see cref="Enqueue(IEnumerable{T})" /> should add data items when <see cref="IsRunning" /> is false.
        /// </summary>
        private bool _enqueueWhenStopped = true;

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
        /// <param name="consumerThreads">Number of consumer threads.</param>
        /// <param name="startImmediately">Whether to start the consumer thread immediately.</param>
        public ProducerConsumer(Action<T> consumerAction, int consumerThreads = 1, bool startImmediately = true)
        {
            if (consumerAction == null)
            {
                throw new ArgumentNullException("consumerAction");
            }

            if (consumerThreads < 1)
            {
                throw new ArgumentOutOfRangeException("consumerThreads", consumerThreads, "Must be greater than 0");
            }

            this._queue = new ProducerConsumerQueue<T>();

            this._consumerAction = consumerAction;

            this._consumerThreads = consumerThreads;

            this.IsRunning = startImmediately;

            this._consumerThreadList = new List<Thread>(consumerThreads);

            for (int i = 0; i < this._consumerThreads; i++)
            {
                Thread consumerThread = new Thread(this.ConsumerThread);
                consumerThread.IsBackground = true;
                this._consumerThreadList.Add(consumerThread);
            }

            foreach (Thread item in this._consumerThreadList)
            {
                item.Start();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerConsumer{T}" /> class.
        /// </summary>
        /// <param name="queue">An instance of <see cref="IProducerConsumerQueue{T}" />.</param>
        /// <param name="consumerAction">The <see cref="Action{T}" /> that will be executed when the consumer thread processes a data item.</param>
        /// <param name="consumerThreads">Number of consumer threads.</param>
        /// <param name="startImmediately">Whether to start the consumer thread immediately.</param>
        public ProducerConsumer(IProducerConsumerQueue<T> queue, Action<T> consumerAction, int consumerThreads = 1, bool startImmediately = true)
        {
            if (consumerAction == null)
            {
                throw new ArgumentNullException("consumerAction");
            }

            if (consumerThreads < 1)
            {
                throw new ArgumentOutOfRangeException("consumerThreads", consumerThreads, "Must be greater than 0");
            }

            this._queue = queue;

            this._consumerAction = consumerAction;

            this._consumerThreads = consumerThreads;

            this.IsRunning = startImmediately;

            this._consumerThreadList = new List<Thread>(consumerThreads);

            for (int i = 0; i < this._consumerThreads; i++)
            {
                Thread worker = new Thread(this.ConsumerThread);
                worker.IsBackground = true;
                this._consumerThreadList.Add(worker);
            }

            foreach (Thread item in this._consumerThreadList)
            {
                item.Start();
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ProducerConsumer{T}" /> class.
        /// </summary>
        ~ProducerConsumer()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the number of items contained in the queue.
        /// </summary>
        public long QueueCount
        {
            get
            {
                lock (this._queueSyncRoot)
                {
                    return this._queue.Count();
                }
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

                if (this._consumerThreadList == null || this._consumerThreadList.Count < 1)
                {
                    return false;
                }

                lock (this._queueSyncRoot)
                {
                    if (this._queue.Count() < 1)
                    {
                        foreach (Thread item in this._consumerThreadList)
                        {
                            if ((item.ThreadState & ThreadState.WaitSleepJoin) != ThreadState.WaitSleepJoin)
                            {
                                return false;
                            }
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
        /// <param name="enqueueWhenStopped">Whether <see cref="Enqueue(T)" /> or <see cref="Enqueue(IEnumerable{T})" /> is able to add data items when stopped.</param>
        /// <param name="clearQueueOnStop">Whether to call <see cref="Clear" /> when stopped.</param>
        public void Stop(bool enqueueWhenStopped = true, bool clearQueueOnStop = false)
        {
            this.CheckDisposed();

            this._enqueueWhenStopped = enqueueWhenStopped;

            this._clearQueueOnStop = clearQueueOnStop;

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
                if (this.IsRunning || this._enqueueWhenStopped)
                {
                    this._queue.Enqueue(item);

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
                if (this.IsRunning || this._enqueueWhenStopped)
                {
                    this._queue.Enqueue(items);

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

                if (this._consumerThreadList != null)
                {
                    foreach (Thread item in this._consumerThreadList)
                    {
                        try
                        {
                            item.Abort();
                        }
                        catch
                        {
                        }
                    }

                    this._consumerThreadList.Clear();
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
                        itemExists = this._queue.Count() > 0;

                        if (itemExists)
                        {
                            nextItem = this._queue.Dequeue();
                        }
                    }

                    if (itemExists)
                    {
                        try
                        {
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
                throw new ObjectDisposedException("DevLib.DesignPatterns.ProducerConsumer{T}");
            }
        }
    }
}
