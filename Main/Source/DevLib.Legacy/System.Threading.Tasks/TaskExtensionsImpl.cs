namespace System.Threading.Tasks
{
    public static class TaskExtensionsImpl
    {
        private const TaskContinuationOptions options = TaskContinuationOptions.ExecuteSynchronously;

        public static Task<TResult> Unwrap<TResult>(Task<Task<TResult>> task)
        {
            var src = new TaskCompletionSource<TResult>();
            task.ContinueWith((t, arg) => Cont(t, (TaskCompletionSource<TResult>)arg), src, CancellationToken.None, options, TaskScheduler.Current);

            return src.Task;
        }

        public static Task Unwrap(Task<Task> task)
        {
            var src = new TaskCompletionSource<object>();
            task.ContinueWith((t, arg) => Cont(t, (TaskCompletionSource<object>)arg), src, CancellationToken.None, options, TaskScheduler.Current);

            return src.Task;
        }

        private static void Cont(Task<Task> source, TaskCompletionSource<object> dest)
        {
            if (source.IsCanceled)
                dest.SetCanceled();
            else if (source.IsFaulted)
                dest.SetException(source.Exception.InnerExceptions);
            else
                source.Result.ContinueWith((t, arg) => SetResult(t, (TaskCompletionSource<object>)arg), dest, CancellationToken.None, options, TaskScheduler.Current);
        }

        private static void Cont<TResult>(Task<Task<TResult>> source, TaskCompletionSource<TResult> dest)
        {
            if (source.IsCanceled)
                dest.SetCanceled();
            else if (source.IsFaulted)
                dest.SetException(source.Exception.InnerExceptions);
            else
                source.Result.ContinueWith((t, arg) => SetResult(t, (TaskCompletionSource<TResult>)arg), dest, CancellationToken.None, options, TaskScheduler.Current);
        }

        private static void SetResult(Task source, TaskCompletionSource<object> dest)
        {
            if (source.IsCanceled)
                dest.SetCanceled();
            else if (source.IsFaulted)
                dest.SetException(source.Exception.InnerExceptions);
            else
                dest.SetResult(null);
        }

        private static void SetResult<TResult>(Task<TResult> source, TaskCompletionSource<TResult> dest)
        {
            if (source.IsCanceled)
                dest.SetCanceled();
            else if (source.IsFaulted)
                dest.SetException(source.Exception.InnerExceptions);
            else
                dest.SetResult(source.Result);
        }
    }
}
