using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Linq.Expressions
{
    public sealed class MethodCallExpression : Expression
    {
        private ReadOnlyCollection<Expression> arguments;
        private MethodInfo method;
        private Expression obj;

        internal MethodCallExpression(MethodInfo method, ReadOnlyCollection<Expression> arguments)
            : base(ExpressionType.Call, method.ReturnType)
        {
            this.method = method;
            this.arguments = arguments;
        }

        internal MethodCallExpression(Expression obj, MethodInfo method, ReadOnlyCollection<Expression> arguments)
            : base(ExpressionType.Call, method.ReturnType)
        {
            this.obj = obj;
            this.method = method;
            this.arguments = arguments;
        }

        public ReadOnlyCollection<Expression> Arguments
        {
            get { return arguments; }
        }

        public MethodInfo Method
        {
            get { return method; }
        }

        public Expression Object
        {
            get { return obj; }
        }

        internal override void Emit(EmitContext ec)
        {
            ec.EmitCall(obj, arguments, method);
        }
    }
}
