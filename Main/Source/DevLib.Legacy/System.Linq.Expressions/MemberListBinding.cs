using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions
{
    public sealed class MemberListBinding : MemberBinding
    {
        private ReadOnlyCollection<ElementInit> initializers;

        internal MemberListBinding(MemberInfo member, ReadOnlyCollection<ElementInit> initializers)
            : base(MemberBindingType.ListBinding, member)
        {
            this.initializers = initializers;
        }

        public ReadOnlyCollection<ElementInit> Initializers
        {
            get { return initializers; }
        }

        internal override void Emit(EmitContext ec, LocalBuilder local)
        {
            var member = EmitLoadMember(ec, local);

            foreach (var initializer in initializers)
                initializer.Emit(ec, member);
        }
    }
}
