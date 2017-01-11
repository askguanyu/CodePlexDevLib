using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;

// HashSet is basically implemented as a reduction of Dictionary<K, V>

namespace System.Collections.Generic
{
    [Serializable, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    [DebuggerDisplay("Count={Count}")]
    //[DebuggerTypeProxy (typeof (CollectionDebuggerView<,>))]
    public class HashSet<T> : ICollection<T>, ISerializable, IDeserializationCallback
#if NET_4_0 || MOONLIGHT
                            , ISet<T>
#endif
    {
        private const int INITIAL_SIZE = 10;
        private const float DEFAULT_LOAD_FACTOR = (90f / 100);
        private const int NO_SLOT = -1;
        private const int HASH_FLAG = -2147483648;

        private struct Link
        {
            public int HashCode;
            public int Next;
        }

        // The hash table contains indices into the "links" array
        private int[] table;

        private Link[] links;
        private T[] slots;

        // The number of slots in "links" and "slots" that
        // are in use (i.e. filled with data) or have been used and marked as
        // "empty" later on.
        private int touched;

        // The index of the first slot in the "empty slots chain".
        // "Remove ()" prepends the cleared slots to the empty chain.
        // "Add ()" fills the first slot in the empty slots chain with the
        // added item (or increases "touched" if the chain itself is empty).
        private int empty_slot;

        // The number of items in this set.
        private int count;

        // The number of items the set can hold without
        // resizing the hash table and the slots arrays.
        private int threshold;

        private IEqualityComparer<T> comparer;
        private SerializationInfo si;

        // The number of changes made to this set. Used by enumerators
        // to detect changes and invalidate themselves.
        private int generation;

        public int Count
        {
            get { return count; }
        }

        public HashSet()
        {
            Init(INITIAL_SIZE, null);
        }

        public HashSet(IEqualityComparer<T> comparer)
        {
            Init(INITIAL_SIZE, comparer);
        }

        public HashSet(IEnumerable<T> collection)
            : this(collection, null)
        {
        }

        public HashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            int capacity = 0;
            var col = collection as ICollection<T>;
            if (col != null)
                capacity = col.Count;

            Init(capacity, comparer);
            foreach (var item in collection)
                Add(item);
        }

        protected HashSet(SerializationInfo info, StreamingContext context)
        {
            si = info;
        }

        private void Init(int capacity, IEqualityComparer<T> comparer)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("capacity");

            this.comparer = comparer ?? EqualityComparer<T>.Default;
            if (capacity == 0)
                capacity = INITIAL_SIZE;

            /* Modify capacity so 'capacity' elements can be added without resizing */
            capacity = (int)(capacity / DEFAULT_LOAD_FACTOR) + 1;

            InitArrays(capacity);
            generation = 0;
        }

        private void InitArrays(int size)
        {
            table = new int[size];

            links = new Link[size];
            empty_slot = NO_SLOT;

            slots = new T[size];
            touched = 0;

            threshold = (int)(table.Length * DEFAULT_LOAD_FACTOR);
            if (threshold == 0 && table.Length > 0)
                threshold = 1;
        }

        private bool SlotsContainsAt(int index, int hash, T item)
        {
            int current = table[index] - 1;
            while (current != NO_SLOT)
            {
                Link link = links[current];
                if (link.HashCode == hash && ((hash == HASH_FLAG && (item == null || null == slots[current])) ? (item == null && null == slots[current]) : comparer.Equals(item, slots[current])))
                    return true;

                current = link.Next;
            }

            return false;
        }

        public void CopyTo(T[] array)
        {
            CopyTo(array, 0, count);
        }

        public void CopyTo(T[] array, int index)
        {
            CopyTo(array, index, count);
        }

        public void CopyTo(T[] array, int index, int count)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            if (index > array.Length)
                throw new ArgumentException("index larger than largest valid index of array");
            if (array.Length - index < count)
                throw new ArgumentException("Destination array cannot hold the requested elements!");

