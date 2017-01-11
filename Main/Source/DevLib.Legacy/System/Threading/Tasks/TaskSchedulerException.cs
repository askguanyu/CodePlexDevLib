using System.Runtime.Serialization;

namespace System.Threading.Tasks
{
    public class TaskSchedulerException : Exception
    {
        private const string exceptionDefaultMessage = "An exception was thrown by a TaskScheduler";

        public TaskSchedulerException()
            : base(exceptionDefaultMessage)
        {
        }

        public TaskSchedulerException(string message)
            : base(message)
        {
        }

        public TaskSchedulerException(Exception innerException)
            : base(exceptionDefaultMessage, innerException)
        {
        }

        public TaskSchedulerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected TaskSchedulerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
