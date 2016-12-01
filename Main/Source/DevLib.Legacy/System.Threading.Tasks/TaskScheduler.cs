using System.Collections.Generic;
using System.Diagnostics;

namespace System.Threading.Tasks
{
    [DebuggerDisplay("Id={Id}")]
    [DebuggerTypeProxy(typeof(TaskSchedulerDebuggerView))]
    public abstract class TaskScheduler
    {
        private static readonly TaskScheduler defaultScheduler = new TpScheduler();

        [ThreadStatic]
        private static TaskScheduler currentScheduler;

        private static int lastId = int.MinValue;

        private int id;

        protected TaskScheduler()
        {
            this.id = Interlocked.Increment(ref lastId);
        }

        public static event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;

        public static TaskScheduler Current
        {
            get
            {
                if (currentScheduler != null)
                    return currentScheduler;

                return defaultScheduler;
            }
            internal set
            {
                currentScheduler = value;
            }
        }

        public static TaskScheduler Default
        {
            get
            {
                return defaultScheduler;
            }
        }

        public int Id
        {
            get
            {
                return id;
            }
        }

        public virtual int MaximumConcurrencyLevel
        {
            get
            {
                return int.MaxValue;
            }
        }

        internal static bool IsDefault
        {
            get
            {
                return currentScheduler == null || currentScheduler == defaultScheduler;
            }
        }

        public static TaskScheduler FromCurrentSynchronizationContext()
        {
            var syncCtx = SynchronizationContext.Current;
            if (syncCtx == null)
                throw new InvalidOperationException("The current SynchronizationContext is null and cannot be used as a TaskScheduler");

            return new SynchronizationContextScheduler(syncCtx);
        }

        internal static UnobservedTaskExceptionEventArgs FireUnobservedEvent(Task task, AggregateException e)
        {
            UnobservedTaskExceptionEventArgs args = new UnobservedTaskExceptionEventArgs(e);

            EventHandler<UnobservedTaskExceptionEventArgs> temp = UnobservedTaskException;
            if (temp == null)
                return args;

            temp(task, args);

            return args;
        }

        internal bool RunInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (!TryExecuteTaskInline(task, taskWasPreviouslyQueued))
                return false;

            if (!task.IsCompleted)
                throw new InvalidOperationException("The TryExecuteTaskInline call to the underlying scheduler succeeded, but the task body was not invoked");

            return true;
        }

        protected internal abstract void QueueTask(Task task);

        protected internal virtual bool TryDequeue(Task task)
        {
            return false;
        }

        protected internal bool TryExecuteTask(Task task)
        {
            if (task.IsCompleted)
                return false;

            if (task.Status == TaskStatus.WaitingToRun)
            {
                task.Execute();
                if (task.WaitOnChildren())
                    task.Wait();

                return true;
            }

            return false;
        }

        protected abstract IEnumerable<Task> GetScheduledTasks();

        protected abstract bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued);

        private sealed class TaskSchedulerDebuggerView
        {
            private readonly TaskScheduler scheduler;

            public TaskSchedulerDebuggerView(TaskScheduler scheduler)
            {
                this.scheduler = scheduler;
            }

            public IEnumerable<Task> ScheduledTasks
            {
                get
                {
                    return scheduler.GetScheduledTasks();
                }
            }
        }
    }
}
