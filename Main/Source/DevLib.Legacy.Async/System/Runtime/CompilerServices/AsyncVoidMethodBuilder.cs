using System.Diagnostics;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct AsyncVoidMethodBuilder : IAsyncMethodBuilder
    {
        private static int s_preventUnobservedTaskExceptionsInvoked;
        private readonly SynchronizationContext m_synchronizationContext;

        private AsyncMethodBuilderCore m_coreState;

        private object m_objectIdForDebugger;

        static AsyncVoidMethodBuilder()
        {
            try
            {
                AsyncVoidMethodBuilder.PreventUnobservedTaskExceptions();
            }
            catch
            {
            }
        }

        private AsyncVoidMethodBuilder(SynchronizationContext synchronizationContext)
        {
            this.m_synchronizationContext = synchronizationContext;
            if (synchronizationContext != null)
            {
                synchronizationContext.OperationStarted();
            }
            this.m_coreState = default(AsyncMethodBuilderCore);
            this.m_objectIdForDebugger = null;
        }

        private object ObjectIdForDebugger
        {
            get
            {
                if (this.m_objectIdForDebugger == null)
                {
                    this.m_objectIdForDebugger = new object();
                }
                return this.m_objectIdForDebugger;
            }
        }

        public static AsyncVoidMethodBuilder Create()
        {
            return new AsyncVoidMethodBuilder(SynchronizationContext.Current);
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            try
            {
                Action completionAction = this.m_coreState.GetCompletionAction<AsyncVoidMethodBuilder, TStateMachine>(ref this, ref stateMachine);
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
                Action completionAction = this.m_coreState.GetCompletionAction<AsyncVoidMethodBuilder, TStateMachine>(ref this, ref stateMachine);
                awaiter.UnsafeOnCompleted(completionAction);
            }
            catch (Exception exception)
            {
                AsyncMethodBuilderCore.ThrowAsync(exception, null);
            }
        }

        void IAsyncMethodBuilder.PreBoxInitialization()
        {
        }

        public void SetException(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            if (this.m_synchronizationContext != null)
            {
                try
                {
                    AsyncMethodBuilderCore.ThrowAsync(exception, this.m_synchronizationContext);
                    return;
                }
                finally
                {
                    this.NotifySynchronizationContextOfCompletion();
                }
            }
            AsyncMethodBuilderCore.ThrowAsync(exception, null);
        }

        public void SetResult()
        {
            if (this.m_synchronizationContext != null)
            {
                this.NotifySynchronizationContextOfCompletion();
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

        internal static void PreventUnobservedTaskExceptions()
        {
            if (Interlocked.CompareExchange(ref AsyncVoidMethodBuilder.s_preventUnobservedTaskExceptionsInvoked, 1, 0) == 0)
            {
                TaskScheduler.UnobservedTaskException += delegate(object s, UnobservedTaskExceptionEventArgs e)
                {
                    e.SetObserved();
                };
            }
        }

        private void NotifySynchronizationContextOfCompletion()
        {
            try
            {
                this.m_synchronizationContext.OperationCompleted();
            }
            catch (Exception exception)
            {
                AsyncMethodBuilderCore.ThrowAsync(exception, null);
            }
        }
    }
}
