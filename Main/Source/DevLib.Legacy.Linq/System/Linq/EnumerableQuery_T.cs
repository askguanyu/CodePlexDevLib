#if NET_4_0 || MOONLIGHT

using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Linq
{
    public class EnumerableQuery<T> : EnumerableQuery, IOrderedQueryable<T>, IQueryable<T>, IQueryProvider
    {
        private QueryableEnumerable<T> queryable;

        public Type ElementType
        {
            get { return queryable.ElementType; }
        }

        public Expression Expression
        {
            get { return queryable.Expression; }
        }

        public IQueryProvider Provider
        {
            get { return queryable; }
        }

        public EnumerableQuery(Expression expression)
        {
            queryable = new QueryableEnumerable<T>(expression);
        }

        public EnumerableQuery(IEnumerable<T> enumerable)
        {
            queryable = new QueryableEnumerable<T>(enumerable);
        }

        public IEnumerable GetEnumerable()
        {
            return queryable.GetEnumerable();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return queryable.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return queryable.GetEnumerator();
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return queryable.CreateQuery(expression);
        }

        public object Execute(Expression expression)
        {
            return queryable.Execute(expression);
        }

        public IQueryable<TElem> CreateQuery<TElem>(Expression expression)
        {
            return new EnumerableQuery<TElem>(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return queryable.Execute<TResult>(expression);
        }

        public override string ToString()
        {
            return queryable.ToString();
        }
    }
}

#endif
