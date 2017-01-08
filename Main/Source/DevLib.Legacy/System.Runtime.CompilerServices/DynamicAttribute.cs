#if NET_4_0 || BOOTSTRAP_NET_4_0 || MOONLIGHT

using System.Collections.Generic;

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public sealed class DynamicAttribute : Attribute
    {
        private static readonly IList<bool> empty = Array.AsReadOnly(new[] { true });

        private IList<bool> transformFlags;

        public DynamicAttribute()
        {
            transformFlags = empty;
        }

        public DynamicAttribute(bool[] transformFlags)
        {
            if (transformFlags == null)
                throw new ArgumentNullException();

            this.transformFlags = transformFlags;
        }

        public IList<bool> TransformFlags
        {
            get
            {
                return transformFlags;
            }
        }
    }
}

#endif
