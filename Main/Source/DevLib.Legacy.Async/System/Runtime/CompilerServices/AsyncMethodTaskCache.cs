using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    internal class AsyncMethodTaskCache<TResult>
    {
        internal static readonly AsyncMethodTaskCache<TResult> Singleton = AsyncMethodTaskCache<TResult>.CreateCache();

        internal static TaskCompletionSource<TResult> CreateCompleted(TResult result)
        {
            TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>();
            taskCompletionSource.TrySetResult(result);
            return taskCompletionSource;
        }

        internal virtual TaskCompletionSource<TResult> FromResult(TResult result)
        {
            return AsyncMethodTaskCache<TResult>.CreateCompleted(result);
        }

        private static AsyncMethodTaskCache<TResult> CreateCache()
        {
            Type typeFromHandle = typeof(TResult);
            if (typeFromHandle == typeof(bool))
            {
                return (AsyncMethodTaskCache<TResult>)(object)new AsyncMethodBooleanTaskCache();
            }

            if (typeFromHandle == typeof(int))
            {
                return (AsyncMethodTaskCache<TResult>)(object)new AsyncMethodInt32TaskCache();
            }

            return null;
        }

        private sealed class AsyncMethodBooleanTaskCache : AsyncMethodTaskCache<bool>
        {
            internal readonly TaskCompletionSource<bool> m_false = AsyncMethodTaskCache<bool>.CreateCompleted(false);
            internal readonly TaskCompletionSource<bool> m_true = AsyncMethodTaskCache<bool>.CreateCompleted(true);

            internal sealed override TaskCompletionSource<bool> FromResult(bool result)
            {
                if (!result)
                {
                    return this.m_false;
                }
                return this.m_true;
            }
        }

        private sealed class AsyncMethodInt32TaskCache : AsyncMethodTaskCache<int>
        {
            internal const int EXCLUSIVE_INT32_MAX = 9;
            internal const int INCLUSIVE_INT32_MIN = -1;
            internal static readonly TaskCompletionSource<int>[] Int32Tasks = AsyncMethodTaskCache<TResult>.AsyncMethodInt32TaskCache.CreateInt32Tasks();

            internal sealed override TaskCompletionSource<int> FromResult(int result)
            {
                if (result < -1 || result >= 9)
                {
                    return AsyncMethodTaskCache<int>.CreateCompleted(result);
                }
                return AsyncMethodTaskCache<TResult>.AsyncMethodInt32TaskCache.Int32Tasks[result - -1];
            }

            private static TaskCompletionSource<int>[] CreateInt32Tasks()
            {
                TaskCompletionSource<int>[] array = new TaskCompletionSource<int>[10];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = AsyncMethodTaskCache<int>.CreateCompleted(i + -1);
                }
                return array;
            }
        }
    }
}
