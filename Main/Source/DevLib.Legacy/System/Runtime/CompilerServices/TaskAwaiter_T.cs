using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct TaskAwaiter<TResult> : ICriticalNotifyCompletion
    {
        private readonly Task<TResult> task;

        internal TaskAwaiter(Task<TResult> task)
        {
            this.task = task;
        }

        public bool IsCompleted
        {
            get
            {
                return task.IsCompleted;
            }
        }

        public TResult GetResult()
        {
            if (!task.IsCompleted)
                task.WaitCore(Timeout.Infinite, CancellationToken.None, true);

            if (task.Status != TaskStatus.RanToCompletion)
                ExceptionDispatchInfo.Capture(TaskAwaiter.HandleUnexpectedTaskResult(task)).Throw();

            return task.Result;
        }

        public void OnCompleted(Action continuation)
        {
            if (continuation == null)
                throw new ArgumentNullException("continuation");

            TaskAwaiter.HandleOnCompleted(task, continuation, true, true);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            if (continuation == null)
                throw new ArgumentNullException("continuation");

            TaskAwaiter.HandleOnCompleted(task, continuation, true, false);
        }
    }
}
