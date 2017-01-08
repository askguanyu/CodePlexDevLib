using System.Collections.Generic;

namespace System.Linq
{
#if NET_4_0

    public interface IGrouping<out TKey, out TElement> : IEnumerable<TElement>
#else

    public interface IGrouping<TKey, TElement> : IEnumerable<TElement>
#endif
    {
        TKey Key { get; }
    }
}
