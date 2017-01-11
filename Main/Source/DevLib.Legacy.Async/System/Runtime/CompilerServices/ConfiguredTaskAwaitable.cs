using System.Security;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct ConfiguredTaskAwaitable
    {
        private readonly ConfiguredTaskAwaitable.ConfiguredTaskAwaiter m_configuredTaskAwaiter;

        internal ConfiguredTaskAwaitable(Task task, bool continueOnCapturedContext)
        {
            this.m_configuredTaskAwaiter = new ConfiguredTaskAwaitable.ConfiguredTaskAwaiter(task, continueOnCapturedContext);
        }

        public ConfiguredTaskAwaitable.ConfiguredTaskAwaiter GetAwaiter()
        {
            return this.m_configuredTaskAwaiter;
        }

        public struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion, INotifyCompletion
        {
            private readonly bool m_continueOnCapturedContext;
            private readonly Task m_task;

            internal ConfiguredTaskAwaiter(Task task, bool continueOnCapturedContext)
            {
                this.m_task = task;
                this.m_continueOnCapturedContext = continueOnCapturedContext;
            }

            public bool IsCompleted
            {
                get
                {
                    return this.m_task.IsCompleted;
                }
            }

            public void GetResult()
            {
                TaskAwaiter.ValidateEnd(this.m_task);
            }

            public void OnCompleted(Action continuation)
            {
                TaskAwaiter.OnCompletedInternal(this.m_task, continuation, this.m_continueOnCapturedContext);
            }

            [SecurityCritical]
            public void UnsafeOnCompleted(Action continuation)
            {
                TaskAwaiter.OnCompletedInternal(this.m_task, continuation, true);
            }
        }
    }

    public struct ConfiguredTaskAwaitable<TResult>
    {
        private readonly ConfiguredTaskAwaitable<TResult>.ConfiguredTaskAwaiter m_configuredTaskAwaiter;

        internal ConfiguredTaskAwaitable(Task<TResult> task, bool continueOnCapturedContext)
        {
            this.m_configuredTaskAwaiter = new ConfiguredTaskAwaitable<TResult>.ConfiguredTaskAwaiter(task, continueOnCapturedContext);
        }

        public ConfiguredTaskAwaitable<TResult>.ConfiguredTaskAwaiter GetAwaiter()
        {
            return this.m_configuredTaskAwaiter;
        }

        public struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion, INotifyCompletion
        {
            private readonly bool m_continueOnCapturedContext;
            private readonly Task<TResult> m_task;

            internal ConfiguredTaskAwaiter(Task<TResult> task, bool continueOnCapturedContext)
            {
                this.m_task = task;
                this.m_continueOnCapturedContext = continueOnCapturedContext;
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
                TaskAwaiter.OnCompletedInternal(this.m_task, continuation, this.m_continueOnCapturedContext);
            }

            [SecurityCritical]
            public void UnsafeOnCompleted(Action continuation)
            {
                TaskAwaiter.OnCompletedInternal(this.m_task, continuation, true);
            }
        }
    }
}
