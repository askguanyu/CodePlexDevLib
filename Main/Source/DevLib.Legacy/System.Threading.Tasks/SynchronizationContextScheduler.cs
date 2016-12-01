namespace System.Threading.Tasks
{
    internal sealed class SynchronizationContextScheduler : TaskScheduler
    {
        private readonly SendOrPostCallback callback;
        private readonly SynchronizationContext ctx;

        public SynchronizationContextScheduler(SynchronizationContext ctx)
        {
            this.ctx = ctx;
            this.callback = TaskLaunchWrapper;
        }

        public override int MaximumConcurrencyLevel
        {
            get
            {
                return 1;
            }
        }

        protected internal override void QueueTask(Task task)
        {
            ctx.Post(callback, task);
        }

        protected internal override bool TryDequeue(Task task)
        {
            return false;
        }

        protected override System.Collections.Generic.IEnumerable<Task> GetScheduledTasks()
        {
            throw new System.NotImplementedException();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return ctx == SynchronizationContext.Current && TryExecuteTask(task);
        }

        private void TaskLaunchWrapper(object obj)
        {
            TryExecuteTask((Task)obj);
        }
    }
}
