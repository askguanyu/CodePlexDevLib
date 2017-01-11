using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Linq
{
    internal interface IQueryableEnumerable : IQueryable
    {
        IEnumerable GetEnumerable();
    }

    internal interface IQueryableEnumerable<TElement> : IQueryableEnumerable, IQueryable<TElement>, IOrderedQueryable<TElement>
    {
    }

    internal class QueryableEnumerable<TElement> : IQueryableEnumerable<TElement>, IQueryProvider
    {
        private IEnumerable<TElement> enumerable;
        private Expression expression;

        public QueryableEnumerable(IEnumerable<TElement> enumerable)
        {
            this.expression = Expression.Constant(this);
            this.enumerable = enumerable;
        }

        public QueryableEnumerable(Expression expression)
        {
            this.expression = expression;
        }

        public Type ElementType
        {
            get { return typeof(TElement); }
        }

        public Expression Expression
        {
            get { return expression; }
        }

        public IQueryProvider Provider
        {
            get { return this; }
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return (IQueryable)Activator.CreateInstance(
                typeof(QueryableEnumerable<>).MakeGenericType(
                    expression.Type.GetFirstGenericArgument()), expression);
        }

        public IQueryable<TElem> CreateQuery<TElem>(Expression expression)
        {
            return new QueryableEnumerable<TElem>(expression);
        }

        public object Execute(Expression expression)
        {
            var lambda = Expression.Lambda(TransformQueryable(expression));
            return lambda.Compile().DynamicInvoke();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var lambda = Expression.Lambda<Func<TResult>>(TransformQueryable(expression));
            return lambda.Compile().Invoke();
        }

        public IEnumerable GetEnumerable()
        {
            return enumerable;
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return Execute<IEnumerable<TElement>>(expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            if (enumerable != null)
                return enumerable.ToString();

            if (expression == null)
                return base.ToString();

            var constant = expression as ConstantExpression;
            if (constant != null && constant.Value == this)
                return base.ToString();

            return expression.ToString();
        }

        private static Expression TransformQueryable(Expression expression)
        {
            return new QueryableTransformer().Transform(expression);
        }
    }
}
