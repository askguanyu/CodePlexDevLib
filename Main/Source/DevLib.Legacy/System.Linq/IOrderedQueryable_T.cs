namespace System.Linq
{
#if NET_4_0

    public interface IOrderedQueryable<out T> : IOrderedQueryable, IQueryable<T>
#else

    public interface IOrderedQueryable<T> : IOrderedQueryable, IQueryable<T>
#endif
    {
    }
}
