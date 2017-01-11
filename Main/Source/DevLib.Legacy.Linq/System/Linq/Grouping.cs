using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    internal class Grouping<K, T> : IGrouping<K, T>
    {
        private IEnumerable<T> group;
        private K key;

        public Grouping(K key, IEnumerable<T> group)
        {
            this.group = group;
            this.key = key;
        }

        public K Key
        {
            get { return key; }
            set { key = value; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return group.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return group.GetEnumerator();
        }
    }
}
