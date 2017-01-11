using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Collections.Concurrent
{
    [ComVisible(false)]
    [DebuggerDisplay("Count={Count}")]
    [DebuggerTypeProxy(typeof(CollectionDebuggerView<>))]
    public class BlockingCollection<T> : IEnumerable<T>, ICollection, IEnumerable, IDisposable
    {
        private const int spinCount = 5;

        private static Stopwatch watch = Stopwatch.StartNew();
        private readonly IProducerConsumerCollection<T> underlyingColl;

        private readonly int upperBound;
        private int addId = int.MinValue;
        private int completeId;
        private AtomicBoolean isComplete;
        private ManualResetEventSlim mreAdd = new ManualResetEventSlim(true);
        private ManualResetEventSlim mreRemove = new ManualResetEventSlim(true);
        private int removeId = int.MinValue;

        public BlockingCollection()
            : this(new ConcurrentQueue<T>(), -1)
        {
        }

        public BlockingCollection(int boundedCapacity)
            : this(new ConcurrentQueue<T>(), boundedCapacity)
        {
        }

        public BlockingCollection(IProducerConsumerCollection<T> collection)
            : this(collection, -1)
        {
        }

        public BlockingCollection(IProducerConsumerCollection<T> collection, int boundedCapacity)
        {
            this.underlyingColl = collection;
            this.upperBound = boundedCapacity;
            this.isComplete = new AtomicBoolean();
        }

        public int BoundedCapacity
        {
            get
            {
                return upperBound;
            }
        }

        public int Count
        {
            get
            {
                return underlyingColl.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return underlyingColl.IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return underlyingColl.SyncRoot;
            }
        }

        public bool IsAddingCompleted
        {
            get
            {
                return isComplete.Value;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return isComplete.Value && addId == removeId;
            }
        }

        public static int AddToAny(BlockingCollection<T>[] collections, T item)
        {
            return AddToAny(collections, item, CancellationToken.None);
        }

        public static int AddToAny(BlockingCollection<T>[] collections, T item, CancellationToken cancellationToken)
        {
            CheckArray(collections);
            WaitHandle[] wait_table = null;
            while (true)
            {
                for (int i = 0; i < collections.Length; ++i)
                {
                    if (collections[i].TryAdd(item))
                        return i;
                }
                cancellationToken.ThrowIfCancellationRequested();
                if (wait_table == null)
                {
                    wait_table = new WaitHandle[collections.Length + 1];
                    for (int i = 0; i < collections.Length; ++i)
                        wait_table[i] = collections[i].mreAdd.WaitHandle;
                    wait_table[collections.Length] = cancellationToken.WaitHandle;
                }
                WaitHandle.WaitAny(wait_table);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public static int TakeFromAny(BlockingCollection<T>[] collections, out T item)
        {
            return TakeFromAny(collections, out item, CancellationToken.None);
        }

        public static int TakeFromAny(BlockingCollection<T>[] collections, out T item, CancellationToken cancellationToken)
        {
            item = default(T);
            CheckArray(collections);
            WaitHandle[] wait_table = null;
            while (true)
            {
                for (int i = 0; i < collections.Length; ++i)
                {
                    if (collections[i].TryTake(out item))
                        return i;
                }
                cancellationToken.ThrowIfCancellationRequested();
                if (wait_table == null)
                {
                    wait_table = new WaitHandle[collections.Length + 1];
                    for (int i = 0; i < collections.Length; ++i)
                        wait_table[i] = collections[i].mreRemove.WaitHandle;
                    wait_table[collections.Length] = cancellationToken.WaitHandle;
                }
                WaitHandle.WaitAny(wait_table);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public static int TryAddToAny(BlockingCollection<T>[] collections, T item)
        {
            CheckArray(collections);
            int index = 0;
            foreach (var coll in collections)
            {
                if (coll.TryAdd(item))
                    return index;
                index++;
            }
            return -1;
        }

        public static int TryAddToAny(BlockingCollection<T>[] collections, T item, TimeSpan timeout)
        {
            CheckArray(collections);
            int index = 0;
            foreach (var coll in collections)
            {
                if (coll.TryAdd(item, timeout))
                    return index;
                index++;
            }
            return -1;
        }

        public static int TryAddToAny(BlockingCollection<T>[] collections, T item, int millisecondsTimeout)
        {
            CheckArray(collections);
            int index = 0;
            foreach (var coll in collections)
            {
                if (coll.TryAdd(item, millisecondsTimeout))
                    return index;
                index++;
            }
            return -1;
        }

        public static int TryAddToAny(BlockingCollection<T>[] collections, T item, int millisecondsTimeout,
                                       CancellationToken cancellationToken)
        {
            CheckArray(collections);
            int index = 0;
            foreach (var coll in collections)
            {
                if (coll.TryAdd(item, millisecondsTimeout, cancellationToken))
                    return index;
                index++;
            }
            return -1;
        }

        public static int TryTakeFromAny(BlockingCollection<T>[] collections, out T item)
        {
            item = default(T);

            CheckArray(collections);
            int index = 0;
            foreach (var coll in collections)
            {
                if (coll.TryTake(out item))
                    return index;
                index++;
            }
            return -1;
        }

        public static int TryTakeFromAny(BlockingCollection<T>[] collections, out T item, TimeSpan timeout)
        {
            item = default(T);

            CheckArray(collections);
            int index = 0;
            foreach (var coll in collections)
            {
                if (coll.TryTake(out item, timeout))
                    return index;
                index++;
            }
            return -1;
        }

        public static int TryTakeFromAny(BlockingCollection<T>[] collections, out T item, int millisecondsTimeout)
        {
            item = default(T);

            CheckArray(collections);
            int index = 0;
            foreach (var coll in collections)
            {
                if (coll.TryTake(out item, millisecondsTimeout))
                    return index;
                index++;
            }
            return -1;
        }

        public static int TryTakeFromAny(BlockingCollection<T>[] collections, out T item, int millisecondsTimeout,
                                          CancellationToken cancellationToken)
        {
            item = default(T);

            CheckArray(collections);
            int index = 0;
            foreach (var coll in collections)
            {
                if (coll.TryTake(out item, millisecondsTimeout, cancellationToken))
                    return index;
                index++;
            }
            return -1;
        }

        public void Add(T item)
        {
            Add(item, CancellationToken.None);
        }

        public void Add(T item, CancellationToken cancellationToken)
        {
            TryAdd(item, -1, cancellationToken);
        }

        public void CompleteAdding()
        {
            completeId = addId;
            isComplete.Value = true;
            mreAdd.Set();
            mreRemove.Set();
        }

        public void CopyTo(T[] array, int index)
        {
            underlyingColl.CopyTo(array, index);
        }

        public void Dispose()
        {
        }

        public IEnumerable<T> GetConsumingEnumerable()
        {
            return GetConsumingEnumerable(CancellationToken.None);
        }

        public IEnumerable<T> GetConsumingEnumerable(CancellationToken cancellationToken)
        {
            while (true)
            {
                T item = default(T);

                try
                {
                    item = Take(cancellationToken);
                }
                catch
                {
                    if (IsCompleted)
                        break;
                    throw;
                }

                yield return item;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            underlyingColl.CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)underlyingColl).GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ((IEnumerable<T>)underlyingColl).GetEnumerator();
        }

        public T Take()
        {
            return Take(CancellationToken.None);
        }

        public T Take(CancellationToken cancellationToken)
        {
            T item;
            TryTake(out item, -1, cancellationToken, true);

            return item;
        }

        public T[] ToArray()
        {
            return underlyingColl.ToArray();
        }

        public bool TryAdd(T item)
        {
            return TryAdd(item, 0, CancellationToken.None);
        }

        public bool TryAdd(T item, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (millisecondsTimeout < -1)
                throw new ArgumentOutOfRangeException("millisecondsTimeout");

            long start = millisecondsTimeout == -1 ? 0 : watch.ElapsedMilliseconds;
            SpinWait sw = new SpinWait();

            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                int cachedAddId = addId;
                int cachedRemoveId = removeId;
                int itemsIn = cachedAddId - cachedRemoveId;

                if (isComplete.Value && cachedAddId >= completeId)
                    ThrowCompleteException();

                if (upperBound != -1 && itemsIn >= upperBound)
                {
                    if (millisecondsTimeout == 0)
                        return false;

                    if (sw.Count <= spinCount)
                    {
                        sw.SpinOnce();
                    }
                    else
                    {
                        mreRemove.Reset();
                        if (cachedRemoveId != removeId || cachedAddId != addId)
                        {
                            mreRemove.Set();
                            continue;
                        }

                        mreRemove.Wait(ComputeTimeout(millisecondsTimeout, start), cancellationToken);
                    }

                    continue;
                }

                if (Interlocked.CompareExchange(ref addId, cachedAddId + 1, cachedAddId) != cachedAddId)
                    continue;

                if (!underlyingColl.TryAdd(item))
                    throw new InvalidOperationException("The underlying collection didn't accept the item.");

                mreAdd.Set();

                return true;
            } while (millisecondsTimeout == -1 || (watch.ElapsedMilliseconds - start) < millisecondsTimeout);

            return false;
        }

        public bool TryAdd(T item, TimeSpan timeout)
        {
            return TryAdd(item, (int)timeout.TotalMilliseconds);
        }

        public bool TryAdd(T item, int millisecondsTimeout)
        {
            return TryAdd(item, millisecondsTimeout, CancellationToken.None);
        }

        public bool TryTake(out T item)
        {
            return TryTake(out item, 0, CancellationToken.None);
        }

        public bool TryTake(out T item, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            return TryTake(out item, millisecondsTimeout, cancellationToken, false);
        }

        public bool TryTake(out T item, TimeSpan timeout)
        {
            return TryTake(out item, (int)timeout.TotalMilliseconds);
        }

        public bool TryTake(out T item, int millisecondsTimeout)
        {
            item = default(T);

            return TryTake(out item, millisecondsTimeout, CancellationToken.None, false);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        private static void CheckArray(BlockingCollection<T>[] collections)
        {
            if (collections == null)
                throw new ArgumentNullException("collections");
            if (collections.Length == 0 || IsThereANullElement(collections))
                throw new ArgumentException("The collections argument is a 0-length array or contains a null element.", "collections");
        }

        private static int ComputeTimeout(int millisecondsTimeout, long start)
        {
            return millisecondsTimeout == -1 ? 500 : (int)Math.Max(watch.ElapsedMilliseconds - start - millisecondsTimeout, 1);
        }

        private static bool IsThereANullElement(BlockingCollection<T>[] collections)
        {
            foreach (BlockingCollection<T> e in collections)
                if (e == null)
                    return true;
            return false;
        }

        private void ThrowCompleteException()
        {
            throw new InvalidOperationException("The BlockingCollection<T> has"
                                                 + " been marked as complete with regards to additions.");
        }

        private bool TryTake(out T item, int milliseconds, CancellationToken cancellationToken, bool throwComplete)
        {
            if (milliseconds < -1)
                throw new ArgumentOutOfRangeException("milliseconds");

            item = default(T);
            SpinWait sw = new SpinWait();
            long start = milliseconds == -1 ? 0 : watch.ElapsedMilliseconds;

            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                int cachedRemoveId = removeId;
                int cachedAddId = addId;

                if (cachedRemoveId == cachedAddId)
                {
                    if (milliseconds == 0)
                        return false;

                    if (IsCompleted)
                    {
                        if (throwComplete)
                            ThrowCompleteException();
                        else
                            return false;
                    }

                    if (sw.Count <= spinCount)
                    {
                        sw.SpinOnce();
                    }
                    else
                    {
                        mreAdd.Reset();
                        if (cachedRemoveId != removeId || cachedAddId != addId)
                        {
                            mreAdd.Set();
                            continue;
                        }

                        mreAdd.Wait(ComputeTimeout(milliseconds, start), cancellationToken);
                    }

                    continue;
                }

                if (Interlocked.CompareExchange(ref removeId, cachedRemoveId + 1, cachedRemoveId) != cachedRemoveId)
                    continue;

                while (!underlyingColl.TryTake(out item)) ;

                mreRemove.Set();

                return true;
            } while (milliseconds == -1 || (watch.ElapsedMilliseconds - start) < milliseconds);

            return false;
        }
    }
}
