using System.Diagnostics;

namespace System.Threading
{
    [DebuggerDisplay("IsCancellationRequested = {IsCancellationRequested}")]
    public struct CancellationToken
    {
        private readonly CancellationTokenSource source;

        public CancellationToken(bool canceled)
            : this(canceled ? CancellationTokenSource.CanceledSource : null)
        {
        }

        internal CancellationToken(CancellationTokenSource source)
        {
            this.source = source;
        }

        public static CancellationToken None
        {
            get
            {
                return new CancellationToken();
            }
        }

        public bool CanBeCanceled
        {
            get
            {
                return source != null;
            }
        }

        public bool IsCancellationRequested
        {
            get
            {
                return Source.IsCancellationRequested;
            }
        }

        public WaitHandle WaitHandle
        {
            get
            {
                return Source.WaitHandle;
            }
        }

        CancellationTokenSource Source
        {
            get
            {
                return source ?? CancellationTokenSource.NoneSource;
            }
        }

        public static bool operator !=(CancellationToken left, CancellationToken right)
        {
            return !left.Equals(right);
        }

        public static bool operator ==(CancellationToken left, CancellationToken right)
        {
            return left.Equals(right);
        }

        public bool Equals(CancellationToken other)
        {
            return this.Source == other.Source;
        }

        public override bool Equals(object other)
        {
            return (other is CancellationToken) ? Equals((CancellationToken)other) : false;
        }

        public override int GetHashCode()
        {
            return Source.GetHashCode();
        }

        public CancellationTokenRegistration Register(Action callback)
        {
            return Register(callback, false);
        }

        public CancellationTokenRegistration Register(Action callback, bool useSynchronizationContext)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            return Source.Register(callback, useSynchronizationContext);
        }

        public CancellationTokenRegistration Register(Action<object> callback, object state)
        {
            return Register(callback, state, false);
        }

        public CancellationTokenRegistration Register(Action<object> callback, object state, bool useSynchronizationContext)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            return Register(() => callback(state), useSynchronizationContext);
        }

        public void ThrowIfCancellationRequested()
        {
            if (source != null && source.IsCancellationRequested)
                throw new System.Couchbase.OperationCanceledException(this);
        }
    }
}
