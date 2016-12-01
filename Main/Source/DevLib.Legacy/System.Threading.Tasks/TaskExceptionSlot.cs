using System.Collections.Concurrent;

namespace System.Threading.Tasks
{
    internal class TaskExceptionSlot
    {
        public object childExceptions;
        public volatile AggregateException Exception;
        public volatile bool Observed;
        private Task parent;

        public TaskExceptionSlot(Task parent)
        {
            this.parent = parent;
        }

        ~TaskExceptionSlot()
        {
            if (Exception != null && !Observed && !TaskScheduler.FireUnobservedEvent(parent, Exception).Observed)
            {
            }
        }

        public ConcurrentQueue<AggregateException> ChildExceptions
        {
            get { return (ConcurrentQueue<AggregateException>)childExceptions; }
        }
    }
}
