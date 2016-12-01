using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct YieldAwaitable
    {
        public YieldAwaitable.YieldAwaiter GetAwaiter()
        {
            return new YieldAwaiter();
        }

        public struct YieldAwaiter : ICriticalNotifyCompletion
        {
            public bool IsCompleted
            {
                get
                {
                    return false;
                }
            }

            public void GetResult()
            {
            }

            public void OnCompleted(Action continuation)
            {
                OnCompleted(continuation, false);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                OnCompleted(continuation, true);
            }

            private void OnCompleted(Action continuation, bool isUnsafe)
            {
                if (continuation == null)
                    throw new ArgumentNullException("continuation");

                var ctx = SynchronizationContext.Current;
                if (ctx != null && ctx.GetType() != typeof(SynchronizationContext))
                {
                    ctx.Post(l => ((Action)l)(), continuation);
                    return;
                }

                if (TaskScheduler.IsDefault)
                {
                    WaitCallback callBack = l => ((Action)l)();
                    ThreadPool.QueueUserWorkItem(callBack, continuation);
                    return;
                }

                new Task(continuation).Start(TaskScheduler.Current);
            }
        }
    }
}
