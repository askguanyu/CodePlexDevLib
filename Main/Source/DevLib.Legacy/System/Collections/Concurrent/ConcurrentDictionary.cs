using System.Collections.Generic;
using System.Diagnostics;

namespace System.Collections.Concurrent
{
    [DebuggerDisplay("Count={Count}")]
    [DebuggerTypeProxy(typeof(CollectionDebuggerView<,>))]
    [Serializable]
    public class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>,
      ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>,
      IDictionary, ICollection, IEnumerable
    {
        private IEqualityComparer<TKey> comparer;

        private SplitOrderedList<TKey, KeyValuePair<TKey, TValue>> internalDictionary;

        public ConcurrentDictionary()
            : this(EqualityComparer<TKey>.Default)
        {
        }

        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : this(collection, EqualityComparer<TKey>.Default)
        {
        }

        public ConcurrentDictionary(IEqualityComparer<TKey> comparer)
        {
            this.comparer = comparer;
            this.internalDictionary = new SplitOrderedList<TKey, KeyValuePair<TKey, TValue>>(comparer);
        }

        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
            : this(comparer)
        {
            foreach (KeyValuePair<TKey, TValue> pair in collection)
                Add(pair.Key, pair.Value);
        }

        public ConcurrentDictionary(int concurrencyLevel, int capacity)
            : this(EqualityComparer<TKey>.Default)
        {
        }

        public ConcurrentDictionary(int concurrencyLevel,
                                     IEnumerable<KeyValuePair<TKey, TValue>> collection,
                                     IEqualityComparer<TKey> comparer)
            : this(collection, comparer)
        {
        }

        public ConcurrentDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
            : this(comparer)
        {
        }

        public int Count
        {
            get
            {
                return internalDictionary.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return true; }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return Count == 0;
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return (ICollection)Keys;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return (ICollection)Values;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                TValue obj;
                if (key is TKey && TryGetValue((TKey)key, out obj))
                    return obj;
                return null;
            }
            set
            {
                if (!(key is TKey) || !(value is TValue))
                    throw new ArgumentException("key or value aren't of correct type");

                this[(TKey)key] = (TValue)value;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return GetValue(key);
            }
            set
            {
                AddOrUpdate(key, value, value);
            }
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            CheckKey(key);
            if (addValueFactory == null)
                throw new ArgumentNullException("addValueFactory");
            if (updateValueFactory == null)
                throw new ArgumentNullException("updateValueFactory");
            return internalDictionary.InsertOrUpdate(Hash(key),
                                                      key,
                                                      () => Make(key, addValueFactory(key)),
                                                      (e) => Make(key, updateValueFactory(key, e.Value))).Value;
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return AddOrUpdate(key, (_) => addValue, updateValueFactory);
        }

        public void Clear()
        {
            internalDictionary = new SplitOrderedList<TKey, KeyValuePair<TKey, TValue>>(comparer);
        }

        public bool ContainsKey(TKey key)
        {
            CheckKey(key);
            KeyValuePair<TKey, TValue> dummy;
            return internalDictionary.Find(Hash(key), key, out dummy);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            CheckKey(key);
            return internalDictionary.InsertOrGet(Hash(key), key, Make(key, default(TValue)), () => Make(key, valueFactory(key))).Value;
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            CheckKey(key);
            return internalDictionary.InsertOrGet(Hash(key), key, Make(key, value), null).Value;
        }

