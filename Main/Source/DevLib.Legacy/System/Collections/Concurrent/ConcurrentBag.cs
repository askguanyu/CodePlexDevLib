using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Concurrent
{
    [ComVisible(false)]
    [DebuggerDisplay("Count={Count}")]
    [DebuggerTypeProxy(typeof(CollectionDebuggerView<>))]
    public class ConcurrentBag<T> : IProducerConsumerCollection<T>, IEnumerable<T>, IEnumerable
    {
        private ConcurrentDictionary<int, CyclicDeque<T>> container = new ConcurrentDictionary<int, CyclicDeque<T>>();
        private int count;
        private int hints;
        private ConcurrentDictionary<int, CyclicDeque<T>> staging = new ConcurrentDictionary<int, CyclicDeque<T>>();

        public ConcurrentBag()
        {
        }

        public ConcurrentBag(IEnumerable<T> collection)
            : this()
        {
            foreach (T item in collection)
                Add(item);
        }

        public int Count
        {
            get
            {
                return count;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return count == 0;
            }
        }

        bool System.Collections.ICollection.IsSynchronized
        {
            get
            {
                return true;
            }
        }

        object System.Collections.ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        public void Add(T item)
        {
            int index;
            CyclicDeque<T> bag = GetBag(out index);
            bag.PushBottom(item);
            staging.TryAdd(index, bag);
            AddHint(index);
            Interlocked.Increment(ref count);
        }

        public void CopyTo(T[] array, int index)
        {
            int c = count;
            if (array.Length < c + index)
                throw new InvalidOperationException("Array is not big enough");

            CopyTo(array, index, c);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        bool IProducerConsumerCollection<T>.TryAdd(T element)
        {
            Add(element);
            return true;
        }

        void System.Collections.ICollection.CopyTo(Array array, int index)
        {
            T[] a = array as T[];
            if (a == null)
                return;

            CopyTo(a, index);
        }

        public T[] ToArray()
        {
            int c = count;
            T[] temp = new T[c];

            CopyTo(temp, 0, c);

            return temp;
        }

        public bool TryPeek(out T result)
        {
            result = default(T);

            if (count == 0)
                return false;

            int hintIndex;
            CyclicDeque<T> bag = GetBag(out hintIndex, false);
            bool ret = true;

            if (bag == null || !bag.PeekBottom(out result))
            {
                var self = bag;
                ret = false;
                foreach (var other in staging)
                {
                    ret = TryGetHint(out hintIndex) && container[hintIndex].PeekTop(out result);

                    if (!ret && other.Value != self)
                        ret = other.Value.PeekTop(out result);

                    if (ret)
                        break;
                }
            }

            return ret;
        }

        public bool TryTake(out T result)
        {
            result = default(T);

            if (count == 0)
                return false;

            int hintIndex;
            CyclicDeque<T> bag = GetBag(out hintIndex, false);
            bool ret = true;

            if (bag == null || bag.PopBottom(out result) != PopResult.Succeed)
            {
                var self = bag;
                ret = false;
                foreach (var other in staging)
                {
                    ret = TryGetHint(out hintIndex) && (bag = container[hintIndex]).PopTop(out result) == PopResult.Succeed;

                    if (!ret && other.Value != self)
                    {
                        var status = other.Value.PopTop(out result);
                        while (status == PopResult.Abort)
                            status = other.Value.PopTop(out result);
                        ret = status == PopResult.Succeed;
                        hintIndex = other.Key;
                        bag = other.Value;
                    }

                    if (ret)
                        break;
                }
            }

            if (ret)
            {
                TidyBag(hintIndex, bag);
                Interlocked.Decrement(ref count);
            }

            return ret;
        }

        private void AddHint(int index)
        {
            if (index > 0xF)
                return;
            var hs = hints;
            Interlocked.CompareExchange(ref hints, (int)(((uint)hs) << 4 | (uint)index), (int)hs);
        }

        private void CopyTo(T[] array, int index, int num)
        {
            int i = index;

            foreach (T item in this)
            {
                if (i >= num)
                    break;

                array[i++] = item;
            }
        }

        private CyclicDeque<T> GetBag(out int index, bool createBag = true)
        {
            index = GetIndex();
            CyclicDeque<T> value;
            if (container.TryGetValue(index, out value))
                return value;

            return createBag ? container.GetOrAdd(index, new CyclicDeque<T>()) : null;
        }

        private IEnumerator<T> GetEnumeratorInternal()
        {
            foreach (var bag in container)
                foreach (T item in bag.Value.GetEnumerable())
                    yield return item;
        }

        private int GetIndex()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }

        private void TidyBag(int index, CyclicDeque<T> bag)
        {
            if (bag != null && bag.IsEmpty)
            {
                if (staging.TryRemove(index, out bag) && !bag.IsEmpty)
                    staging.TryAdd(index, bag);
            }
        }

        private bool TryGetHint(out int index)
        {
            var hs = hints;
            index = 0;

            if (Interlocked.CompareExchange(ref hints, (int)(((uint)hs) >> 4), hs) == hs)
                index = (int)(hs & 0xF);

            return index > 0;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorInternal();
        }
    }
}
