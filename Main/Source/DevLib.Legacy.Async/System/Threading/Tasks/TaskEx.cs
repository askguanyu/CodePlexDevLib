using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
    public static class TaskEx
    {
        private const string ArgumentOutOfRange_TimeoutNonNegativeOrMinusOne = "The timeout must be non-negative or -1, and it must be less than or equal to Int32.MaxValue.";

        private static Task s_preCanceledTask = ((Func<Task>)(() =>
        {
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
            completionSource.TrySetCanceled();
            return (Task)completionSource.Task;
        }))();

        private static Task s_preCompletedTask = (Task)TaskEx.FromResult<bool>(false);

        public static Task Delay(int dueTime)
        {
            return TaskEx.Delay(dueTime, CancellationToken.None);
        }

        public static Task Delay(TimeSpan dueTime)
        {
            return TaskEx.Delay(dueTime, CancellationToken.None);
        }

        public static Task Delay(TimeSpan dueTime, CancellationToken cancellationToken)
        {
            long totalMilliseconds = (long)dueTime.TotalMilliseconds;
            if (totalMilliseconds < -1L || totalMilliseconds > (long)int.MaxValue)
                throw new ArgumentOutOfRangeException("dueTime", "The timeout must be non-negative or -1, and it must be less than or equal to Int32.MaxValue.");
            return TaskEx.Delay((int)totalMilliseconds, cancellationToken);
        }

        public static Task Delay(int dueTime, CancellationToken cancellationToken)
        {
            if (dueTime < -1)
                throw new ArgumentOutOfRangeException("dueTime", "The timeout must be non-negative or -1, and it must be less than or equal to Int32.MaxValue.");
            if (cancellationToken.IsCancellationRequested)
                return TaskEx.s_preCanceledTask;
            if (dueTime == 0)
                return TaskEx.s_preCompletedTask;
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            CancellationTokenRegistration ctr = new CancellationTokenRegistration();
            Timer timer = new Timer((TimerCallback)(self =>
            {
                ctr.Dispose();
                ((Timer)self).Dispose();
                tcs.TrySetResult(true);
            }));
            if (cancellationToken.CanBeCanceled)
                ctr = cancellationToken.Register((Action)(() =>
                {
                    timer.Dispose();
                    tcs.TrySetCanceled();
                }));
            timer.Change(dueTime, -1);
            return (Task)tcs.Task;
        }

        public static Task<TResult> FromResult<TResult>(TResult result)
        {
            TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>((object)result);
            completionSource.TrySetResult(result);
            return completionSource.Task;
        }

        public static Task Run(Action action)
        {
            return TaskEx.Run(action, CancellationToken.None);
        }

        public static Task Run(Action action, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(action, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
        }

        public static Task<TResult> Run<TResult>(Func<TResult> function)
        {
            return TaskEx.Run<TResult>(function, CancellationToken.None);
        }

        public static Task<TResult> Run<TResult>(Func<TResult> function, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew<TResult>(function, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
        }

        public static Task Run(Func<Task> function)
        {
            return TaskEx.Run(function, CancellationToken.None);
        }

        public static Task Run(Func<Task> function, CancellationToken cancellationToken)
        {
            return TaskEx.Run<Task>(function, cancellationToken).Unwrap();
        }

        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function)
        {
            return TaskEx.Run<TResult>(function, CancellationToken.None);
        }

        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken)
        {
            return TaskEx.Run<Task<TResult>>(function, cancellationToken).Unwrap<TResult>();
        }

        public static Task WhenAll(params Task[] tasks)
        {
            return TaskEx.WhenAll((IEnumerable<Task>)tasks);
        }

        public static Task<TResult[]> WhenAll<TResult>(params Task<TResult>[] tasks)
        {
            return TaskEx.WhenAll<TResult>((IEnumerable<Task<TResult>>)tasks);
        }

        public static Task WhenAll(IEnumerable<Task> tasks)
        {
            return (Task)TaskEx.WhenAllCore<object>(tasks, (Action<Task[], TaskCompletionSource<object>>)((completedTasks, tcs) => tcs.TrySetResult((object)null)));
        }

        public static Task<TResult[]> WhenAll<TResult>(IEnumerable<Task<TResult>> tasks)
        {
            return TaskEx.WhenAllCore<TResult[]>(tasks.Cast<Task>(), (Action<Task[], TaskCompletionSource<TResult[]>>)((completedTasks, tcs) => tcs.TrySetResult(((IEnumerable<Task>)completedTasks).Select<Task, TResult>((Func<Task, TResult>)(t => ((Task<TResult>)t).Result)).ToArray<TResult>())));
        }

        public static Task<Task> WhenAny(params Task[] tasks)
        {
            return TaskEx.WhenAny((IEnumerable<Task>)tasks);
        }

        public static Task<Task> WhenAny(IEnumerable<Task> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException("tasks");
            TaskCompletionSource<Task> tcs = new TaskCompletionSource<Task>();
            Task.Factory.ContinueWhenAny<bool>(tasks as Task[] ?? tasks.ToArray<Task>(), (Func<Task, bool>)(completed => tcs.TrySetResult(completed)), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            return tcs.Task;
        }

        public static Task<Task<TResult>> WhenAny<TResult>(params Task<TResult>[] tasks)
        {
            return TaskEx.WhenAny<TResult>((IEnumerable<Task<TResult>>)tasks);
        }

        public static Task<Task<TResult>> WhenAny<TResult>(IEnumerable<Task<TResult>> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException("tasks");
            TaskCompletionSource<Task<TResult>> tcs = new TaskCompletionSource<Task<TResult>>();
            Task.Factory.ContinueWhenAny<TResult, bool>(tasks as Task<TResult>[] ?? tasks.ToArray<Task<TResult>>(), (Func<Task<TResult>, bool>)(completed => tcs.TrySetResult(completed)), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            return tcs.Task;
        }

        public static YieldAwaitable Yield()
        {
            return new YieldAwaitable();
        }

        private static void AddPotentiallyUnwrappedExceptions(ref List<Exception> targetList, Exception exception)
        {
            AggregateException aggregateException = exception as AggregateException;
            if (targetList == null)
                targetList = new List<Exception>();
            if (aggregateException != null)
                targetList.Add(aggregateException.InnerExceptions.Count == 1 ? exception.InnerException : exception);
            else
                targetList.Add(exception);
        }

        private static Task<TResult> WhenAllCore<TResult>(IEnumerable<Task> tasks, Action<Task[], TaskCompletionSource<TResult>> setResultAction)
        {
            if (tasks == null)
                throw new ArgumentNullException("tasks");
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            Task[] tasks1 = tasks as Task[] ?? tasks.ToArray<Task>();
            if (tasks1.Length == 0)
                setResultAction(tasks1, tcs);
            else
                Task.Factory.ContinueWhenAll(tasks1, (Action<Task[]>)(completedTasks =>
                {
                    List<Exception> targetList = (List<Exception>)null;
                    bool flag = false;
                    foreach (Task completedTask in completedTasks)
                    {
                        if (completedTask.IsFaulted)
                            TaskEx.AddPotentiallyUnwrappedExceptions(ref targetList, (Exception)completedTask.Exception);
                        else if (completedTask.IsCanceled)
                            flag = true;
                    }
                    if (targetList != null && targetList.Count > 0)
                        tcs.TrySetException((IEnumerable<Exception>)targetList);
                    else if (flag)
                        tcs.TrySetCanceled();
                    else
                        setResultAction(completedTasks, tcs);
                }), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            return tcs.Task;
        }
    }
}
