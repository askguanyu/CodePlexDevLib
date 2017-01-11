using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    internal abstract class OrderedEnumerable<TElement> : IOrderedEnumerable<TElement>
    {
        private IEnumerable<TElement> source;

        protected OrderedEnumerable(IEnumerable<TElement> source)
        {
            this.source = source;
        }

        public abstract SortContext<TElement> CreateContext(SortContext<TElement> current);

        public IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(
            Func<TElement, TKey> selector, IComparer<TKey> comparer, bool descending)
        {
            return new OrderedSequence<TElement, TKey>(this, source, selector, comparer,
                descending ? SortDirection.Descending : SortDirection.Ascending);
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return Sort(source).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected abstract IEnumerable<TElement> Sort(IEnumerable<TElement> source);
    }
}
