using System.Collections.ObjectModel;

namespace System.Linq.Expressions
{
    public sealed class InvocationExpression : Expression
    {
        private ReadOnlyCollection<Expression> arguments;
        private Expression expression;

        internal InvocationExpression(Expression expression, Type type, ReadOnlyCollection<Expression> arguments)
            : base(ExpressionType.Invoke, type)
        {
            this.expression = expression;
            this.arguments = arguments;
        }

        public ReadOnlyCollection<Expression> Arguments
        {
            get { return arguments; }
        }

        public Expression Expression
        {
            get { return expression; }
        }

        internal override void Emit(EmitContext ec)
        {
            ec.EmitCall(expression, arguments, expression.Type.GetInvokeMethod());
        }
    }
}
