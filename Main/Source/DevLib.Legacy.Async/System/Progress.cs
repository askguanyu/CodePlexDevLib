using System.Threading;

namespace System
{
    public class Progress<T> : IProgress<T>
    {
        private readonly Action<T> m_handler;
        private readonly SendOrPostCallback m_invokeHandlers;
        private readonly SynchronizationContext m_synchronizationContext;

        public Progress()
        {
            this.m_synchronizationContext = (SynchronizationContext.Current ?? ProgressStatics.DefaultContext);
            this.m_invokeHandlers = new SendOrPostCallback(this.InvokeHandlers);
        }

        public Progress(Action<T> handler)
            : this()
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            this.m_handler = handler;
        }

        public event ProgressEventHandler<T> ProgressChanged;

        void IProgress<T>.Report(T value)
        {
            this.OnReport(value);
        }

        protected virtual void OnReport(T value)
        {
            Action<T> handler = this.m_handler;
            ProgressEventHandler<T> progressChanged = this.ProgressChanged;
            if (handler != null || progressChanged != null)
            {
                this.m_synchronizationContext.Post(this.m_invokeHandlers, value);
            }
        }

        private void InvokeHandlers(object state)
        {
            T t = (T)((object)state);
            Action<T> handler = this.m_handler;
            ProgressEventHandler<T> progressChanged = this.ProgressChanged;
            if (handler != null)
            {
                handler(t);
            }
            if (progressChanged != null)
            {
                progressChanged(this, t);
            }
        }
    }
}
