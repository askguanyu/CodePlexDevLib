using System.Threading;

namespace System.Runtime.CompilerServices
{
    public struct AsyncVoidMethodBuilder
    {
        private static readonly SynchronizationContext null_context = new SynchronizationContext();

        private readonly SynchronizationContext context;
        private IAsyncStateMachine stateMachine;

        private AsyncVoidMethodBuilder(SynchronizationContext context)
        {
            this.context = context;
            this.stateMachine = null;
        }

        public static AsyncVoidMethodBuilder Create()
        {
            var ctx = SynchronizationContext.Current ?? null_context;
            ctx.OperationStarted();

            return new AsyncVoidMethodBuilder(ctx);
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
            if (exception == null)
                throw new ArgumentNullException("exception");

            try
            {
                context.Post(l => { throw (Exception)l; }, exception);
            }
            finally
            {
                SetResult();
            }
        }

        public void SetResult()
        {
            context.OperationCompleted();
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
