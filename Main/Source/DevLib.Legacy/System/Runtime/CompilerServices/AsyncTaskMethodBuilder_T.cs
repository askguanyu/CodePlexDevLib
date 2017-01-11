using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct AsyncTaskMethodBuilder<TResult>
    {
        private readonly Task<TResult> task;
        private IAsyncStateMachine stateMachine;

        private AsyncTaskMethodBuilder(Task<TResult> task)
        {
            this.task = task;
            this.stateMachine = null;
        }

        public Task<TResult> Task
        {
            get
            {
                return task;
            }
        }

        public static AsyncTaskMethodBuilder<TResult> Create()
        {
            var task = new Task<TResult>(TaskActionInvoker.Promise, null, CancellationToken.None, TaskCreationOptions.None, null);
            task.SetupScheduler(TaskScheduler.Current);
            return new AsyncTaskMethodBuilder<TResult>(task);
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            var action = new Action(stateMachine.MoveNext);
            awaiter.OnCompleted(action);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            var action = new Action(stateMachine.MoveNext);
            awaiter.UnsafeOnCompleted(action);
        }

        public void SetException(Exception exception)
        {
            if (Task.TrySetException(new AggregateException(exception), exception is OperationCanceledException, true))
                return;

            throw new InvalidOperationException("The task has already completed");
        }

        public void SetResult(TResult result)
        {
            if (!task.TrySetResult(result))
                throw new InvalidOperationException("The task has already completed");
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            if (stateMachine == null)
                throw new ArgumentNullException("stateMachine");

            if (this.stateMachine != null)
                throw new InvalidOperationException("The state machine was previously set");

            this.stateMachine = stateMachine;
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            if (stateMachine == null)
                throw new ArgumentNullException("stateMachine");

            stateMachine.MoveNext();
        }
    }
}
