namespace DevLib.Samples
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Linq;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class SyncDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> innerDict;
        private ReaderWriterLockSlim readWriteLock;

        public SyncDictionary()
        {
            this.readWriteLock = new ReaderWriterLockSlim();
            this.innerDict = new Dictionary<TKey, TValue>();
        }

        public SyncDictionary(int capacity)
        {
            this.readWriteLock = new ReaderWriterLockSlim();
            this.innerDict = new Dictionary<TKey, TValue>(capacity);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            using (new AcquireWriteLock(this.readWriteLock))
            {
                this.innerDict[item.Key] = item.Value;
            }
        }

        public void Add(TKey key, TValue value)
        {
            using (new AcquireWriteLock(this.readWriteLock))
            {
                this.innerDict[key] = value;
            }
        }

        public void Clear()
        {
            using (new AcquireWriteLock(this.readWriteLock))
            {
                this.innerDict.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            using (new AcquireReadLock(this.readWriteLock))
            {
                return this.innerDict.Contains<KeyValuePair<TKey, TValue>>(item);
            }
        }

        public bool ContainsKey(TKey key)
        {
            using (new AcquireReadLock(this.readWriteLock))
            {
                return this.innerDict.ContainsKey(key);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            using (new AcquireReadLock(this.readWriteLock))
            {
                this.innerDict.ToArray<KeyValuePair<TKey, TValue>>().CopyTo(array, arrayIndex);
            }
        }

        public IEnumerator GetEnumerator()
        {
            using (new AcquireReadLock(this.readWriteLock))
            {
                return this.innerDict.GetEnumerator();
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            using (new AcquireReadLock(this.readWriteLock))
            {
                return this.innerDict.GetEnumerator();
            }
        }

        public bool Remove(TKey key)
        {
            bool isRemoved;
            using (new AcquireWriteLock(this.readWriteLock))
            {
                isRemoved = this.innerDict.Remove(key);
            }
            return isRemoved;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            using (new AcquireWriteLock(this.readWriteLock))
            {
                return this.innerDict.Remove(item.Key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            using (new AcquireReadLock(this.readWriteLock))
            {
                return this.innerDict.TryGetValue(key, out value);
            }
        }

        public int Count
        {
            get
            {
                using (new AcquireReadLock(this.readWriteLock))
                {
                    return this.innerDict.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                using (new AcquireReadLock(this.readWriteLock))
                {
                    return this.innerDict[key];
                }
            }
            set
            {
                using (new AcquireWriteLock(this.readWriteLock))
                {
                    this.innerDict[key] = value;
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                using (new AcquireReadLock(this.readWriteLock))
                {
                    return this.innerDict.Keys;
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                using (new AcquireReadLock(this.readWriteLock))
                {
                    return this.innerDict.Values;
                }
            }
        }

        private class AcquireReadLock : IDisposable
        {
            private ReaderWriterLockSlim rwLock;
            private bool disposedValue;

            public AcquireReadLock(ReaderWriterLockSlim rwLock)
            {
                this.rwLock = new ReaderWriterLockSlim();
                this.disposedValue = false;
                this.rwLock = rwLock;
                this.rwLock.EnterReadLock();
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!this.disposedValue && disposing)
                {
                    this.rwLock.ExitReadLock();
                }
                this.disposedValue = true;
            }
        }

        private class AcquireWriteLock : IDisposable
        {
            private ReaderWriterLockSlim rwLock;
            private bool disposedValue;

            public AcquireWriteLock(ReaderWriterLockSlim rwLock)
            {
                this.rwLock = new ReaderWriterLockSlim();
                this.disposedValue = false;
                this.rwLock = rwLock;
                this.rwLock.EnterWriteLock();
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!this.disposedValue && disposing)
                {
                    this.rwLock.ExitWriteLock();
                }
                this.disposedValue = true;
            }
        }
    }
}
