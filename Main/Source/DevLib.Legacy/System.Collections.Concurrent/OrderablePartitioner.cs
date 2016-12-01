using System.Collections.Generic;

namespace System.Collections.Concurrent
{
    public abstract class OrderablePartitioner<TSource> : Partitioner<TSource>
    {
        private bool keysNormalized;
        private bool keysOrderedAcrossPartitions;
        private bool keysOrderedInEachPartition;

        protected OrderablePartitioner(bool keysOrderedInEachPartition,
                                        bool keysOrderedAcrossPartitions,
                                        bool keysNormalized)
            : base()
        {
            this.keysOrderedInEachPartition = keysOrderedInEachPartition;
            this.keysOrderedAcrossPartitions = keysOrderedAcrossPartitions;
            this.keysNormalized = keysNormalized;
        }

        public bool KeysNormalized
        {
            get
            {
                return keysNormalized;
            }
        }

        public bool KeysOrderedAcrossPartitions
        {
            get
            {
                return keysOrderedAcrossPartitions;
            }
        }

        public bool KeysOrderedInEachPartition
        {
            get
            {
                return keysOrderedInEachPartition;
            }
        }

        public override IEnumerable<TSource> GetDynamicPartitions()
        {
            foreach (KeyValuePair<long, TSource> item in GetOrderableDynamicPartitions())
                yield return item.Value;
        }

        public virtual IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions()
        {
            if (!SupportsDynamicPartitions)
                throw new NotSupportedException();

            return null;
        }

        public abstract IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions(int partitionCount);

        public override IList<IEnumerator<TSource>> GetPartitions(int partitionCount)
        {
            IEnumerator<TSource>[] temp = new IEnumerator<TSource>[partitionCount];
            IList<IEnumerator<KeyValuePair<long, TSource>>> enumerators
              = GetOrderablePartitions(partitionCount);

            for (int i = 0; i < enumerators.Count; i++)
                temp[i] = new ProxyEnumerator(enumerators[i]);

            return temp;
        }

        private IEnumerator<TSource> GetProxyEnumerator(IEnumerator<KeyValuePair<long, TSource>> enumerator)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current.Value;
        }

        private class ProxyEnumerator : IEnumerator<TSource>, IDisposable
        {
            private IEnumerator<KeyValuePair<long, TSource>> internalEnumerator;

            internal ProxyEnumerator(IEnumerator<KeyValuePair<long, TSource>> enumerator)
            {
                internalEnumerator = enumerator;
            }

            public TSource Current
            {
                get;
                private set;
            }

            public void Dispose()
            {
                internalEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                if (!internalEnumerator.MoveNext())
                    return false;

                Current = internalEnumerator.Current.Value;

                return true;
            }

            public void Reset()
            {
                internalEnumerator.Reset();
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }
        }
    }
}
