using System.Reflection.Emit;

namespace System.Linq.Expressions
{
    public sealed class ConditionalExpression : Expression
    {
        private Expression if_false;
        private Expression if_true;
        private Expression test;

        internal ConditionalExpression(Expression test, Expression if_true, Expression if_false)
            : base(ExpressionType.Conditional, if_true.Type)
        {
            this.test = test;
            this.if_true = if_true;
            this.if_false = if_false;
        }

        public Expression IfFalse
        {
            get { return if_false; }
        }

        public Expression IfTrue
        {
            get { return if_true; }
        }

        public Expression Test
        {
            get { return test; }
        }

        internal override void Emit(EmitContext ec)
        {
            var ig = ec.ig;
            var false_target = ig.DefineLabel();
            var end_target = ig.DefineLabel();

            test.Emit(ec);
            ig.Emit(OpCodes.Brfalse, false_target);

            if_true.Emit(ec);
            ig.Emit(OpCodes.Br, end_target);

            ig.MarkLabel(false_target);
            if_false.Emit(ec);

            ig.MarkLabel(end_target);
        }
    }
}
