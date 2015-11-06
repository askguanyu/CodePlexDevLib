//-----------------------------------------------------------------------
// <copyright file="ObjectPool.cs" company="YuGuan Corporation">
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
    /// Delegate method to create a new instance.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <returns>A new instance</returns>
    public delegate TResult PooledObjectBuilderFunc<TResult>();

    /// <summary>
    /// Generic implementation of object pooling pattern with predefined pool size limit.
    /// </summary>
    /// <typeparam name="T">The type of element in the object pool.</typeparam>
    public class ObjectPool<T> : IDisposable
    {
        /// <summary>
        /// Field _builderFunc.
        /// </summary>
        private readonly PooledObjectBuilderFunc<T> _builderFunc;

        /// <summary>
        /// Field _acquireFunc.
        /// </summary>
        private readonly PooledObjectBuilderFunc<T> _acquireFunc;

        /// <summary>
        /// Field _store.
        /// </summary>
        private readonly IStore _store;

        /// <summary>
        /// Field _maxCapacity.
        /// </summary>
        private readonly int? _maxCapacity;

        /// <summary>
        /// Field _syncRoot.
        /// </summary>
        private readonly Semaphore _syncRoot;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Field _count.
        /// </summary>
        private int _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPool{T}" /> class.
        /// </summary>
        /// <param name="builder">Delegate method to create a new instance.</param>
        /// <param name="maxCapacity">The maximum capacity of the object pool.</param>
        /// <param name="storeMode">The access mode.</param>
        /// <param name="loadingMode">The loading mode.</param>
        public ObjectPool(PooledObjectBuilderFunc<T> builder, int? maxCapacity = null, ObjectPoolStoreMode storeMode = ObjectPoolStoreMode.Stack, ObjectPoolLoadingMode loadingMode = ObjectPoolLoadingMode.Lazy)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            if (maxCapacity.HasValue && maxCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException("maxCapacity", maxCapacity, "Argument 'maxCapacity' must be greater than zero.");
            }

            this._maxCapacity = maxCapacity;
            this._builderFunc = builder;

            if (this._maxCapacity.HasValue)
            {
                this._syncRoot = new Semaphore(this._maxCapacity.Value, this._maxCapacity.Value);
            }
            else
            {
                this._syncRoot = null;
            }

            switch (storeMode)
            {
                case ObjectPoolStoreMode.Queue:
                    this._store = this._maxCapacity.HasValue ? new QueueStore(this._maxCapacity.Value) : new QueueStore();
                    break;
                case ObjectPoolStoreMode.Stack:
                    this._store = this._maxCapacity.HasValue ? new StackStore(this._maxCapacity.Value) : new StackStore();
                    break;
                case ObjectPoolStoreMode.CircularQueue:
                default:
                    this._store = this._maxCapacity.HasValue ? new CircularQueueStore(this._maxCapacity.Value) : new CircularQueueStore();
                    break;
            }

            switch (loadingMode)
            {
                case ObjectPoolLoadingMode.Eager:
                    if (this._maxCapacity.HasValue)
                    {
                        this.PreloadAll();
                        this._acquireFunc = this.AcquireEager;
                    }
                    else
                    {
                        this._acquireFunc = this.AcquireLazy;
                    }

                    break;
                case ObjectPoolLoadingMode.Lazy:
                    this._acquireFunc = this.AcquireLazy;
                    break;
                case ObjectPoolLoadingMode.LazyExpanding:
                default:
                    if (this._maxCapacity.HasValue)
                    {
                        this._acquireFunc = this.AcquireLazyExpanding;
                    }
                    else
                    {
                        this._acquireFunc = this.AcquireLazy;
                    }

                    break;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ObjectPool{T}"/> class.
        /// </summary>
        ~ObjectPool()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Interface of store.
        /// </summary>
        private interface IStore
        {
            /// <summary>
            /// Gets the number of elements contained in the store.
            /// </summary>
            int Count
            {
                get;
            }

            /// <summary>
            /// Gets an object that can be used to synchronize access to the store.
            /// </summary>
            object SyncRoot
            {
                get;
            }

            /// <summary>
            /// Fetches one instance.
            /// </summary>
            /// <returns>One Instance.</returns>
            T Fetch();

            /// <summary>
            /// Returns the specified object to the store.
            /// </summary>
            /// <param name="value">The object to store.</param>
            void Store(T value);
        }

        /// <summary>
        /// Gets the number of elements contained in the object pool.
        /// </summary>
        public int Count
        {
            get
            {
                return Thread.VolatileRead(ref this._count);
            }
        }

        /// <summary>
        /// Gets an object from the pool.
        /// </summary>
        /// <param name="releaseAction">The action call on the object before return the object to the pool.</param>
        /// <returns>A ObjectPoolRegistration instance represents the object in the pool.</returns>
        public ObjectPoolRegistration<T> Acquire(Action<T> releaseAction = null)
        {
            this.CheckDisposed();

            if (this._syncRoot != null)
            {
                this._syncRoot.WaitOne();
            }

            T value = this._acquireFunc();

            Action<T> action = item =>
            {
                if (releaseAction != null)
                {
                    releaseAction(item);
                }

                this.Release(item);
            };

            return new ObjectPoolRegistration<T>(value, action);
        }

        /// <summary>
        /// Returns the object to the pool.
        /// </summary>
        /// <param name="value">The object to return to the pool.</param>
        public void Release(T value)
        {
            this.CheckDisposed();

            lock (this._store.SyncRoot)
            {
                this._store.Store(value);
            }

            if (this._syncRoot != null)
            {
                this._syncRoot.Release();
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ObjectPool{T}" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ObjectPool{T}" /> class.
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

                if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
                {
                    lock (this._store.SyncRoot)
                    {
                        while (this._store.Count > 0)
                        {
                            IDisposable disposable = (IDisposable)this._store.Fetch();
                            disposable.Dispose();
                        }
                    }
                }

                if (this._syncRoot != null)
                {
                    this._syncRoot.Close();
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
                throw new ObjectDisposedException("DevLib.DesignPatterns.ObjectPool{T}");
            }
        }

        /// <summary>
        /// Gets an object with eager mode.
        /// </summary>
        /// <returns>An object from the pool.</returns>
        private T AcquireEager()
        {
            lock (this._store.SyncRoot)
            {
                return this._store.Fetch();
            }
        }

        /// <summary>
        /// Gets an object with lazy mode.
        /// </summary>
        /// <returns>An object from the pool.</returns>
        private T AcquireLazy()
        {
            lock (this._store.SyncRoot)
            {
                if (this._store.Count > 0)
                {
                    return this._store.Fetch();
                }
            }

            Interlocked.Increment(ref this._count);

            return this._builderFunc();
        }

        /// <summary>
        /// Gets an object with lazy expanding mode.
        /// </summary>
        /// <returns>An object from the pool.</returns>
        private T AcquireLazyExpanding()
        {
            bool shouldExpand = false;

            if (this._count < this._maxCapacity.Value)
            {
                int newCount = Interlocked.Increment(ref this._count);

                if (newCount <= this._maxCapacity.Value)
                {
                    shouldExpand = true;
                }
                else
                {
                    Interlocked.Decrement(ref this._count);
                }
            }

            if (shouldExpand)
            {
                return this._builderFunc();
            }
            else
            {
                lock (this._store.SyncRoot)
                {
                    return this._store.Fetch();
                }
            }
        }

        /// <summary>
        /// Preloads all the items to the pool.
        /// </summary>
        private void PreloadAll()
        {
            for (int i = 0; i < this._maxCapacity.Value; i++)
            {
                T item = this._builderFunc();
                this._store.Store(item);
            }

            this._count = this._maxCapacity.Value;
        }

        /// <summary>
        /// Represents a last-in-first-out (LIFO) collection of objects.
        /// </summary>
        private class StackStore : Stack<T>, IStore
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StackStore"/> class.
            /// </summary>
            public StackStore()
                : base()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="StackStore"/> class.
            /// </summary>
            /// <param name="capacity">The initial number of elements that the System.Collections.Generic.Stack{T} can contain.</param>
            public StackStore(int capacity)
                : base(capacity)
            {
            }

            /// <summary>
            /// Gets an object that can be used to synchronize access to the store.
            /// </summary>
            public object SyncRoot
            {
                get
                {
                    return ((ICollection)this).SyncRoot;
                }
            }

            /// <summary>
            /// Fetches one instance.
            /// </summary>
            /// <returns>One Instance.</returns>
            public T Fetch()
            {
                return this.Pop();
            }

            /// <summary>
            /// Returns the specified object to the store.
            /// </summary>
            /// <param name="value">The object to store.</param>
            public void Store(T value)
            {
                this.Push(value);
            }
        }

        /// <summary>
        /// Represents a first-in, first-out (FIFO) collection of objects.
        /// </summary>
        private class QueueStore : Queue<T>, IStore
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="QueueStore"/> class.
            /// </summary>
            public QueueStore()
                : base()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="QueueStore"/> class.
            /// </summary>
            /// <param name="capacity">The initial number of elements that the System.Collections.Generic.Queue{T} can contain.</param>
            public QueueStore(int capacity)
                : base(capacity)
            {
            }

            /// <summary>
            /// Gets an object that can be used to synchronize access to the store.
            /// </summary>
            public object SyncRoot
            {
                get
                {
                    return ((ICollection)this).SyncRoot;
                }
            }

            /// <summary>
            /// Fetches one instance.
            /// </summary>
            /// <returns>One Instance.</returns>
            public T Fetch()
            {
                return this.Dequeue();
            }

            /// <summary>
            /// Returns the specified item to the store.
            /// </summary>
            /// <param name="value">The object to store.</param>
            public void Store(T value)
            {
                this.Enqueue(value);
            }
        }

        /// <summary>
        /// Represents a circular FIFO queue.
        /// </summary>
        private class CircularQueueStore : IStore
        {
            /// <summary>
            /// Field _slots.
            /// </summary>
            private readonly List<Slot> _slots;

            /// <summary>
            /// Field _freeSlotCount.
            /// </summary>
            private int _freeSlotCount;

            /// <summary>
            /// Field _position.
            /// </summary>
            private int _position = -1;

            /// <summary>
            /// Initializes a new instance of the <see cref="CircularQueueStore"/> class.
            /// </summary>
            public CircularQueueStore()
            {
                this._slots = new List<Slot>();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CircularQueueStore"/> class.
            /// </summary>
            /// <param name="capacity">The initial number of elements that the CircularStore can contain.</param>
            public CircularQueueStore(int capacity)
            {
                this._slots = new List<Slot>(capacity);
            }

            /// <summary>
            /// Gets the number of elements contained in the store.
            /// </summary>
            public int Count
            {
                get { return this._freeSlotCount; }
            }

            /// <summary>
            /// Gets an object that can be used to synchronize access to the store.
            /// </summary>
            public object SyncRoot
            {
                get
                {
                    return ((ICollection)this._slots).SyncRoot;
                }
            }

            /// <summary>
            /// Fetches one instance.
            /// </summary>
            /// <returns>One Instance.</returns>
            public T Fetch()
            {
                if (this.Count == 0)
                {
                    throw new InvalidOperationException("The CircularStore buffer is empty.");
                }

                int startPosition = this._position;

                do
                {
                    this.Advance();

                    Slot slot = this._slots[this._position];

                    if (!slot.IsInUse)
                    {
                        slot.IsInUse = true;
                        --this._freeSlotCount;

                        return slot.Value;
                    }
                }
                while (startPosition != this._position);

                throw new InvalidOperationException("CircularStore has no free slot.");
            }

            /// <summary>
            /// Returns the specified object to the store.
            /// </summary>
            /// <param name="value">The object to store.</param>
            public void Store(T value)
            {
                Slot slot = this._slots.Find(s => object.Equals(s.Value, value));

                if (slot == null)
                {
                    slot = new Slot(value);
                    this._slots.Add(slot);
                }

                slot.IsInUse = false;
                ++this._freeSlotCount;
            }

            /// <summary>
            /// Advances the position.
            /// </summary>
            private void Advance()
            {
                this._position = (this._position + 1) % this._slots.Count;
            }

            /// <summary>
            /// Represents a slot in the store.
            /// </summary>
            private class Slot
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="Slot"/> class.
                /// </summary>
                /// <param name="value">The object represent the slot.</param>
                public Slot(T value)
                {
                    this.Value = value;
                }

                /// <summary>
                /// Gets the object.
                /// </summary>
                public T Value
                {
                    get;
                    private set;
                }

                /// <summary>
                /// Gets or sets a value indicating whether the slot is in use.
                /// </summary>
                public bool IsInUse
                {
                    get;
                    set;
                }
            }
        }
    }
}
