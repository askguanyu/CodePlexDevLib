using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions
{
    public sealed class MemberAssignment : MemberBinding
    {
        private Expression expression;

        internal MemberAssignment(MemberInfo member, Expression expression)
            : base(MemberBindingType.Assignment, member)
        {
            this.expression = expression;
        }

        public Expression Expression
        {
            get { return expression; }
        }

        internal override void Emit(EmitContext ec, LocalBuilder local)
        {
            this.Member.OnFieldOrProperty(
                field => EmitFieldAssignment(ec, field, local),
                prop => EmitPropertyAssignment(ec, prop, local));
        }

        private void EmitFieldAssignment(EmitContext ec, FieldInfo field, LocalBuilder local)
        {
            ec.EmitLoadSubject(local);
            expression.Emit(ec);
            ec.ig.Emit(OpCodes.Stfld, field);
        }

        private void EmitPropertyAssignment(EmitContext ec, PropertyInfo property, LocalBuilder local)
        {
            var setter = property.GetSetMethod(true);
            if (setter == null)
                throw new InvalidOperationException();

            ec.EmitLoadSubject(local);
            expression.Emit(ec);
            ec.EmitCall(setter);
        }
    }
}
