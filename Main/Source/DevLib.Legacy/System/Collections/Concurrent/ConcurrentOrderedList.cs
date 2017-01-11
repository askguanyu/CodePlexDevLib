using System.Collections.Generic;
using System.Threading;

namespace System.Collections.Concurrent
{
    internal class ConcurrentOrderedList<T> : ICollection<T>, IEnumerable<T>
    {
        private IEqualityComparer<T> comparer;

        private int count;

        private Node head;

        private Node tail;

        public ConcurrentOrderedList()
            : this(EqualityComparer<T>.Default)
        {
        }

        public ConcurrentOrderedList(IEqualityComparer<T> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            this.comparer = comparer;

            head = new Node();
            tail = new Node();
            head.next = tail;
        }

        public IEqualityComparer<T> Comparer
        {
            get
            {
                return comparer;
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Clear()
        {
            head.next = tail;
        }

        public bool Contains(T data)
        {
            return ContainsHash(comparer.GetHashCode(data));
        }

        public bool ContainsHash(int key)
        {
            Node node;

            if (!ListFind(key, out node))
                return false;

            return true;
        }

        public void CopyTo(T[] array, int startIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex");
            if (count > array.Length - startIndex)
                throw new ArgumentException("array", "The number of elements is greater than the available space from startIndex to the end of the destination array.");

            foreach (T item in this)
            {
                if (startIndex >= array.Length)
                    break;

                array[startIndex++] = item;
            }
        }

        void ICollection<T>.Add(T item)
        {
            TryAdd(item);
        }

        bool ICollection<T>.Remove(T item)
        {
            return TryRemove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        public bool TryAdd(T data)
        {
            Node node = new Node();
            node.Data = data;
            node.Key = comparer.GetHashCode(data);

            if (ListInsert(node))
            {
                Interlocked.Increment(ref count);
                return true;
            }

            return false;
        }

        public bool TryGetFromHash(int key, out T data)
        {
            data = default(T);
            Node node;

            if (!ListFind(key, out node))
                return false;

            data = node.Data;
            return true;
        }

        public bool TryPop(out T data)
        {
            return ListPop(out data);
        }

        public bool TryRemove(T data)
        {
            T dummy;
            return TryRemoveHash(comparer.GetHashCode(data), out dummy);
        }

        public bool TryRemoveHash(int key, out T data)
        {
            if (ListDelete(key, out data))
            {
                Interlocked.Decrement(ref count);
                return true;
            }

            return false;
        }

        private IEnumerator<T> GetEnumeratorInternal()
        {
            Node node = head.Next;

            while (node != tail)
            {
                while (node.Marked)
                {
                    node = node.Next;
                    if (node == tail)
                        yield break;
                }
                yield return node.Data;
                node = node.Next;
            }
        }

        private bool ListDelete(int key, out T data)
        {
            Node rightNode = null, rightNodeNext = null, leftNode = null;
            data = default(T);

            do
            {
                rightNode = ListSearch(key, ref leftNode);
                if (rightNode == tail || rightNode.Key != key)
                    return false;

                data = rightNode.Data;

                rightNodeNext = rightNode.Next;
                if (!rightNodeNext.Marked)
                    if (Interlocked.CompareExchange(ref rightNode.next, new Node(rightNodeNext), rightNodeNext) == rightNodeNext)
                        break;
            } while (true);

            if (Interlocked.CompareExchange(ref leftNode.next, rightNodeNext, rightNode) != rightNodeNext)
                ListSearch(rightNode.Key, ref leftNode);

            return true;
        }

        private bool ListFind(int key, out Node data)
        {
            Node rightNode = null, leftNode = null;
            data = null;

            data = rightNode = ListSearch(key, ref leftNode);

            return rightNode != tail && rightNode.Key == key;
        }

        private bool ListInsert(Node newNode)
        {
            int key = newNode.Key;
            Node rightNode = null, leftNode = null;

            do
            {
                rightNode = ListSearch(key, ref leftNode);
                if (rightNode != tail && rightNode.Key == key)
                    return false;

                newNode.next = rightNode;
                if (Interlocked.CompareExchange(ref leftNode.next, newNode, rightNode) == rightNode)
                    return true;
            } while (true);
        }

        private bool ListPop(out T data)
        {
            Node rightNode = null, rightNodeNext = null, leftNode = head;
            data = default(T);

            do
            {
                rightNode = head.Next;
                if (rightNode == tail)
                    return false;

                data = rightNode.Data;

                rightNodeNext = rightNode.Next;
                if (!rightNodeNext.Marked)
                    if (Interlocked.CompareExchange(ref rightNode.next, new Node(rightNodeNext), rightNodeNext) == rightNodeNext)
                        break;
            } while (true);

            if (Interlocked.CompareExchange(ref leftNode.next, rightNodeNext, rightNode) != rightNodeNext)
                ListSearch(rightNode.Key, ref leftNode);

            return true;
        }

        private Node ListSearch(int key, ref Node left)
        {
            Node leftNodeNext = null, rightNode = null;

            do
            {
                Node t = head;
                Node tNext = t.Next;
                do
                {
                    if (!tNext.Marked)
                    {
                        left = t;
                        leftNodeNext = tNext;
                    }
                    t = tNext.Marked ? tNext.Next : tNext;
                    if (t == tail)
                        break;

                    tNext = t.Next;
                } while (tNext.Marked || t.Key < key);

                rightNode = t;

                if (leftNodeNext == rightNode)
                {
                    if (rightNode != tail && rightNode.Next.Marked)
                        continue;
                    else
                        return rightNode;
                }

                if (Interlocked.CompareExchange(ref left.next, rightNode, leftNodeNext) == leftNodeNext)
                {
                    if (rightNode != tail && rightNode.Next.Marked)
                        continue;
                    else
                        return rightNode;
                }
            } while (true);
        }

        private class Node
        {
            public T Data;
            public int Key;
            public bool Marked;
            public object next;

            public Node()
            {
            }

            public Node(Node wrapped)
            {
                Marked = true;
                next = wrapped;
            }

            public Node Next
            {
                get { return (Node)next; }
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumeratorInternal();
        }
    }
}
