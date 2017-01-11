using System.Collections.ObjectModel;

namespace System.Linq.Expressions
{
    public sealed class ListInitExpression : Expression
    {
        private ReadOnlyCollection<ElementInit> initializers;
        private NewExpression new_expression;

        internal ListInitExpression(NewExpression new_expression, ReadOnlyCollection<ElementInit> initializers)
            : base(ExpressionType.ListInit, new_expression.Type)
        {
            this.new_expression = new_expression;
            this.initializers = initializers;
        }

        public ReadOnlyCollection<ElementInit> Initializers
        {
            get { return initializers; }
        }

        public NewExpression NewExpression
        {
            get { return new_expression; }
        }

        internal override void Emit(EmitContext ec)
        {
            var local = ec.EmitStored(new_expression);
            ec.EmitCollection(initializers, local);
            ec.EmitLoad(local);
        }
    }
}