        void ICollection.CopyTo(Array array, int startIndex)
        {
            KeyValuePair<TKey, TValue>[] arr = array as KeyValuePair<TKey, TValue>[];
            if (arr == null)
                return;

            CopyTo(arr, startIndex, Count);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> pair)
        {
            Add(pair.Key, pair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> pair)
        {
            TValue value;
            if (!TryGetValue(pair.Key, out value))
                return false;

            return EqualityComparer<TValue>.Default.Equals(value, pair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> pair)
        {
            return Remove(pair.Key);
        }

        void IDictionary.Add(object key, object value)
        {
            if (!(key is TKey) || !(value is TValue))
                throw new ArgumentException("key or value aren't of correct type");

            Add((TKey)key, (TValue)value);
        }

        bool IDictionary.Contains(object key)
        {
            if (!(key is TKey))
                return false;

            return ContainsKey((TKey)key);
        }

        void IDictionary.Remove(object key)
        {
            if (!(key is TKey))
                return;

            Remove((TKey)key);
        }

        private uint Hash(TKey key)
        {
            return (uint)comparer.GetHashCode(key);
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            return Remove(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new ConcurrentDictionaryEnumerator(GetEnumeratorInternal());
        }

        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            return new List<KeyValuePair<TKey, TValue>>(this).ToArray();
        }

        public bool TryAdd(TKey key, TValue value)
        {
            CheckKey(key);
            return internalDictionary.Insert(Hash(key), key, Make(key, value));
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            CheckKey(key);
            KeyValuePair<TKey, TValue> pair;
            bool result = internalDictionary.Find(Hash(key), key, out pair);
            value = pair.Value;

            return result;
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            CheckKey(key);
            KeyValuePair<TKey, TValue> data;
            bool result = internalDictionary.Delete(Hash(key), key, out data);
            value = data.Value;
            return result;
        }

        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            CheckKey(key);
            return internalDictionary.CompareExchange(Hash(key), key, Make(key, newValue), (e) => e.Value.Equals(comparisonValue));
        }

        private static KeyValuePair<U, V> Make<U, V>(U key, V value)
        {
            return new KeyValuePair<U, V>(key, value);
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            Add(key, value);
        }

        private TValue AddOrUpdate(TKey key, TValue addValue, TValue updateValue)
        {
            CheckKey(key);
            return internalDictionary.InsertOrUpdate(Hash(key),
                                                      key,
                                                      Make(key, addValue),
                                                      Make(key, updateValue)).Value;
        }

        private void CheckKey(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
        }

        private void Add(TKey key, TValue value)
        {
            while (!TryAdd(key, value)) ;
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int startIndex)
        {
            CopyTo(array, startIndex);
        }

        private void CopyTo(KeyValuePair<TKey, TValue>[] array, int startIndex, int num)
        {
            foreach (var kvp in this)
            {
                array[startIndex++] = kvp;

                if (--num <= 0)
                    return;
            }
        }

        private IEnumerator<KeyValuePair<TKey, TValue>> GetEnumeratorInternal()
        {
            return internalDictionary.GetEnumerator();
        }

        private ICollection<T> GetPart<T>(Func<KeyValuePair<TKey, TValue>, T> extractor)
        {
            List<T> temp = new List<T>();

            foreach (KeyValuePair<TKey, TValue> kvp in this)
                temp.Add(extractor(kvp));

            return temp.AsReadOnly();
        }

        private TValue GetValue(TKey key)
        {
            TValue temp;
            if (!TryGetValue(key, out temp))
                throw new KeyNotFoundException(key.ToString());
            return temp;
        }

        private bool Remove(TKey key)
        {
            TValue dummy;

            return TryRemove(key, out dummy);
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return GetPart<TKey>((kvp) => kvp.Key);
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return GetPart<TValue>((kvp) => kvp.Value);
            }
        }

        private void CopyTo(KeyValuePair<TKey, TValue>[] array, int startIndex)
        {
            CopyTo(array, startIndex, Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumeratorInternal();
        }

        private class ConcurrentDictionaryEnumerator : IDictionaryEnumerator
        {
            private IEnumerator<KeyValuePair<TKey, TValue>> internalEnum;

            public ConcurrentDictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> internalEnum)
            {
                this.internalEnum = internalEnum;
            }

            public object Current
            {
                get
                {
                    return Entry;
                }
            }

            public DictionaryEntry Entry
            {
                get
                {
                    KeyValuePair<TKey, TValue> current = internalEnum.Current;
                    return new DictionaryEntry(current.Key, current.Value);
                }
            }

            public object Key
            {
                get
                {
                    return internalEnum.Current.Key;
                }
            }

            public object Value
            {
                get
                {
                    return internalEnum.Current.Value;
                }
            }

            public bool MoveNext()
            {
                return internalEnum.MoveNext();
            }

            public void Reset()
            {
                internalEnum.Reset();
            }
        }
    }
}
