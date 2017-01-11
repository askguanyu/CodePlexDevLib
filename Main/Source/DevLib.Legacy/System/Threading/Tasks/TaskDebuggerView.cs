namespace System.Threading.Tasks
{
    internal sealed class TaskDebuggerView
    {
        private readonly Task task;

        public TaskDebuggerView(Task task)
        {
            this.task = task;
        }

        public object AsyncState
        {
            get
            {
                return task.AsyncState;
            }
        }

        public TaskCreationOptions CreationOptions
        {
            get
            {
                return task.CreationOptions;
            }
        }

        public Exception Exception
        {
            get
            {
                return task.Exception;
            }
        }

        public int Id
        {
            get
            {
                return task.Id;
            }
        }

        public string Method
        {
            get
            {
                return task.DisplayActionMethod;
            }
        }

        public TaskStatus Status
        {
            get
            {
                return task.Status;
            }
        }
    }
}
