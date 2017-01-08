namespace System.Linq
{
    internal abstract class SortContext<TElement>
    {
        protected SortContext<TElement> child_context;
        protected SortDirection direction;

        protected SortContext(SortDirection direction, SortContext<TElement> child_context)
        {
            this.direction = direction;
            this.child_context = child_context;
        }

        public abstract int Compare(int first_index, int second_index);

        public abstract void Initialize(TElement[] elements);
    }
}
