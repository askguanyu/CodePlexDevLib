using System.Reflection.Emit;

namespace System.Linq.Expressions
{
    public sealed class ParameterExpression : Expression
    {
        private string name;

        internal ParameterExpression(Type type, string name)
            : base(ExpressionType.Parameter, type)
        {
            this.name = name;
        }

        public string Name
        {
            get { return name; }
        }

        internal override void Emit(EmitContext ec)
        {
            int position = -1;
            if (ec.IsLocalParameter(this, ref position))
            {
                EmitLocalParameter(ec, position);
                return;
            }

            int level = 0;
            if (ec.IsHoistedLocal(this, ref level, ref position))
            {
                EmitHoistedLocal(ec, level, position);
                return;
            }

            throw new InvalidOperationException("Parameter out of scope");
        }

        private void EmitHoistedLocal(EmitContext ec, int level, int position)
        {
            ec.EmitScope();

            for (int i = 0; i < level; i++)
                ec.EmitParentScope();

            ec.EmitLoadLocals();

            ec.ig.Emit(OpCodes.Ldc_I4, position);
            ec.ig.Emit(OpCodes.Ldelem, typeof(object));

            ec.EmitLoadStrongBoxValue(Type);
        }

        private void EmitLocalParameter(EmitContext ec, int position)
        {
            ec.ig.Emit(OpCodes.Ldarg, position);
        }
    }
}
