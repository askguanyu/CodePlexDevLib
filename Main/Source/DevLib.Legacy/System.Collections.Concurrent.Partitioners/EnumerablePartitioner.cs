using System.Collections.Generic;

namespace System.Collections.Concurrent.Partitioners
{
    internal class EnumerablePartitioner<T> : OrderablePartitioner<T>
    {
        private const int InitialPartitionSize = 1;
        private const int PartitionMultiplier = 2;
        private int initialPartitionSize;
        private int partitionMultiplier;
        private IEnumerable<T> source;

        public EnumerablePartitioner(IEnumerable<T> source)
            : this(source, InitialPartitionSize, PartitionMultiplier)
        {
        }

        public EnumerablePartitioner(IEnumerable<T> source, int initialPartitionSize, int partitionMultiplier)
            : base(true, false, true)
        {
            this.source = source;
            this.initialPartitionSize = initialPartitionSize;
            this.partitionMultiplier = partitionMultiplier;
        }

        public override IList<IEnumerator<KeyValuePair<long, T>>> GetOrderablePartitions(int partitionCount)
        {
            if (partitionCount <= 0)
                throw new ArgumentOutOfRangeException("partitionCount");

            IEnumerator<KeyValuePair<long, T>>[] enumerators
                = new IEnumerator<KeyValuePair<long, T>>[partitionCount];

            PartitionerState state = new PartitionerState();
            IEnumerator<T> src = source.GetEnumerator();
            bool isSimple = initialPartitionSize == 1 && partitionMultiplier == 1;

            for (int i = 0; i < enumerators.Length; i++)
            {
                enumerators[i] = isSimple ? GetPartitionEnumeratorSimple(src, state, i == enumerators.Length - 1) : GetPartitionEnumerator(src, state);
            }

            return enumerators;
        }

        private IEnumerator<KeyValuePair<long, T>> GetPartitionEnumerator(IEnumerator<T> src, PartitionerState state)
        {
            int count = initialPartitionSize;
            List<T> list = new List<T>();

            while (!state.Finished)
            {
                list.Clear();
                long ind = -1;

                lock (state.SyncLock)
                {
                    if (state.Finished)
                        break;

                    ind = state.Index;

                    for (int i = 0; i < count; i++)
                    {
                        if (state.Finished = !src.MoveNext())
                        {
                            if (list.Count == 0)
                                yield break;
                            else
                                break;
                        }

                        list.Add(src.Current);
                        state.Index++;
                    }
                }

                for (int i = 0; i < list.Count; i++)
                    yield return new KeyValuePair<long, T>(ind + i, list[i]);

                count *= partitionMultiplier;
            }
        }

        private IEnumerator<KeyValuePair<long, T>> GetPartitionEnumeratorSimple(IEnumerator<T> src,
                                                                         PartitionerState state,
                                                                         bool last)
        {
            long index = -1;
            var value = default(T);

            try
            {
                do
                {
                    lock (state.SyncLock)
                    {
                        if (state.Finished)
                            break;
                        if (state.Finished = !src.MoveNext())
                            break;

                        index = state.Index++;
                        value = src.Current;
                    }

                    yield return new KeyValuePair<long, T>(index, value);
                } while (!state.Finished);
            }
            finally
            {
                if (last)
                    src.Dispose();
            }
        }

        private class PartitionerState
        {
            public readonly object SyncLock = new object();
            public bool Finished;
            public long Index = 0;
        }
    }
}
