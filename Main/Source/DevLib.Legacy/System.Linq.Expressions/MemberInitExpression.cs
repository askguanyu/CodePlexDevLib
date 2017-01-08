using System.Collections.ObjectModel;

namespace System.Linq.Expressions
{
    public sealed class MemberInitExpression : Expression
    {
        private ReadOnlyCollection<MemberBinding> bindings;
        private NewExpression new_expression;

        internal MemberInitExpression(NewExpression new_expression, ReadOnlyCollection<MemberBinding> bindings)
            : base(ExpressionType.MemberInit, new_expression.Type)
        {
            this.new_expression = new_expression;
            this.bindings = bindings;
        }

        public ReadOnlyCollection<MemberBinding> Bindings
        {
            get { return bindings; }
        }

        public NewExpression NewExpression
        {
            get { return new_expression; }
        }

        internal override void Emit(EmitContext ec)
        {
            var local = ec.EmitStored(new_expression);
            ec.EmitCollection(bindings, local);
            ec.EmitLoad(local);
        }
    }
}
