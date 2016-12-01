using System.Diagnostics;

namespace System.Collections.Generic
{
    internal sealed class CollectionDebuggerView<T>
    {
        private readonly ICollection<T> c;

        public CollectionDebuggerView(ICollection<T> col)
        {
            this.c = col;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                var o = new T[c.Count];
                c.CopyTo(o, 0);
                return o;
            }
        }
    }

    internal sealed class CollectionDebuggerView<T, U>
    {
        private readonly ICollection<KeyValuePair<T, U>> c;

        public CollectionDebuggerView(ICollection<KeyValuePair<T, U>> col)
        {
            this.c = col;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<T, U>[] Items
        {
            get
            {
                var o = new KeyValuePair<T, U>[c.Count];
                c.CopyTo(o, 0);
                return o;
            }
        }
    }
}
