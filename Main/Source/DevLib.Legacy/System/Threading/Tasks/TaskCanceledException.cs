using System.Runtime.Serialization;

namespace System.Threading.Tasks
{
    [Serializable]
    public class TaskCanceledException : System.Couchbase.OperationCanceledException
    {
        private readonly Task task;

        public TaskCanceledException()
            : base()
        {
        }

        public TaskCanceledException(string message)
            : base(message)
        {
        }

        public TaskCanceledException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public TaskCanceledException(Task task)
            : base("The Task was canceled")
        {
            this.task = task;
        }

        protected TaskCanceledException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public Task Task
        {
            get
            {
                return task;
            }
        }
    }
}
