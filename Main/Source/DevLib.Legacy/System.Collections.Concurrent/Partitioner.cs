using System.Collections.Generic;

namespace System.Collections.Concurrent
{
    using Partitioners;

    public static class Partitioner
    {
        public static OrderablePartitioner<TSource> Create<TSource>(IEnumerable<TSource> source)
        {
            IList<TSource> tempIList = source as IList<TSource>;
            if (tempIList != null)
                return Create(tempIList, true);

            return new EnumerablePartitioner<TSource>(source);
        }

        public static OrderablePartitioner<TSource> Create<TSource>(TSource[] array, bool loadBalance)
        {
            return Create(array, loadBalance);
        }

        public static OrderablePartitioner<TSource> Create<TSource>(IList<TSource> list, bool loadBalance)
        {
            return new ListPartitioner<TSource>(list);
        }

        public static OrderablePartitioner<Tuple<int, int>> Create(int fromInclusive,
                                                                    int toExclusive)
        {
            int rangeSize = (toExclusive - fromInclusive) / (Environment.ProcessorCount * 3);
            if (rangeSize < 1)
                rangeSize = 1;

            return Create(fromInclusive, toExclusive, rangeSize);
        }

        public static OrderablePartitioner<Tuple<int, int>> Create(int fromInclusive,
                                                                    int toExclusive,
                                                                    int rangeSize)
        {
            if (fromInclusive >= toExclusive)
                throw new ArgumentOutOfRangeException("toExclusive");
            if (rangeSize <= 0)
                throw new ArgumentOutOfRangeException("rangeSize");

            return new UserRangePartitioner(fromInclusive, toExclusive, rangeSize);
        }

        public static OrderablePartitioner<Tuple<long, long>> Create(long fromInclusive,
                                                                      long toExclusive)
        {
            long rangeSize = (toExclusive - fromInclusive) / (Environment.ProcessorCount * 3);
            if (rangeSize < 1)
                rangeSize = 1;

            return Create(fromInclusive, toExclusive, rangeSize);
        }

        public static OrderablePartitioner<Tuple<long, long>> Create(long fromInclusive,
                                                                      long toExclusive,
                                                                      long rangeSize)
        {
            if (rangeSize <= 0)
                throw new ArgumentOutOfRangeException("rangeSize");
            if (fromInclusive >= toExclusive)
                throw new ArgumentOutOfRangeException("toExclusive");

            return new UserLongRangePartitioner(fromInclusive, toExclusive, rangeSize);
        }
    }

    public abstract class Partitioner<TSource>
    {
        protected Partitioner()
        {
        }

        public virtual bool SupportsDynamicPartitions
        {
            get
            {
                return false;
            }
        }

        public virtual IEnumerable<TSource> GetDynamicPartitions()
        {
            if (!SupportsDynamicPartitions)
                throw new NotSupportedException();

            return null;
        }

        public abstract IList<IEnumerator<TSource>> GetPartitions(int partitionCount);
    }
}
