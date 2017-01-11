using System.Collections.Generic;

namespace System.Linq
{
    internal class OrderedSequence<TElement, TKey> : OrderedEnumerable<TElement>
    {
        private IComparer<TKey> comparer;
        private SortDirection direction;
        private OrderedEnumerable<TElement> parent;

        private Func<TElement, TKey> selector;

        internal OrderedSequence(IEnumerable<TElement> source, Func<TElement, TKey> key_selector, IComparer<TKey> comparer, SortDirection direction)
            : base(source)
        {
            this.selector = key_selector;
            this.comparer = comparer ?? Comparer<TKey>.Default;
            this.direction = direction;
        }

        internal OrderedSequence(OrderedEnumerable<TElement> parent, IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, SortDirection direction)
            : this(source, keySelector, comparer, direction)
        {
            this.parent = parent;
        }

        public override SortContext<TElement> CreateContext(SortContext<TElement> current)
        {
            SortContext<TElement> context = new SortSequenceContext<TElement, TKey>(selector, comparer, direction, current);

            if (parent != null)
                return parent.CreateContext(context);

            return context;
        }

        protected override IEnumerable<TElement> Sort(IEnumerable<TElement> source)
        {
            return QuickSort<TElement>.Sort(source, CreateContext(null));
        }
    }
}
