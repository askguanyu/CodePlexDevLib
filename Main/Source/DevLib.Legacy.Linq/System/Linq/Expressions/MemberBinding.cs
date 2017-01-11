using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions
{
    public abstract class MemberBinding
    {
        private MemberBindingType binding_type;
        private MemberInfo member;

        protected MemberBinding(MemberBindingType binding_type, MemberInfo member)
        {
            this.binding_type = binding_type;
            this.member = member;
        }

        public MemberBindingType BindingType
        {
            get { return binding_type; }
        }

        public MemberInfo Member
        {
            get { return member; }
        }

        public override string ToString()
        {
            return ExpressionPrinter.ToString(this);
        }

        internal abstract void Emit(EmitContext ec, LocalBuilder local);

        internal LocalBuilder EmitLoadMember(EmitContext ec, LocalBuilder local)
        {
            ec.EmitLoadSubject(local);

            return member.OnFieldOrProperty<LocalBuilder>(
                field => EmitLoadField(ec, field),
                prop => EmitLoadProperty(ec, prop));
        }

        private LocalBuilder EmitLoadField(EmitContext ec, FieldInfo field)
        {
            var store = ec.ig.DeclareLocal(field.FieldType);
            ec.ig.Emit(OpCodes.Ldfld, field);
            ec.ig.Emit(OpCodes.Stloc, store);
            return store;
        }

        private LocalBuilder EmitLoadProperty(EmitContext ec, PropertyInfo property)
        {
            var getter = property.GetGetMethod(true);
            if (getter == null)
                throw new NotSupportedException();

            var store = ec.ig.DeclareLocal(property.PropertyType);
            ec.EmitCall(getter);
            ec.ig.Emit(OpCodes.Stloc, store);
            return store;
        }
    }
}
