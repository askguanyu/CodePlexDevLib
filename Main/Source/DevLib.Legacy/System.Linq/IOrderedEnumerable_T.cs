using System.Collections.Generic;

namespace System.Linq
{
    public interface IOrderedEnumerable<TElement> : IEnumerable<TElement>
    {
        IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(Func<TElement, TKey> selector, IComparer<TKey> comparer, bool descending);
    }
}
