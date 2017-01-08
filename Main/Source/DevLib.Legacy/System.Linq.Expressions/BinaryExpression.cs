using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions
{
    public sealed class BinaryExpression : Expression
    {
        private LambdaExpression conversion;
        private Expression left;
        private bool lift_to_null, is_lifted;
        private MethodInfo method;
        private Expression right;

        internal BinaryExpression(ExpressionType node_type, Type type, Expression left, Expression right)
            : base(node_type, type)
        {
            this.left = left;
            this.right = right;
        }

        internal BinaryExpression(ExpressionType node_type, Type type, Expression left, Expression right, MethodInfo method)
            : base(node_type, type)
        {
            this.left = left;
            this.right = right;
            this.method = method;
        }

        internal BinaryExpression(ExpressionType node_type, Type type, Expression left, Expression right, bool lift_to_null,
            bool is_lifted, MethodInfo method, LambdaExpression conversion) : base(node_type, type)
        {
            this.left = left;
            this.right = right;
            this.method = method;
            this.conversion = conversion;
            this.lift_to_null = lift_to_null;
            this.is_lifted = is_lifted;
        }

        public LambdaExpression Conversion
        {
            get { return conversion; }
        }

        public bool IsLifted
        {
            get { return is_lifted; }
        }

        public bool IsLiftedToNull
        {
            get { return lift_to_null; }
        }

        public Expression Left
        {
            get { return left; }
        }

        public MethodInfo Method
        {
            get { return method; }
        }

        public Expression Right
        {
            get { return right; }
        }

        internal override void Emit(EmitContext ec)
        {
            if (method != null)
            {
                EmitUserDefinedOperator(ec);
                return;
            }

            switch (NodeType)
            {
                case ExpressionType.ArrayIndex:
                    EmitArrayAccess(ec);
                    return;

                case ExpressionType.Coalesce:
                    if (conversion != null)
                        EmitConvertedCoalesce(ec);
                    else
                        EmitCoalesce(ec);
                    return;

                case ExpressionType.Power:
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Divide:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.LeftShift:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    EmitArithmeticBinary(ec);
                    return;

                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                    EmitRelationalBinary(ec);
                    return;

                case ExpressionType.And:
                case ExpressionType.Or:
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    EmitLogicalBinary(ec);
                    return;

                default:
                    throw new NotSupportedException(this.NodeType.ToString());
            }
        }

        private static bool IsInt32OrInt64(Type type)
        {
            return type == typeof(int) || type == typeof(long);
        }

        private static bool IsSingleOrDouble(Type type)
        {
            return type == typeof(float) || type == typeof(double);
        }

        private void EmitArithmeticBinary(EmitContext ec)
        {
            if (!IsLifted)
                EmitNonLiftedBinary(ec);
            else
                EmitLiftedArithmeticBinary(ec);
        }

        private void EmitArrayAccess(EmitContext ec)
        {
            left.Emit(ec);
            right.Emit(ec);
            ec.ig.Emit(OpCodes.Ldelem, this.Type);
        }

        private void EmitBinaryOperator(EmitContext ec)
        {
            var ig = ec.ig;
            bool is_unsigned = IsUnsigned(left.Type);

            switch (NodeType)
            {
                case ExpressionType.Add:
                    ig.Emit(OpCodes.Add);
                    break;

                case ExpressionType.AddChecked:
                    if (IsInt32OrInt64(left.Type))
                        ig.Emit(OpCodes.Add_Ovf);
                    else
                        ig.Emit(is_unsigned ? OpCodes.Add_Ovf_Un : OpCodes.Add);
                    break;

                case ExpressionType.Subtract:
                    ig.Emit(OpCodes.Sub);
                    break;

                case ExpressionType.SubtractChecked:
                    if (IsInt32OrInt64(left.Type))
                        ig.Emit(OpCodes.Sub_Ovf);
                    else
                        ig.Emit(is_unsigned ? OpCodes.Sub_Ovf_Un : OpCodes.Sub);
                    break;

                case ExpressionType.Multiply:
                    ig.Emit(OpCodes.Mul);
                    break;

                case ExpressionType.MultiplyChecked:
                    if (IsInt32OrInt64(left.Type))
                        ig.Emit(OpCodes.Mul_Ovf);
                    else
                        ig.Emit(is_unsigned ? OpCodes.Mul_Ovf_Un : OpCodes.Mul);
                    break;

                case ExpressionType.Divide:
                    ig.Emit(is_unsigned ? OpCodes.Div_Un : OpCodes.Div);
                    break;

                case ExpressionType.Modulo:
                    ig.Emit(is_unsigned ? OpCodes.Rem_Un : OpCodes.Rem);
                    break;

                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                    ig.Emit(OpCodes.Ldc_I4, left.Type == typeof(int) ? 0x1f : 0x3f);
                    ig.Emit(OpCodes.And);
                    if (NodeType == ExpressionType.RightShift)
                        ig.Emit(is_unsigned ? OpCodes.Shr_Un : OpCodes.Shr);
                    else
                        ig.Emit(OpCodes.Shl);
                    break;

                case ExpressionType.And:
                    ig.Emit(OpCodes.And);
                    break;

                case ExpressionType.Or:
                    ig.Emit(OpCodes.Or);
                    break;

                case ExpressionType.ExclusiveOr:
                    ig.Emit(OpCodes.Xor);
                    break;

                case ExpressionType.GreaterThan:
                    ig.Emit(is_unsigned ? OpCodes.Cgt_Un : OpCodes.Cgt);
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    if (is_unsigned || IsSingleOrDouble(left.Type))
                        ig.Emit(OpCodes.Clt_Un);
                    else
                        ig.Emit(OpCodes.Clt);

                    ig.Emit(OpCodes.Ldc_I4_0);
                    ig.Emit(OpCodes.Ceq);
                    break;

                case ExpressionType.LessThan:
                    ig.Emit(is_unsigned ? OpCodes.Clt_Un : OpCodes.Clt);
                    break;

                case ExpressionType.LessThanOrEqual:
                    if (is_unsigned || IsSingleOrDouble(left.Type))
                        ig.Emit(OpCodes.Cgt_Un);
                    else
                        ig.Emit(OpCodes.Cgt);

                    ig.Emit(OpCodes.Ldc_I4_0);
                    ig.Emit(OpCodes.Ceq);
                    break;

                case ExpressionType.Equal:
                    ig.Emit(OpCodes.Ceq);
                    break;

                case ExpressionType.NotEqual:
                    ig.Emit(OpCodes.Ceq);
                    ig.Emit(OpCodes.Ldc_I4_0);
                    ig.Emit(OpCodes.Ceq);
                    break;

                case ExpressionType.Power:
                    ig.Emit(OpCodes.Call, typeof(Math).GetMethod("Pow"));
                    break;

                default:
                    throw new InvalidOperationException(
                        string.Format("Internal error: BinaryExpression contains non-Binary nodetype {0}", NodeType));
            }
        }

        private void EmitCoalesce(EmitContext ec)
        {
            var ig = ec.ig;
            var done = ig.DefineLabel();
            var load_right = ig.DefineLabel();

            var left = ec.EmitStored(this.left);
            var left_is_nullable = left.LocalType.IsNullable();

            if (left_is_nullable)
                ec.EmitNullableHasValue(left);
            else
                ec.EmitLoad(left);

            ig.Emit(OpCodes.Brfalse, load_right);

            if (left_is_nullable && !Type.IsNullable())
                ec.EmitNullableGetValue(left);
            else
                ec.EmitLoad(left);

            ig.Emit(OpCodes.Br, done);

            ig.MarkLabel(load_right);
            ec.Emit(this.right);

            ig.MarkLabel(done);
        }

        private void EmitConvertedCoalesce(EmitContext ec)
        {
            var ig = ec.ig;
            var done = ig.DefineLabel();
            var load_right = ig.DefineLabel();

            var left = ec.EmitStored(this.left);

            if (left.LocalType.IsNullable())
                ec.EmitNullableHasValue(left);
            else
                ec.EmitLoad(left);

            ig.Emit(OpCodes.Brfalse, load_right);

            ec.Emit(conversion);
            ec.EmitLoad(left);
            ig.Emit(OpCodes.Callvirt, conversion.Type.GetInvokeMethod());

            ig.Emit(OpCodes.Br, done);

            ig.MarkLabel(load_right);
            ec.Emit(this.right);

            ig.MarkLabel(done);
        }

        private void EmitLeftLiftedToNullBinary(EmitContext ec)
        {
            var ig = ec.ig;

            var ret = ig.DefineLabel();
            var done = ig.DefineLabel();

            var left = ec.EmitStored(this.left);

            ec.EmitNullableHasValue(left);
            ig.Emit(OpCodes.Brfalse, ret);

            ec.EmitNullableGetValueOrDefault(left);
            ec.Emit(right);

            EmitBinaryOperator(ec);

            ec.EmitNullableNew(Type);

            ig.Emit(OpCodes.Br, done);

            ig.MarkLabel(ret);

            var temp = ig.DeclareLocal(Type);
            ec.EmitNullableInitialize(temp);

            ig.MarkLabel(done);
        }

        private void EmitLiftedArithmeticBinary(EmitContext ec)
        {
            if (IsLeftLiftedBinary())
                EmitLeftLiftedToNullBinary(ec);
            else
                EmitLiftedToNullBinary(ec);
        }

        private void EmitLiftedLogical(EmitContext ec)
        {
            var ig = ec.ig;
            var and = NodeType == ExpressionType.And;
            var left = ec.EmitStored(this.left);
            var right = ec.EmitStored(this.right);

            var ret_from_left = ig.DefineLabel();
            var ret_from_right = ig.DefineLabel();
            var done = ig.DefineLabel();

            ec.EmitNullableGetValueOrDefault(left);
            ig.Emit(OpCodes.Brtrue, ret_from_left);
            ec.EmitNullableGetValueOrDefault(right);
            ig.Emit(OpCodes.Brtrue, ret_from_right);

            ec.EmitNullableHasValue(left);
            ig.Emit(OpCodes.Brfalse, ret_from_left);

            ig.MarkLabel(ret_from_right);
            ec.EmitLoad(and ? left : right);
            ig.Emit(OpCodes.Br, done);

            ig.MarkLabel(ret_from_left);
            ec.EmitLoad(and ? right : left);

            ig.MarkLabel(done);
        }

        private void EmitLiftedLogicalShortCircuit(EmitContext ec)
        {
            var ig = ec.ig;
            var and = NodeType == ExpressionType.AndAlso;
            var left_is_null = ig.DefineLabel();
            var ret_from_left = ig.DefineLabel();
            var ret_null = ig.DefineLabel();
            var ret_new = ig.DefineLabel();
            var done = ig.DefineLabel();

            var left = ec.EmitStored(this.left);

            ec.EmitNullableHasValue(left);
            ig.Emit(OpCodes.Brfalse, left_is_null);

            ec.EmitNullableGetValueOrDefault(left);

            ig.Emit(OpCodes.Ldc_I4_0);
            ig.Emit(OpCodes.Ceq);
            ig.Emit(and ? OpCodes.Brtrue : OpCodes.Brfalse, ret_from_left);

            ig.MarkLabel(left_is_null);
            var right = ec.EmitStored(this.right);

            ec.EmitNullableHasValue(right);
            ig.Emit(OpCodes.Brfalse_S, ret_null);

            ec.EmitNullableGetValueOrDefault(right);

            ig.Emit(OpCodes.Ldc_I4_0);
            ig.Emit(OpCodes.Ceq);

            ig.Emit(and ? OpCodes.Brtrue : OpCodes.Brfalse, ret_from_left);

            ec.EmitNullableHasValue(left);
            ig.Emit(OpCodes.Brfalse, ret_null);

            ig.Emit(and ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            ig.Emit(OpCodes.Br_S, ret_new);

            ig.MarkLabel(ret_from_left);
            ig.Emit(and ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);

            ig.MarkLabel(ret_new);
            ec.EmitNullableNew(Type);
            ig.Emit(OpCodes.Br, done);

            ig.MarkLabel(ret_null);
            var ret = ig.DeclareLocal(Type);
            ec.EmitNullableInitialize(ret);

            ig.MarkLabel(done);
        }

        private void EmitLiftedRelationalBinary(EmitContext ec)
        {
            var ig = ec.ig;
            var left = ec.EmitStored(this.left);
            var right = ec.EmitStored(this.right);

            var ret = ig.DefineLabel();
            var done = ig.DefineLabel();

            ec.EmitNullableGetValueOrDefault(left);
            ec.EmitNullableGetValueOrDefault(right);

            switch (NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    ig.Emit(OpCodes.Bne_Un, ret);
                    break;

                default:
                    EmitBinaryOperator(ec);
                    ig.Emit(OpCodes.Brfalse, ret);
                    break;
            }

            ec.EmitNullableHasValue(left);
            ec.EmitNullableHasValue(right);

            switch (NodeType)
            {
                case ExpressionType.Equal:
                    ig.Emit(OpCodes.Ceq);
                    break;

                case ExpressionType.NotEqual:
                    ig.Emit(OpCodes.Ceq);
                    ig.Emit(OpCodes.Ldc_I4_0);
                    ig.Emit(OpCodes.Ceq);
                    break;

                default:
                    ig.Emit(OpCodes.And);
                    break;
            }

            ig.Emit(OpCodes.Br, done);

            ig.MarkLabel(ret);

            ig.Emit(NodeType == ExpressionType.NotEqual ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);

            ig.MarkLabel(done);
        }

        private void EmitLiftedToNullBinary(EmitContext ec)
        {
            var ig = ec.ig;
            var left = ec.EmitStored(this.left);
            var right = ec.EmitStored(this.right);
            var result = ig.DeclareLocal(Type);

            var has_value = ig.DefineLabel();
            var done = ig.DefineLabel();

            ec.EmitNullableHasValue(left);
            ec.EmitNullableHasValue(right);
            ig.Emit(OpCodes.And);
            ig.Emit(OpCodes.Brtrue, has_value);

            ec.EmitNullableInitialize(result);

            ig.Emit(OpCodes.Br, done);

            ig.MarkLabel(has_value);

            ec.EmitNullableGetValueOrDefault(left);
            ec.EmitNullableGetValueOrDefault(right);

            EmitBinaryOperator(ec);

            ec.EmitNullableNew(result.LocalType);

            ig.MarkLabel(done);
        }

        private void EmitLiftedToNullUserDefinedOperator(EmitContext ec)
        {
            var ig = ec.ig;

            var ret = ig.DefineLabel();
            var done = ig.DefineLabel();

            var left = ec.EmitStored(this.left);
            var right = ec.EmitStored(this.right);

            ec.EmitNullableHasValue(left);
            ec.EmitNullableHasValue(right);
            ig.Emit(OpCodes.And);
            ig.Emit(OpCodes.Brfalse, ret);

            ec.EmitNullableGetValueOrDefault(left);
            ec.EmitNullableGetValueOrDefault(right);
            ec.EmitCall(method);
            ec.EmitNullableNew(Type);
            ig.Emit(OpCodes.Br, done);

            ig.MarkLabel(ret);
            var temp = ig.DeclareLocal(Type);
            ec.EmitNullableInitialize(temp);

            ig.MarkLabel(done);
        }

        private void EmitLiftedUserDefinedOperator(EmitContext ec)
        {
            var ig = ec.ig;

            var ret_true = ig.DefineLabel();
            var ret_false = ig.DefineLabel();
            var done = ig.DefineLabel();

            var left = ec.EmitStored(this.left);
            var right = ec.EmitStored(this.right);

            ec.EmitNullableHasValue(left);
            ec.EmitNullableHasValue(right);
            switch (NodeType)
            {
                case ExpressionType.Equal:
                    ig.Emit(OpCodes.Bne_Un, ret_false);
                    ec.EmitNullableHasValue(left);
                    ig.Emit(OpCodes.Brfalse, ret_true);
                    break;

                case ExpressionType.NotEqual:
                    ig.Emit(OpCodes.Bne_Un, ret_true);
                    ec.EmitNullableHasValue(left);
                    ig.Emit(OpCodes.Brfalse, ret_false);
                    break;

                default:
                    ig.Emit(OpCodes.And);
                    ig.Emit(OpCodes.Brfalse, ret_false);
                    break;
            }

            ec.EmitNullableGetValueOrDefault(left);
            ec.EmitNullableGetValueOrDefault(right);
            ec.EmitCall(method);
            ig.Emit(OpCodes.Br, done);

            ig.MarkLabel(ret_true);
            ig.Emit(OpCodes.Ldc_I4_1);
            ig.Emit(OpCodes.Br, done);

            ig.MarkLabel(ret_false);
            ig.Emit(OpCodes.Ldc_I4_0);
            ig.Emit(OpCodes.Br, done);

            ig.MarkLabel(done);
        }

        private void EmitLogical(EmitContext ec)
        {
            EmitNonLiftedBinary(ec);
        }

        private void EmitLogicalBinary(EmitContext ec)
        {
            switch (NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.Or:
                    if (!IsLifted)
                        EmitLogical(ec);
                    else if (Type == typeof(bool?))
                        EmitLiftedLogical(ec);
                    else
                        EmitLiftedArithmeticBinary(ec);
                    break;

                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    if (!IsLifted)
                        EmitLogicalShortCircuit(ec);
                    else
                        EmitLiftedLogicalShortCircuit(ec);
                    break;
            }
        }

        private void EmitLogicalShortCircuit(EmitContext ec)
        {
            var ig = ec.ig;
            var and = NodeType == ExpressionType.AndAlso;
            var ret = ig.DefineLabel();
            var done = ig.DefineLabel();

            ec.Emit(left);
            ig.Emit(and ? OpCodes.Brfalse : OpCodes.Brtrue, ret);

            ec.Emit(right);

            ig.Emit(OpCodes.Br, done);

            ig.MarkLabel(ret);
            ig.Emit(and ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);

            ig.MarkLabel(done);
        }

        private void EmitNonLiftedBinary(EmitContext ec)
        {
            ec.Emit(left);
            ec.Emit(right);
            EmitBinaryOperator(ec);
        }

        private void EmitRelationalBinary(EmitContext ec)
        {
            if (!IsLifted)
                EmitNonLiftedBinary(ec);
            else if (IsLiftedToNull)
                EmitLiftedToNullBinary(ec);
            else
                EmitLiftedRelationalBinary(ec);
        }

        private void EmitUserDefinedLiftedLogicalShortCircuit(EmitContext ec)
        {
            var ig = ec.ig;
            var and = NodeType == ExpressionType.AndAlso;

            var left_is_null = ig.DefineLabel();
            var ret_left = ig.DefineLabel();
            var ret_null = ig.DefineLabel();
            var done = ig.DefineLabel();

            var left = ec.EmitStored(this.left);

            ec.EmitNullableHasValue(left);
            ig.Emit(OpCodes.Brfalse, and ? ret_null : left_is_null);

            ec.EmitNullableGetValueOrDefault(left);
            ec.EmitCall(and ? GetFalseOperator() : GetTrueOperator());
            ig.Emit(OpCodes.Brtrue, ret_left);

            ig.MarkLabel(left_is_null);
            var right = ec.EmitStored(this.right);
            ec.EmitNullableHasValue(right);
            ig.Emit(OpCodes.Brfalse, ret_null);

            ec.EmitNullableGetValueOrDefault(left);
            ec.EmitNullableGetValueOrDefault(right);
            ec.EmitCall(method);

            ec.EmitNullableNew(Type);
            ig.Emit(OpCodes.Br, done);

            ig.MarkLabel(ret_left);
            ec.EmitLoad(left);
            ig.Emit(OpCodes.Br, done);

            ig.MarkLabel(ret_null);
            var ret = ig.DeclareLocal(Type);
            ec.EmitNullableInitialize(ret);

            ig.MarkLabel(done);
        }

        private void EmitUserDefinedLogicalShortCircuit(EmitContext ec)
        {
            var ig = ec.ig;
            var and = NodeType == ExpressionType.AndAlso;

            var done = ig.DefineLabel();

            var left = ec.EmitStored(this.left);

            ec.EmitLoad(left);
            ig.Emit(OpCodes.Dup);
            ec.EmitCall(and ? GetFalseOperator() : GetTrueOperator());
            ig.Emit(OpCodes.Brtrue, done);

            ec.Emit(this.right);
            ec.EmitCall(method);

            ig.MarkLabel(done);
        }

        private void EmitUserDefinedOperator(EmitContext ec)
        {
            if (!IsLifted)
            {
                switch (NodeType)
                {
                    case ExpressionType.AndAlso:
                    case ExpressionType.OrElse:
                        EmitUserDefinedLogicalShortCircuit(ec);
                        break;

                    default:
                        left.Emit(ec);
                        right.Emit(ec);
                        ec.EmitCall(method);
                        break;
                }
            }
            else if (IsLiftedToNull)
            {
                switch (NodeType)
                {
                    case ExpressionType.AndAlso:
                    case ExpressionType.OrElse:
                        EmitUserDefinedLiftedLogicalShortCircuit(ec);
                        break;

                    default:
                        EmitLiftedToNullUserDefinedOperator(ec);
                        break;
                }
            }
            else
                EmitLiftedUserDefinedOperator(ec);
        }

        private MethodInfo GetFalseOperator()
        {
            return GetFalseOperator(left.Type.GetNotNullableType());
        }

        private MethodInfo GetTrueOperator()
        {
            return GetTrueOperator(left.Type.GetNotNullableType());
        }

        private bool IsLeftLiftedBinary()
        {
            return left.Type.IsNullable() && !right.Type.IsNullable();
        }
    }
}
