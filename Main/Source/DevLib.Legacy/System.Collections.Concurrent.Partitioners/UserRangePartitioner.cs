using System.Collections.Generic;
using System.Threading;

namespace System.Collections.Concurrent.Partitioners
{
    internal class UserLongRangePartitioner : OrderablePartitioner<Tuple<long, long>>
    {
        private readonly long end;
        private readonly long rangeSize;
        private readonly long start;

        public UserLongRangePartitioner(long start, long end, long rangeSize)
            : base(true, true, true)
        {
            this.start = start;
            this.end = end;
            this.rangeSize = rangeSize;
        }

        public override IList<IEnumerator<KeyValuePair<long, Tuple<long, long>>>> GetOrderablePartitions(int partitionCount)
        {
            if (partitionCount <= 0)
                throw new ArgumentOutOfRangeException("partitionCount");

            long currentIndex = 0;
            Func<long> getNextIndex = () => Interlocked.Increment(ref currentIndex) - 1;

            var enumerators = new IEnumerator<KeyValuePair<long, Tuple<long, long>>>[partitionCount];
            for (int i = 0; i < partitionCount; i++)
                enumerators[i] = GetEnumerator(getNextIndex);

            return enumerators;
        }

        private IEnumerator<KeyValuePair<long, Tuple<long, long>>> GetEnumerator(Func<long> getNextIndex)
        {
            while (true)
            {
                long index = getNextIndex();
                long sliceStart = index * rangeSize + start;

                if (sliceStart >= end)
                    break;

                yield return new KeyValuePair<long, Tuple<long, long>>(index, Tuple.Create(sliceStart, Math.Min(end, sliceStart + rangeSize)));
                sliceStart += rangeSize;
            }
        }
    }

    internal class UserRangePartitioner : OrderablePartitioner<Tuple<int, int>>
    {
        private readonly int end;
        private readonly int rangeSize;
        private readonly int start;

        public UserRangePartitioner(int start, int end, int rangeSize)
            : base(true, true, true)
        {
            this.start = start;
            this.end = end;
            this.rangeSize = rangeSize;
        }

        public override IList<IEnumerator<KeyValuePair<long, Tuple<int, int>>>> GetOrderablePartitions(int partitionCount)
        {
            if (partitionCount <= 0)
                throw new ArgumentOutOfRangeException("partitionCount");

            int currentIndex = 0;
            Func<int> getNextIndex = () => Interlocked.Increment(ref currentIndex) - 1;

            var enumerators = new IEnumerator<KeyValuePair<long, Tuple<int, int>>>[partitionCount];
            for (int i = 0; i < partitionCount; i++)
                enumerators[i] = GetEnumerator(getNextIndex);

            return enumerators;
        }

        private IEnumerator<KeyValuePair<long, Tuple<int, int>>> GetEnumerator(Func<int> getNextIndex)
        {
            while (true)
            {
                int index = getNextIndex();
                int sliceStart = index * rangeSize + start;

                if (sliceStart >= end)
                    break;

                yield return new KeyValuePair<long, Tuple<int, int>>(index, Tuple.Create(sliceStart, Math.Min(end, sliceStart + rangeSize)));
                sliceStart += rangeSize;
            }
        }
    }
}
