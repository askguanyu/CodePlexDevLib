using System.Collections.Generic;

namespace System.Linq
{
    internal class SortSequenceContext<TElement, TKey> : SortContext<TElement>
    {
        private IComparer<TKey> comparer;
        private TKey[] keys;
        private Func<TElement, TKey> selector;

        public SortSequenceContext(Func<TElement, TKey> selector, IComparer<TKey> comparer, SortDirection direction, SortContext<TElement> child_context)
            : base(direction, child_context)
        {
            this.selector = selector;
            this.comparer = comparer;
        }

        public override int Compare(int first_index, int second_index)
        {
            int comparison = comparer.Compare(keys[first_index], keys[second_index]);

            if (comparison == 0)
            {
                if (child_context != null)
                    return child_context.Compare(first_index, second_index);

                comparison = direction == SortDirection.Descending
                    ? second_index - first_index
                    : first_index - second_index;
            }

            return direction == SortDirection.Descending ? -comparison : comparison;
        }

        public override void Initialize(TElement[] elements)
        {
            if (child_context != null)
                child_context.Initialize(elements);

            keys = new TKey[elements.Length];
            for (int i = 0; i < keys.Length; i++)
                keys[i] = selector(elements[i]);
        }
    }
}
