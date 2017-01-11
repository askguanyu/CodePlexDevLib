using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    public class Lookup<TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>, ILookup<TKey, TElement>
    {
        private Dictionary<TKey, IGrouping<TKey, TElement>> groups;
        private IGrouping<TKey, TElement> nullGrouping;

        internal Lookup(Dictionary<TKey, List<TElement>> lookup, IEnumerable<TElement> nullKeyElements)
        {
            groups = new Dictionary<TKey, IGrouping<TKey, TElement>>(lookup.Comparer);
            foreach (var slot in lookup)
                groups.Add(slot.Key, new Grouping<TKey, TElement>(slot.Key, slot.Value));

            if (nullKeyElements != null)
                nullGrouping = new Grouping<TKey, TElement>(default(TKey), nullKeyElements);
        }

        public int Count
        {
            get { return (nullGrouping == null) ? groups.Count : groups.Count + 1; }
        }

        public IEnumerable<TElement> this[TKey key]
        {
            get
            {
                if (key == null && nullGrouping != null)
                    return nullGrouping;
                else if (key != null)
                {
                    IGrouping<TKey, TElement> group;
                    if (groups.TryGetValue(key, out group))
                        return group;
                }

                return new TElement[0];
            }
        }

        public IEnumerable<TResult> ApplyResultSelector<TResult>(Func<TKey, IEnumerable<TElement>, TResult> selector)
        {
            if (nullGrouping != null)
                yield return selector(nullGrouping.Key, nullGrouping);

            foreach (var group in groups.Values)
                yield return selector(group.Key, group);
        }

        public bool Contains(TKey key)
        {
            return (key != null) ? groups.ContainsKey(key) : nullGrouping != null;
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            if (nullGrouping != null)
                yield return nullGrouping;

            foreach (var g in groups.Values)
                yield return g;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
