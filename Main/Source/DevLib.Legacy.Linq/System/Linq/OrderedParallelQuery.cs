#if NET_4_0

using System.Collections.Generic;

namespace System.Linq
{
    // Only returned after OrderBy and ThenBy.
    public class OrderedParallelQuery<TSource> : ParallelQuery<TSource>
    {
        private QueryOrderByNode<TSource> node;

        internal OrderedParallelQuery(QueryOrderByNode<TSource> node)
            : base(node)
        {
            this.node = node;
        }

        internal new QueryOrderByNode<TSource> Node
        {
            get
            {
                return node;
            }
        }

        public override IEnumerator<TSource> GetEnumerator()
        {
            return base.GetEnumerator();
        }
    }
}

#endif
