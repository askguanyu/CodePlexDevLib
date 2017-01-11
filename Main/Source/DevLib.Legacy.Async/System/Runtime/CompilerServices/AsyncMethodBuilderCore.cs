using System.Diagnostics;
using System.Security;
using System.Threading;

namespace System.Runtime.CompilerServices
{
    internal struct AsyncMethodBuilderCore
    {
        internal IAsyncStateMachine m_stateMachine;

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            if (stateMachine == null)
            {
                throw new ArgumentNullException("stateMachine");
            }
            if (this.m_stateMachine != null)
            {
                throw new InvalidOperationException("The builder was not properly initialized.");
            }
            this.m_stateMachine = stateMachine;
        }

        internal static void ThrowAsync(Exception exception, SynchronizationContext targetContext)
        {
            if (targetContext != null)
            {
                try
                {
                    targetContext.Post(delegate(object state)
                    {
                        throw TaskAwaiter.PrepareExceptionForRethrow((Exception)state);
                    }, exception);
                    return;
                }
                catch (Exception ex)
                {
                    exception = new AggregateException(new Exception[]
                    {
                        exception,
                        ex
                    });
                }
            }
            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                throw TaskAwaiter.PrepareExceptionForRethrow((Exception)state);
            }, exception);
        }

        [SecuritySafeCritical]
        internal Action GetCompletionAction<TMethodBuilder, TStateMachine>(ref TMethodBuilder builder, ref TStateMachine stateMachine)
            where TMethodBuilder : IAsyncMethodBuilder
            where TStateMachine : IAsyncStateMachine
        {
            ExecutionContext context = ExecutionContext.Capture();
            AsyncMethodBuilderCore.MoveNextRunner moveNextRunner = new AsyncMethodBuilderCore.MoveNextRunner(context);
            Action result = new Action(moveNextRunner.Run);
            if (this.m_stateMachine == null)
            {
                builder.PreBoxInitialization();
                this.m_stateMachine = stateMachine;
                this.m_stateMachine.SetStateMachine(this.m_stateMachine);
            }
            moveNextRunner.m_stateMachine = this.m_stateMachine;
            return result;
        }

        [DebuggerStepThrough, SecuritySafeCritical]
        internal void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            if (stateMachine == null)
            {
                throw new ArgumentNullException("stateMachine");
            }
            stateMachine.MoveNext();
        }

        private sealed class MoveNextRunner
        {
            internal IAsyncStateMachine m_stateMachine;

            [SecurityCritical]
            private static ContextCallback s_invokeMoveNext;

            private readonly ExecutionContext m_context;

            [SecurityCritical]
            internal MoveNextRunner(ExecutionContext context)
            {
                this.m_context = context;
            }

            [SecuritySafeCritical]
            internal void Run()
            {
                if (this.m_context != null)
                {
                    try
                    {
                        ContextCallback contextCallback = AsyncMethodBuilderCore.MoveNextRunner.s_invokeMoveNext;
                        if (contextCallback == null)
                        {
                            contextCallback = (AsyncMethodBuilderCore.MoveNextRunner.s_invokeMoveNext = new ContextCallback(AsyncMethodBuilderCore.MoveNextRunner.InvokeMoveNext));
                        }
                        ExecutionContext.Run(this.m_context, contextCallback, this.m_stateMachine);
                        return;
                    }
                    finally
                    {
                        this.m_context.Dispose();
                    }
                }
                this.m_stateMachine.MoveNext();
            }

            [SecurityCritical]
            private static void InvokeMoveNext(object stateMachine)
            {
                ((IAsyncStateMachine)stateMachine).MoveNext();
            }
        }
    }
}
