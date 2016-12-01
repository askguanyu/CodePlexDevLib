namespace System.Threading.Tasks
{
    internal class TaskConstants<T>
    {
        internal static readonly Task<T> Canceled;

        static TaskConstants()
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            Canceled = tcs.Task;
        }
    }
}
