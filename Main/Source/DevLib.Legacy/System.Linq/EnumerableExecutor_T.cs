#if NET_4_0 || MOONLIGHT

using System.Linq.Expressions;

namespace System.Linq
{
    public class EnumerableExecutor<T> : EnumerableExecutor
    {
        public EnumerableExecutor(Expression expression)
        {
        }
    }
}

#endif
