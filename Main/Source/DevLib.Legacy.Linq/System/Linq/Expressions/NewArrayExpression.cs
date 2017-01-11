using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions
{
    public sealed class NewArrayExpression : Expression
    {
        private ReadOnlyCollection<Expression> expressions;

        internal NewArrayExpression(ExpressionType et, Type type, ReadOnlyCollection<Expression> expressions)
            : base(et, type)
        {
            this.expressions = expressions;
        }

        public ReadOnlyCollection<Expression> Expressions
        {
            get { return expressions; }
        }

        internal override void Emit(EmitContext ec)
        {
            var type = this.Type.GetElementType();

            switch (this.NodeType)
            {
                case ExpressionType.NewArrayInit:
                    EmitNewArrayInit(ec, type);
                    return;

                case ExpressionType.NewArrayBounds:
                    EmitNewArrayBounds(ec, type);
                    return;

                default:
                    throw new NotSupportedException();
            }
        }

        private static Type CreateArray(Type type, int rank)
        {
            return type.MakeArrayType(rank);
        }

        private static Type[] CreateTypeParameters(int rank)
        {
            return Enumerable.Repeat(typeof(int), rank).ToArray();
        }

        private static ConstructorInfo GetArrayConstructor(Type type, int rank)
        {
            return CreateArray(type, rank).GetConstructor(CreateTypeParameters(rank));
        }

        private void EmitNewArrayBounds(EmitContext ec, Type type)
        {
            int rank = expressions.Count;

            ec.EmitCollection(expressions);

            if (rank == 1)
            {
                ec.ig.Emit(OpCodes.Newarr, type);
                return;
            }

            ec.ig.Emit(OpCodes.Newobj, GetArrayConstructor(type, rank));
        }

        private void EmitNewArrayInit(EmitContext ec, Type type)
        {
            var size = expressions.Count;

            ec.ig.Emit(OpCodes.Ldc_I4, size);
            ec.ig.Emit(OpCodes.Newarr, type);

            for (int i = 0; i < size; i++)
            {
                ec.ig.Emit(OpCodes.Dup);
                ec.ig.Emit(OpCodes.Ldc_I4, i);
                expressions[i].Emit(ec);
                ec.ig.Emit(OpCodes.Stelem, type);
            }
        }
    }
}
