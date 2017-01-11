using System.Collections.Generic;

namespace System.Threading.Tasks
{
    internal interface IConcurrentDeque<T>
    {
        IEnumerable<T> GetEnumerable();

        PopResult PopBottom(out T obj);

        PopResult PopTop(out T obj);

        void PushBottom(T obj);
    }
}
