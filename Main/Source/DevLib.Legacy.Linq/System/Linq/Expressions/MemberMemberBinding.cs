using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions
{
    public sealed class MemberMemberBinding : MemberBinding
    {
        private ReadOnlyCollection<MemberBinding> bindings;

        internal MemberMemberBinding(MemberInfo member, ReadOnlyCollection<MemberBinding> bindings)
            : base(MemberBindingType.MemberBinding, member)
        {
            this.bindings = bindings;
        }

        public ReadOnlyCollection<MemberBinding> Bindings
        {
            get { return bindings; }
        }

        internal override void Emit(EmitContext ec, LocalBuilder local)
        {
            var member = EmitLoadMember(ec, local);

            foreach (var binding in bindings)
                binding.Emit(ec, member);
        }
    }
}