            for (int i = 0, items = 0; i < touched && items < count; i++)
            {
                if (GetLinkHashCode(i) != 0)
                    array[index++] = slots[i];
            }
        }

        private void Resize()
        {
            int newSize = PrimeHelper.ToPrime((table.Length << 1) | 1);

            // allocate new hash table and link slots array
            var newTable = new int[newSize];
            var newLinks = new Link[newSize];

            for (int i = 0; i < table.Length; i++)
            {
                int current = table[i] - 1;
                while (current != NO_SLOT)
                {
                    int hashCode = newLinks[current].HashCode = GetItemHashCode(slots[current]);
                    int index = (hashCode & int.MaxValue) % newSize;
                    newLinks[current].Next = newTable[index] - 1;
                    newTable[index] = current + 1;
                    current = links[current].Next;
                }
            }

            table = newTable;
            links = newLinks;

            // allocate new data slots, copy data
            var newSlots = new T[newSize];
            Array.Copy(slots, 0, newSlots, 0, touched);
            slots = newSlots;

            threshold = (int)(newSize * DEFAULT_LOAD_FACTOR);
        }

        private int GetLinkHashCode(int index)
        {
            return links[index].HashCode & HASH_FLAG;
        }

        private int GetItemHashCode(T item)
        {
            if (item == null)
                return HASH_FLAG;
            return comparer.GetHashCode(item) | HASH_FLAG;
        }

        public bool Add(T item)
        {
            int hashCode = GetItemHashCode(item);
            int index = (hashCode & int.MaxValue) % table.Length;

            if (SlotsContainsAt(index, hashCode, item))
                return false;

            if (++count > threshold)
            {
                Resize();
                index = (hashCode & int.MaxValue) % table.Length;
            }

            // find an empty slot
            int current = empty_slot;
            if (current == NO_SLOT)
                current = touched++;
            else
                empty_slot = links[current].Next;

            // store the hash code of the added item,
            // prepend the added item to its linked list,
            // update the hash table
            links[current].HashCode = hashCode;
            links[current].Next = table[index] - 1;
            table[index] = current + 1;

            // store item
            slots[current] = item;

            generation++;

            return true;
        }

        public IEqualityComparer<T> Comparer
        {
            get { return comparer; }
        }

        public void Clear()
        {
            count = 0;

            Array.Clear(table, 0, table.Length);
            Array.Clear(slots, 0, slots.Length);
            Array.Clear(links, 0, links.Length);

            // empty the "empty slots chain"
            empty_slot = NO_SLOT;

            touched = 0;
            generation++;
        }

        public bool Contains(T item)
        {
            int hashCode = GetItemHashCode(item);
            int index = (hashCode & int.MaxValue) % table.Length;

            return SlotsContainsAt(index, hashCode, item);
        }

        public bool Remove(T item)
        {
            // get first item of linked list corresponding to given key
            int hashCode = GetItemHashCode(item);
            int index = (hashCode & int.MaxValue) % table.Length;
            int current = table[index] - 1;

            // if there is no linked list, return false
            if (current == NO_SLOT)
                return false;

            // walk linked list until right slot (and its predecessor) is
            // found or end is reached
            int prev = NO_SLOT;
            do
            {
                Link link = links[current];
                if (link.HashCode == hashCode && ((hashCode == HASH_FLAG && (item == null || null == slots[current])) ? (item == null && null == slots[current]) : comparer.Equals(slots[current], item)))
                    break;

                prev = current;
                current = link.Next;
            } while (current != NO_SLOT);

            // if we reached the end of the chain, return false
            if (current == NO_SLOT)
                return false;

            count--;

            // remove slot from linked list
            // is slot at beginning of linked list?
            if (prev == NO_SLOT)
                table[index] = links[current].Next + 1;
            else
                links[prev].Next = links[current].Next;

            // mark slot as empty and prepend it to "empty slots chain"
            links[current].Next = empty_slot;
            empty_slot = current;

            // clear slot
            links[current].HashCode = 0;
            slots[current] = default(T);

            generation++;

            return true;
        }

        public int RemoveWhere(Predicate<T> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            // int counter = 0;

            var candidates = new List<T>();

            foreach (var item in this)
                if (predicate(item))
                    candidates.Add(item);

            foreach (var item in candidates)
                Remove(item);

            return candidates.Count;
        }

        public void TrimExcess()
        {
            Resize();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            var other_set = ToSet(other);

            RemoveWhere(item => !other_set.Contains(item));
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            foreach (var item in other)
                Remove(item);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            foreach (var item in other)
                if (Contains(item))
                    return true;

            return false;
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            var other_set = ToSet(other);

            if (count != other_set.Count)
                return false;

            foreach (var item in this)
                if (!other_set.Contains(item))
                    return false;

            return true;
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            foreach (var item in ToSet(other))
                if (!Add(item))
                    Remove(item);
        }

        private HashSet<T> ToSet(IEnumerable<T> enumerable)
        {
            var set = enumerable as HashSet<T>;
            if (set == null || !Comparer.Equals(set.Comparer))
                set = new HashSet<T>(enumerable);

            return set;
        }

        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            foreach (var item in other)
                Add(item);
        }

        private bool CheckIsSubsetOf(HashSet<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            foreach (var item in this)
                if (!other.Contains(item))
                    return false;

            return true;
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (count == 0)
                return true;

            var other_set = ToSet(other);

            if (count > other_set.Count)
                return false;

            return CheckIsSubsetOf(other_set);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (count == 0)
                return true;

            var other_set = ToSet(other);

            if (count >= other_set.Count)
                return false;

            return CheckIsSubsetOf(other_set);
        }

        private bool CheckIsSupersetOf(HashSet<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            foreach (var item in other)
                if (!Contains(item))
                    return false;

            return true;
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            var other_set = ToSet(other);

            if (count < other_set.Count)
                return false;

            return CheckIsSupersetOf(other_set);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            var other_set = ToSet(other);

            if (count <= other_set.Count)
                return false;

            return CheckIsSupersetOf(other_set);
        }

        [MonoTODO]
        public static IEqualityComparer<HashSet<T>> CreateSetComparer()
        {
            throw new NotImplementedException();
        }

        [MonoTODO]
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        [MonoTODO]
        public virtual void OnDeserialization(object sender)
        {
            if (si == null)
                return;

            throw new NotImplementedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        void ICollection<T>.CopyTo(T[] array, int index)
        {
            CopyTo(array, index);
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        [Serializable]
        public struct Enumerator : IEnumerator<T>, IDisposable
        {
            private HashSet<T> hashset;
            private int next;
            private int stamp;

            private T current;

            internal Enumerator(HashSet<T> hashset)
                : this()
            {
                this.hashset = hashset;
                this.stamp = hashset.generation;
            }

            public bool MoveNext()
            {
                CheckState();

                if (next < 0)
                    return false;

                while (next < hashset.touched)
                {
                    int cur = next++;
                    if (hashset.GetLinkHashCode(cur) != 0)
                    {
                        current = hashset.slots[cur];
                        return true;
                    }
                }

                next = NO_SLOT;
                return false;
            }

            public T Current
            {
                get { return current; }
            }

            object IEnumerator.Current
            {
                get
                {
                    CheckState();
                    if (next <= 0)
                        throw new InvalidOperationException("Current is not valid");
                    return current;
                }
            }

            void IEnumerator.Reset()
            {
                CheckState();
                next = 0;
            }

            public void Dispose()
            {
                hashset = null;
            }

            private void CheckState()
            {
                if (hashset == null)
                    throw new ObjectDisposedException(null);
                if (hashset.generation != stamp)
                    throw new InvalidOperationException("HashSet have been modified while it was iterated over");
            }
        }

        // borrowed from System.Collections.HashTable
        private static class PrimeHelper
        {
            private static readonly int[] primes_table = {
                11,
                19,
                37,
                73,
                109,
                163,
                251,
                367,
                557,
                823,
                1237,
                1861,
                2777,
                4177,
                6247,
                9371,
                14057,
                21089,
                31627,
                47431,
                71143,
                106721,
                160073,
                240101,
                360163,
                540217,
                810343,
                1215497,
                1823231,
                2734867,
                4102283,
                6153409,
                9230113,
                13845163
            };

            private static bool TestPrime(int x)
            {
                if ((x & 1) != 0)
                {
                    int top = (int)Math.Sqrt(x);

                    for (int n = 3; n < top; n += 2)
                    {
                        if ((x % n) == 0)
                            return false;
                    }

                    return true;
                }

                // There is only one even prime - 2.
                return x == 2;
            }

            private static int CalcPrime(int x)
            {
                for (int i = (x & (~1)) - 1; i < Int32.MaxValue; i += 2)
                    if (TestPrime(i))
                        return i;

                return x;
            }

            public static int ToPrime(int x)
            {
                for (int i = 0; i < primes_table.Length; i++)
                    if (x <= primes_table[i])
                        return primes_table[i];

                return CalcPrime(x);
            }
        }
    }
}
