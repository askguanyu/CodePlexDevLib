using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions
{
    public sealed class MemberExpression : Expression
    {
        private Expression expression;
        private MemberInfo member;

        internal MemberExpression(Expression expression, MemberInfo member, Type type)
            : base(ExpressionType.MemberAccess, type)
        {
            this.expression = expression;
            this.member = member;
        }

        public Expression Expression
        {
            get { return expression; }
        }

        public MemberInfo Member
        {
            get { return member; }
        }

        internal override void Emit(EmitContext ec)
        {
            member.OnFieldOrProperty(
                field => EmitFieldAccess(ec, field),
                prop => EmitPropertyAccess(ec, prop));
        }

        private void EmitFieldAccess(EmitContext ec, FieldInfo field)
        {
            if (!field.IsStatic)
            {
                ec.EmitLoadSubject(expression);
                ec.ig.Emit(OpCodes.Ldfld, field);
            }
            else
                ec.ig.Emit(OpCodes.Ldsfld, field);
        }

        private void EmitPropertyAccess(EmitContext ec, PropertyInfo property)
        {
            var getter = property.GetGetMethod(true);
            if (!getter.IsStatic)
                ec.EmitLoadSubject(expression);

            ec.EmitCall(getter);
        }
    }
}
