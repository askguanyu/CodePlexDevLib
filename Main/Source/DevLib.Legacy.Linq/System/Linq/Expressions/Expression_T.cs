using System.Collections.ObjectModel;

namespace System.Linq.Expressions
{
    public sealed class Expression<TDelegate> : LambdaExpression
    {
        internal Expression(Expression body, ReadOnlyCollection<ParameterExpression> parameters)
            : base(typeof(TDelegate), body, parameters)
        {
        }

        public new TDelegate Compile()
        {
            return (TDelegate)(object)base.Compile();
        }
    }
}
