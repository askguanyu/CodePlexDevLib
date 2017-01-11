using System.Collections.Generic;

namespace System.Linq
{
#if NET_4_0

    public interface IQueryable<out T> : IQueryable, IEnumerable<T>
#else

    public interface IQueryable<T> : IQueryable, IEnumerable<T>
#endif
    {
    }
}
