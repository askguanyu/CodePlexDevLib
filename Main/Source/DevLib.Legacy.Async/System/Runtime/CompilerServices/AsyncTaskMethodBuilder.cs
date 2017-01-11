using System.Diagnostics;
using System.Security;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct AsyncTaskMethodBuilder : IAsyncMethodBuilder
    {
        private static readonly TaskCompletionSource<VoidTaskResult> s_cachedCompleted = AsyncTaskMethodBuilder<VoidTaskResult>.s_defaultResultTask;

        private AsyncTaskMethodBuilder<VoidTaskResult> m_builder;

        public Task Task
        {
            get
            {
                return this.m_builder.Task;
            }
        }

        private object ObjectIdForDebugger
        {
            get
            {
                return this.Task;
            }
        }

        public static AsyncTaskMethodBuilder Create()
        {
            return default(AsyncTaskMethodBuilder);
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            this.m_builder.AwaitOnCompleted<TAwaiter, TStateMachine>(ref awaiter, ref stateMachine);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            this.m_builder.AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref awaiter, ref stateMachine);
        }

        void IAsyncMethodBuilder.PreBoxInitialization()
        {
            Task arg_06_0 = this.Task;
        }

        public void SetException(Exception exception)
        {
            this.m_builder.SetException(exception);
        }

        public void SetResult()
        {
            this.m_builder.SetResult(AsyncTaskMethodBuilder.s_cachedCompleted);
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            this.m_builder.SetStateMachine(stateMachine);
        }

        [DebuggerStepThrough]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            this.m_builder.Start<TStateMachine>(ref stateMachine);
        }

        internal void SetNotificationForWaitCompletion(bool enabled)
        {
            this.m_builder.SetNotificationForWaitCompletion(enabled);
        }
    }

    public struct AsyncTaskMethodBuilder<TResult> : IAsyncMethodBuilder
    {
        internal static readonly TaskCompletionSource<TResult> s_defaultResultTask;

        private AsyncMethodBuilderCore m_coreState;

        private TaskCompletionSource<TResult> m_task;

        static AsyncTaskMethodBuilder()
        {
            AsyncTaskMethodBuilder<TResult>.s_defaultResultTask = AsyncMethodTaskCache<TResult>.CreateCompleted(default(TResult));
            try
            {
                AsyncVoidMethodBuilder.PreventUnobservedTaskExceptions();
            }
            catch
            {
            }
        }

        public Task<TResult> Task
        {
            get
            {
                return this.CompletionSource.Task;
            }
        }

        internal TaskCompletionSource<TResult> CompletionSource
        {
            get
            {
                TaskCompletionSource<TResult> taskCompletionSource = this.m_task;
                if (taskCompletionSource == null)
                {
                    taskCompletionSource = (this.m_task = new TaskCompletionSource<TResult>());
                }
                return taskCompletionSource;
            }
        }

        private object ObjectIdForDebugger
        {
            get
            {
                return this.Task;
            }
        }

        public static AsyncTaskMethodBuilder<TResult> Create()
        {
            return default(AsyncTaskMethodBuilder<TResult>);
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            try
            {
                Action completionAction = this.m_coreState.GetCompletionAction<AsyncTaskMethodBuilder<TResult>, TStateMachine>(ref this, ref stateMachine);
                awaiter.OnCompleted(completionAction);
            }
            catch (Exception exception)
            {
                AsyncMethodBuilderCore.ThrowAsync(exception, null);
            }
        }

        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            try
            {
                Action completionAction = this.m_coreState.GetCompletionAction<AsyncTaskMethodBuilder<TResult>, TStateMachine>(ref this, ref stateMachine);
                awaiter.UnsafeOnCompleted(completionAction);
            }
            catch (Exception exception)
            {
                AsyncMethodBuilderCore.ThrowAsync(exception, null);
            }
        }

        void IAsyncMethodBuilder.PreBoxInitialization()
        {
            Task<TResult> arg_06_0 = this.Task;
        }

        public void SetException(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            TaskCompletionSource<TResult> completionSource = this.CompletionSource;
            if (!((exception is OperationCanceledException) ? completionSource.TrySetCanceled() : completionSource.TrySetException(exception)))
            {
                throw new InvalidOperationException("The Task was already completed.");
            }
        }

        public void SetResult(TResult result)
        {
            TaskCompletionSource<TResult> task = this.m_task;
            if (task == null)
            {
                this.m_task = this.GetTaskForResult(result);
                return;
            }
            if (!task.TrySetResult(result))
            {
                throw new InvalidOperationException("The Task was already completed.");
            }
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            this.m_coreState.SetStateMachine(stateMachine);
        }

        [DebuggerStepThrough]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            this.m_coreState.Start<TStateMachine>(ref stateMachine);
        }

        internal void SetNotificationForWaitCompletion(bool enabled)
        {
        }

        internal void SetResult(TaskCompletionSource<TResult> completedTask)
        {
            if (this.m_task == null)
            {
                this.m_task = completedTask;
                return;
            }
            this.SetResult(default(TResult));
        }

        private TaskCompletionSource<TResult> GetTaskForResult(TResult result)
        {
            AsyncMethodTaskCache<TResult> singleton = AsyncMethodTaskCache<TResult>.Singleton;
            if (singleton == null)
            {
                return AsyncMethodTaskCache<TResult>.CreateCompleted(result);
            }
            return singleton.FromResult(result);
        }
    }
}
