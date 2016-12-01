using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct TaskAwaiter : ICriticalNotifyCompletion
    {
        private readonly Task task;

        internal TaskAwaiter(Task task)
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

        public void GetResult()
        {
            if (!task.IsCompleted)
                task.WaitCore(Timeout.Infinite, CancellationToken.None, true);

            if (task.Status != TaskStatus.RanToCompletion)
                ExceptionDispatchInfo.Capture(HandleUnexpectedTaskResult(task)).Throw();
        }

        public void OnCompleted(Action continuation)
        {
            if (continuation == null)
                throw new ArgumentNullException("continuation");

            HandleOnCompleted(task, continuation, true, true);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            if (continuation == null)
                throw new ArgumentNullException("continuation");

            HandleOnCompleted(task, continuation, true, false);
        }

        internal static void HandleOnCompleted(Task task, Action continuation, bool continueOnSourceContext, bool manageContext)
        {
            if (continueOnSourceContext && SynchronizationContext.Current != null && SynchronizationContext.Current.GetType() != typeof(SynchronizationContext))
            {
                task.ContinueWith(new SynchronizationContextContinuation(continuation, SynchronizationContext.Current));
            }
            else
            {
                IContinuation cont;
                Task cont_task;
                if (continueOnSourceContext && !TaskScheduler.IsDefault)
                {
                    cont_task = new Task(TaskActionInvoker.Create(continuation), null, CancellationToken.None, TaskCreationOptions.None, null);
                    cont_task.SetupScheduler(TaskScheduler.Current);
                    cont = new SchedulerAwaitContinuation(cont_task);
                }
                else
                {
                    cont_task = null;
                    cont = new AwaiterActionContinuation(continuation);
                }

                if (task.ContinueWith(cont, false))
                    return;

                if (cont_task == null)
                {
                    cont_task = new Task(TaskActionInvoker.Create(continuation), null, CancellationToken.None, TaskCreationOptions.None, null);
                    cont_task.SetupScheduler(TaskScheduler.Current);
                }

                cont_task.Schedule(true);
            }
        }

        internal static Exception HandleUnexpectedTaskResult(Task task)
        {
            var slot = task.ExceptionSlot;
            switch (task.Status)
            {
                case TaskStatus.Canceled:
                    if (slot.Exception != null)
                        goto case TaskStatus.Faulted;

                    return new TaskCanceledException(task);

                case TaskStatus.Faulted:
                    slot.Observed = true;
                    return slot.Exception.InnerException;

                default:
                    throw new ArgumentException(string.Format("Unexpected task `{0}' status `{1}'", task.Id, task.Status));
            }
        }
    }
}
