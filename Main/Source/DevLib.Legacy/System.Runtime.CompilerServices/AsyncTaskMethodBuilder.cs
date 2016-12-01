using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct AsyncTaskMethodBuilder
    {
        private readonly Task<object> task;
        private IAsyncStateMachine stateMachine;

        private AsyncTaskMethodBuilder(Task<object> task)
        {
            this.task = task;
            this.stateMachine = null;
        }

        public Task Task
        {
            get
            {
                return task;
            }
        }

        public static AsyncTaskMethodBuilder Create()
        {
            var task = new Task<object>(TaskActionInvoker.Promise, null, CancellationToken.None, TaskCreationOptions.None, null);
            task.SetupScheduler(TaskScheduler.Current);
            return new AsyncTaskMethodBuilder(task);
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

        public void SetResult()
        {
            if (!task.TrySetResult(null))
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
