using System.Collections.Concurrent;

namespace System.Threading.Tasks
{
    internal struct TaskCompletionQueue<TCompletion> where TCompletion : class
    {
        private object completed;
        private object single;

        public bool HasElements
        {
            get
            {
                return single != null || (completed != null && Completed.Count != 0);
            }
        }

        ConcurrentOrderedList<TCompletion> Completed
        {
            get { return (ConcurrentOrderedList<TCompletion>)completed; }
        }

        TCompletion Single
        {
            get { return (TCompletion)single; }
        }

        public void Add(TCompletion continuation)
        {
            if (single == null && Interlocked.CompareExchange(ref single, continuation, null) == null)
                return;
            if (completed == null)
                Interlocked.CompareExchange(ref completed, new ConcurrentOrderedList<TCompletion>(), null);
            Completed.TryAdd(continuation);
        }

        public bool Remove(TCompletion continuation)
        {
            TCompletion temp = Single;
            if (temp != null && temp == continuation && Interlocked.CompareExchange(ref single, null, continuation) == continuation)
                return true;
            if (completed != null)
                return Completed.TryRemove(continuation);
            return false;
        }

        public bool TryGetNextCompletion(out TCompletion continuation)
        {
            continuation = null;

            if (single != null && (continuation = (TCompletion)Interlocked.Exchange(ref single, null)) != null)
                return true;

            return completed != null && Completed.TryPop(out continuation);
        }
    }
}
