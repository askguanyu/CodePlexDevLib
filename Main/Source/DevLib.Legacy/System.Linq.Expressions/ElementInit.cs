using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions
{
    public sealed class ElementInit
    {
        private MethodInfo add_method;
        private ReadOnlyCollection<Expression> arguments;

        internal ElementInit(MethodInfo add_method, ReadOnlyCollection<Expression> arguments)
        {
            this.add_method = add_method;
            this.arguments = arguments;
        }

        public MethodInfo AddMethod
        {
            get { return add_method; }
        }

        public ReadOnlyCollection<Expression> Arguments
        {
            get { return arguments; }
        }

        public override string ToString()
        {
            return ExpressionPrinter.ToString(this);
        }

        internal void Emit(EmitContext ec, LocalBuilder local)
        {
            ec.EmitCall(local, arguments, add_method);
            EmitPopIfNeeded(ec);
        }

        private void EmitPopIfNeeded(EmitContext ec)
        {
            if (add_method.ReturnType == typeof(void))
                return;

            ec.ig.Emit(OpCodes.Pop);
        }
    }
}
