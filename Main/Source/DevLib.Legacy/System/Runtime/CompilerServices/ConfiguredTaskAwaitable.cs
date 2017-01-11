using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct ConfiguredTaskAwaitable
    {
        private readonly ConfiguredTaskAwaiter awaiter;

        internal ConfiguredTaskAwaitable(Task task, bool continueOnSourceContext)
        {
            awaiter = new ConfiguredTaskAwaiter(task, continueOnSourceContext);
        }

        public ConfiguredTaskAwaiter GetAwaiter()
        {
            return awaiter;
        }

        public struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion
        {
            private readonly bool continueOnSourceContext;
            private readonly Task task;

            internal ConfiguredTaskAwaiter(Task task, bool continueOnSourceContext)
            {
                this.task = task;
                this.continueOnSourceContext = continueOnSourceContext;
            }

            public bool IsCompleted
            {
                get
                {
                    return task.IsCompleted;
                }
            }

            public void GetResult()
            {
                if (!task.IsCompleted)
                    task.WaitCore(Timeout.Infinite, CancellationToken.None, true);

                if (task.Status != TaskStatus.RanToCompletion)
                    ExceptionDispatchInfo.Capture(TaskAwaiter.HandleUnexpectedTaskResult(task)).Throw();
            }

            public void OnCompleted(Action continuation)
            {
                if (continuation == null)
                    throw new ArgumentNullException("continuation");

                TaskAwaiter.HandleOnCompleted(task, continuation, continueOnSourceContext, true);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                if (continuation == null)
                    throw new ArgumentNullException("continuation");

                TaskAwaiter.HandleOnCompleted(task, continuation, continueOnSourceContext, false);
            }
        }
    }
}
