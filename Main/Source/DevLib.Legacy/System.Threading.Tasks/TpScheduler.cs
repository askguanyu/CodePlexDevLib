using System.Collections.Generic;

namespace System.Threading.Tasks
{
    internal sealed class TpScheduler : TaskScheduler
    {
        private static readonly WaitCallback callback = TaskExecuterCallback;

        protected internal override void QueueTask(Task task)
        {
            if ((task.CreationOptions & TaskCreationOptions.LongRunning) != 0)
            {
                var thread = new Thread(l => ((Task)l).Execute())
                {
                    IsBackground = true
                };

                thread.Start(task);
                return;
            }

            ThreadPool.QueueUserWorkItem(callback, task);
        }

        protected internal override bool TryDequeue(Task task)
        {
            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            throw new NotImplementedException();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (taskWasPreviouslyQueued && !TryDequeue(task))
                return false;

            return TryExecuteTask(task);
        }

        private static void TaskExecuterCallback(object obj)
        {
            Task task = (Task)obj;
            task.Execute();
        }
    }
}
