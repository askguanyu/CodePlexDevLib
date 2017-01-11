using System.Reflection;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct TaskAwaiter : ICriticalNotifyCompletion, INotifyCompletion
    {
        internal const bool CONTINUE_ON_CAPTURED_CONTEXT_DEFAULT = true;

        private const string InvalidOperationException_TaskNotCompleted = "The task has not yet completed.";

        private static readonly object[] s_emptyParams = new object[0];
        private static readonly MethodInfo s_prepForRemoting = TaskAwaiter.GetPrepForRemotingMethodInfo();
        private readonly Task m_task;

        internal TaskAwaiter(Task task)
        {
            this.m_task = task;
        }

        public bool IsCompleted
        {
            get
            {
                return this.m_task.IsCompleted;
            }
        }

        private static bool IsValidLocationForInlining
        {
            get
            {
                SynchronizationContext current = SynchronizationContext.Current;
                return (current == null || !(current.GetType() != typeof(SynchronizationContext))) && TaskScheduler.Current == TaskScheduler.Default;
            }
        }

        public void GetResult()
        {
            TaskAwaiter.ValidateEnd(this.m_task);
        }

        public void OnCompleted(Action continuation)
        {
            TaskAwaiter.OnCompletedInternal(this.m_task, continuation, true);
        }

        [SecurityCritical]
        public void UnsafeOnCompleted(Action continuation)
        {
            TaskAwaiter.OnCompletedInternal(this.m_task, continuation, true);
        }

        internal static void OnCompletedInternal(Task task, Action continuation, bool continueOnCapturedContext)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException("continuation");
            }
            SynchronizationContext sc = continueOnCapturedContext ? SynchronizationContext.Current : null;
            if (sc != null && sc.GetType() != typeof(SynchronizationContext))
            {
                task.ContinueWith(delegate(Task param0)
                {
                    try
                    {
                        sc.Post(delegate(object state)
                        {
                            ((Action)state)();
                        }, continuation);
                    }
                    catch (Exception exception)
                    {
                        AsyncMethodBuilderCore.ThrowAsync(exception, null);
                    }
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                return;
            }
            TaskScheduler taskScheduler = continueOnCapturedContext ? TaskScheduler.Current : TaskScheduler.Default;
            if (task.IsCompleted)
            {
                Task.Factory.StartNew(delegate(object s)
                {
                    ((Action)s)();
                }, continuation, CancellationToken.None, TaskCreationOptions.None, taskScheduler);
                return;
            }
            if (taskScheduler != TaskScheduler.Default)
            {
                task.ContinueWith(delegate(Task _)
                {
                    TaskAwaiter.RunNoException(continuation);
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, taskScheduler);
                return;
            }
            task.ContinueWith(delegate(Task param0)
            {
                if (TaskAwaiter.IsValidLocationForInlining)
                {
                    TaskAwaiter.RunNoException(continuation);
                    return;
                }
                Task.Factory.StartNew(delegate(object s)
                {
                    TaskAwaiter.RunNoException((Action)s);
                }, continuation, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        internal static Exception PrepareExceptionForRethrow(Exception exc)
        {
            if (TaskAwaiter.s_prepForRemoting != null)
            {
                try
                {
                    TaskAwaiter.s_prepForRemoting.Invoke(exc, TaskAwaiter.s_emptyParams);
                }
                catch
                {
                }
            }
            return exc;
        }

        internal static void ValidateEnd(Task task)
        {
            if (task.Status != TaskStatus.RanToCompletion)
            {
                TaskAwaiter.HandleNonSuccess(task);
            }
        }

        private static MethodInfo GetPrepForRemotingMethodInfo()
        {
            MethodInfo result;
            try
            {
                result = typeof(Exception).GetMethod("PrepForRemoting", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            catch
            {
                result = null;
            }
            return result;
        }

        private static void HandleNonSuccess(Task task)
        {
            if (!task.IsCompleted)
            {
                try
                {
                    task.Wait();
                }
                catch
                {
                }
            }
            if (task.Status != TaskStatus.RanToCompletion)
            {
                TaskAwaiter.ThrowForNonSuccess(task);
            }
        }

        private static void RunNoException(Action continuation)
        {
            try
            {
                continuation();
            }
            catch (Exception exception)
            {
                AsyncMethodBuilderCore.ThrowAsync(exception, null);
            }
        }

        private static void ThrowForNonSuccess(Task task)
        {
            switch (task.Status)
            {
                case TaskStatus.Canceled:
                    throw new TaskCanceledException(task);
                case TaskStatus.Faulted:
                    throw TaskAwaiter.PrepareExceptionForRethrow(task.Exception.InnerException);
                default:
                    throw new InvalidOperationException("The task has not yet completed.");
            }
        }
    }

    public struct TaskAwaiter<TResult> : ICriticalNotifyCompletion, INotifyCompletion
    {
        private readonly Task<TResult> m_task;

        internal TaskAwaiter(Task<TResult> task)
        {
            this.m_task = task;
        }

        public bool IsCompleted
        {
            get
            {
                return this.m_task.IsCompleted;
            }
        }

        public TResult GetResult()
        {
            TaskAwaiter.ValidateEnd(this.m_task);
            return this.m_task.Result;
        }

        public void OnCompleted(Action continuation)
        {
            TaskAwaiter.OnCompletedInternal(this.m_task, continuation, true);
        }

        [SecurityCritical]
        public void UnsafeOnCompleted(Action continuation)
        {
            TaskAwaiter.OnCompletedInternal(this.m_task, continuation, true);
        }
    }
}
