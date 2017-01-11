namespace System.Threading.Tasks
{
    internal static class TaskConstants
    {
        public static readonly Task Canceled;
        public static readonly Task Finished;

        static TaskConstants()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            Finished = tcs.Task;

            tcs = new TaskCompletionSource<object>();
            tcs.SetCanceled();
            Canceled = tcs.Task;
        }
    }
}
