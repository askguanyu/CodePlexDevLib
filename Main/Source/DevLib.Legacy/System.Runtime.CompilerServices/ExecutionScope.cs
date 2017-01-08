using System.Linq.Expressions;

namespace System.Runtime.CompilerServices
{
#if MOONLIGHT

    [Obsolete("do not use this type", true)]
#endif

    public class ExecutionScope
    {
        public object[] Globals;
        public object[] Locals;
        public ExecutionScope Parent;

#if !MOONLIGHT
        internal CompilationContext context;
#endif
        internal int compilation_unit;

#if !MOONLIGHT

        private ExecutionScope(CompilationContext context, int compilation_unit)
        {
            this.context = context;
            this.compilation_unit = compilation_unit;
            this.Globals = context.GetGlobals();
        }

        internal ExecutionScope(CompilationContext context)
            : this(context, 0)
        {
        }

        internal ExecutionScope(CompilationContext context, int compilation_unit, ExecutionScope parent, object[] locals)
            : this(context, compilation_unit)
        {
            this.Parent = parent;
            this.Locals = locals;
        }

#endif

        public Delegate CreateDelegate(int indexLambda, object[] locals)
        {
#if MOONLIGHT
            throw new NotSupportedException();
#else
            return context.CreateDelegate(
                indexLambda,
                new ExecutionScope(context, indexLambda, this, locals));
#endif
        }

        public object[] CreateHoistedLocals()
        {
#if MOONLIGHT
            throw new NotSupportedException();
#else
            return context.CreateHoistedLocals(compilation_unit);
#endif
        }

        public Expression IsolateExpression(Expression expression, object[] locals)
        {
#if MOONLIGHT
            throw new NotSupportedException();
#else
            return context.IsolateExpression(this, locals, expression);
#endif
        }
    }
}
