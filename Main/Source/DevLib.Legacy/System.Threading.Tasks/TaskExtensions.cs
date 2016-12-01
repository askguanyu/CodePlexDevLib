#if NET35
namespace System.Threading.Tasks
{
    public static class TaskExtensions
    {
        public static Task<TResult> Unwrap<TResult>(this Task<Task<TResult>> task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            return TaskExtensionsImpl.Unwrap(task);
        }

        public static Task Unwrap(this Task<Task> task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            return TaskExtensionsImpl.Unwrap(task);
        }
    }
}
#endif
