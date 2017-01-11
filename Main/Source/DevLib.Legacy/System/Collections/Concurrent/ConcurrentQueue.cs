using System.Collections.Generic;
using System.Threading;

namespace System.Collections.Concurrent
{
    [System.Diagnostics.DebuggerDisplay("Count={Count}")]
    [System.Diagnostics.DebuggerTypeProxy(typeof(CollectionDebuggerView<>))]
    public class ConcurrentQueue<T> : IProducerConsumerCollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        private int count;

        private object head = new Node();

        private object syncRoot = new object();

        private object tail;

        public ConcurrentQueue()
        {
            tail = head;
        }

        public ConcurrentQueue(IEnumerable<T> collection)
            : this()
        {
            foreach (T item in collection)
                Enqueue(item);
        }

        public int Count
        {
            get
            {
                return count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return true; }
        }

        object ICollection.SyncRoot
        {
            get { return syncRoot; }
        }

        public bool IsEmpty
        {
            get
            {
                return count == 0;
            }
        }

        private Node Head
        {
            get { return (Node)head; }
        }

        private Node Tail
        {
            get { return (Node)tail; }
        }

        public void CopyTo(T[] array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            if (index >= array.Length)
                throw new ArgumentException("index is equals or greather than array length", "index");

            IEnumerator<T> e = InternalGetEnumerator();
            int i = index;
            while (e.MoveNext())
            {
                if (i == array.Length - index)
                    throw new ArgumentException("The number of elememts in the collection exceeds the capacity of array", "array");
                array[i++] = e.Current;
            }
        }

        public void Enqueue(T item)
        {
            Node node = new Node();
            node.Value = item;

            Node oldTail = null;
            Node oldNext = null;

            bool update = false;
            while (!update)
            {
                oldTail = Tail;
                oldNext = oldTail.Next;

                if (tail == oldTail)
                {
                    if (oldNext == null)
                    {
                        update = Interlocked.CompareExchange(ref Tail.next, node, null) == null;
                    }
                    else
                    {
                        Interlocked.CompareExchange(ref tail, oldNext, oldTail);
                    }
                }
            }
            Interlocked.CompareExchange(ref tail, node, oldTail);
            Interlocked.Increment(ref count);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (array.Rank > 1)
                throw new ArgumentException("The array can't be multidimensional");
            if (array.GetLowerBound(0) != 0)
                throw new ArgumentException("The array needs to be 0-based");

            T[] dest = array as T[];
            if (dest == null)
                throw new ArgumentException("The array cannot be cast to the collection element type", "array");
            CopyTo(dest, index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return InternalGetEnumerator();
        }

        bool IProducerConsumerCollection<T>.TryAdd(T item)
        {
            Enqueue(item);
            return true;
        }

        bool IProducerConsumerCollection<T>.TryTake(out T item)
        {
            return TryDequeue(out item);
        }

        public T[] ToArray()
        {
            return new List<T>(this).ToArray();
        }

        public bool TryDequeue(out T result)
        {
            result = default(T);
            Node oldNext = null;
            bool advanced = false;

            while (!advanced)
            {
                Node oldHead = Head;
                Node oldTail = Tail;
                oldNext = oldHead.Next;

                if (oldHead == head)
                {
                    if (oldHead == oldTail)
                    {
                        if (oldNext != null)
                        {
                            Interlocked.CompareExchange(ref tail, oldNext, oldTail);
                            continue;
                        }
                        result = default(T);
                        return false;
                    }
                    else
                    {
                        result = oldNext.Value;
                        advanced = Interlocked.CompareExchange(ref head, oldNext, oldHead) == oldHead;
                    }
                }
            }

            oldNext.Value = default(T);

            Interlocked.Decrement(ref count);

            return true;
        }

        public bool TryPeek(out T result)
        {
            result = default(T);
            bool update = true;

            while (update)
            {
                Node oldHead = Head;
                Node oldNext = oldHead.Next;

                if (oldNext == null)
                {
                    result = default(T);
                    return false;
                }

                result = oldNext.Value;

                update = head != oldHead;
            }
            return true;
        }

        internal void Clear()
        {
            count = 0;
            tail = head = new Node();
        }

        private IEnumerator<T> InternalGetEnumerator()
        {
            Node my_head = Head;
            while ((my_head = my_head.Next) != null)
            {
                yield return my_head.Value;
            }
        }

        private class Node
        {
            public object next;
            public T Value;

            public Node Next
            {
                get { return (Node)next; }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)InternalGetEnumerator();
        }
    }
}
