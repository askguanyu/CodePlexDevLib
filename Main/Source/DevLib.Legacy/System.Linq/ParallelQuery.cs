#if NET_4_0

using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    public class ParallelQuery : IEnumerable
    {
        private ParallelExecutionMode execMode = ParallelExecutionMode.Default;
        private ParallelMergeOptions mergeOptions = ParallelMergeOptions.Default;

        internal ParallelQuery()
        {
        }

        internal ParallelMergeOptions MergeOptions
        {
            get
            {
                return mergeOptions;
            }
            set
            {
                mergeOptions = value;
            }
        }

        internal ParallelExecutionMode ExecMode
        {
            get
            {
                return execMode;
            }
            set
            {
                execMode = value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorTrick();
        }

        // Trick to get the correct IEnumerator from ParallelQuery<TSource>
        internal virtual IEnumerator GetEnumeratorTrick()
        {
            return null;
        }

        internal virtual ParallelQuery<object> TypedQuery
        {
            get
            {
                return null;
            }
        }
    }

    public class ParallelQuery<TSource> : ParallelQuery, IEnumerable<TSource>, IEnumerable
    {
        private QueryBaseNode<TSource> node;

        internal ParallelQuery(QueryBaseNode<TSource> node)
        {
            this.node = node;
        }

        internal QueryBaseNode<TSource> Node
        {
            get
            {
                return node;
            }
        }

        public virtual IEnumerator<TSource> GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumeratorInternal();
        }

        private IEnumerator<TSource> GetEnumeratorInternal()
        {
            return new ParallelQueryEnumerator<TSource>(node);
        }

        internal override IEnumerator GetEnumeratorTrick()
        {
            return (IEnumerator)GetEnumeratorInternal();
        }

        internal override ParallelQuery<object> TypedQuery
        {
            get
            {
                return new ParallelQuery<object>(new QueryCastNode<TSource>(node));
            }
        }
    }
}

#endif
