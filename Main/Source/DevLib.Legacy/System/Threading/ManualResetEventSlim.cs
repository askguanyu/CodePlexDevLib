namespace System.Threading
{
    [System.Diagnostics.DebuggerDisplayAttribute("Set = {IsSet}")]
    public class ManualResetEventSlim : IDisposable
    {
        internal AtomicBooleanValue disposed;
        private readonly int spinCount;

        private object handle;

        private int state;

        private int used;

        public ManualResetEventSlim()
            : this(false, 10)
        {
        }

        public ManualResetEventSlim(bool initialState)
            : this(initialState, 10)
        {
        }

        public ManualResetEventSlim(bool initialState, int spinCount)
        {
            if (spinCount < 0 || spinCount > 2047)
                throw new ArgumentOutOfRangeException("spinCount");

            this.state = initialState ? 1 : 0;
            this.spinCount = spinCount;
        }

        public bool IsSet
        {
            get
            {
                return (state & 1) == 1;
            }
        }

        public int SpinCount
        {
            get
            {
                return spinCount;
            }
        }

        public WaitHandle WaitHandle
        {
            get
            {
                ThrowIfDisposed();

                if (handle != null)
                    return Handle;

                var isSet = IsSet;
                var mre = new ManualResetEvent(IsSet);
                if (Interlocked.CompareExchange(ref handle, mre, null) == null)
                {
                    if (isSet != IsSet)
                    {
                        if (IsSet)
                        {
                            mre.Set();
                        }
                        else
                        {
                            mre.Reset();
                        }
                    }
                }
                else
                {
                    ((IDisposable)mre).Dispose();
                }

                return Handle;
            }
        }

        private ManualResetEvent Handle
        {
            get { return (ManualResetEvent)handle; }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Reset()
        {
            ThrowIfDisposed();

            var stamp = UpdateStateWithOp(false);
            if (handle != null)
                CommitChangeToHandle(stamp);
        }

        public void Set()
        {
            var stamp = UpdateStateWithOp(true);
            if (handle != null)
                CommitChangeToHandle(stamp);
        }

        public void Wait()
        {
            Wait(CancellationToken.None);
        }

        public bool Wait(int millisecondsTimeout)
        {
            return Wait(millisecondsTimeout, CancellationToken.None);
        }

        public bool Wait(TimeSpan timeout)
        {
            return Wait(CheckTimeout(timeout), CancellationToken.None);
        }

        public void Wait(CancellationToken cancellationToken)
        {
            Wait(Timeout.Infinite, cancellationToken);
        }

        public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (millisecondsTimeout < -1)
                throw new ArgumentOutOfRangeException("millisecondsTimeout");

            ThrowIfDisposed();

            if (!IsSet)
            {
                SpinWait wait = new SpinWait();

                while (!IsSet)
                {
                    if (wait.Count < spinCount)
                    {
                        wait.SpinOnce();
                        continue;
                    }

                    break;
                }

                cancellationToken.ThrowIfCancellationRequested();

                if (IsSet)
                    return true;

                WaitHandle handle = WaitHandle;

                if (cancellationToken.CanBeCanceled)
                {
                    var result = WaitHandle.WaitAny(new[] { handle, cancellationToken.WaitHandle }, millisecondsTimeout, false);
                    if (result == 1)
                        throw new System.Couchbase.OperationCanceledException(cancellationToken);
                    if (result == WaitHandle.WaitTimeout)
                        return false;
                }
                else
                {
                    if (!handle.WaitOne(millisecondsTimeout, false))
                        return false;
                }
            }

            return true;
        }

        public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            return Wait(CheckTimeout(timeout), cancellationToken);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed.TryRelaxedSet())
                return;

            if (handle != null)
            {
                var tmpHandle = Interlocked.Exchange(ref handle, null);
                if (used > 0)
                {
                    SpinWait wait = new SpinWait();
                    while (used > 0)
                        wait.SpinOnce();
                }
                ((IDisposable)tmpHandle).Dispose();
            }
        }

        private static int CheckTimeout(TimeSpan timeout)
        {
            try
            {
                return checked((int)timeout.TotalMilliseconds);
            }
            catch (System.OverflowException)
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
        }

        private void CommitChangeToHandle(long stamp)
        {
            Interlocked.Increment(ref used);
            var tmpHandle = Handle;
            if (tmpHandle != null)
            {
                if ((stamp & 1) == 1)
                    tmpHandle.Set();
                else
                    tmpHandle.Reset();

                int currentState;
                do
                {
                    currentState = state;
                    if (currentState != stamp && (stamp & 1) != (currentState & 1))
                    {
                        if ((currentState & 1) == 1)
                            tmpHandle.Set();
                        else
                            tmpHandle.Reset();
                    }
                } while (currentState != state);
            }
            Interlocked.Decrement(ref used);
        }

        private void ThrowIfDisposed()
        {
            if (disposed.Value)
                throw new ObjectDisposedException("ManualResetEventSlim");
        }

        private long UpdateStateWithOp(bool set)
        {
            int oldValue, newValue;
            do
            {
                oldValue = state;
                newValue = (int)(((oldValue >> 1) + 1) << 1) | (set ? 1 : 0);
            } while (Interlocked.CompareExchange(ref state, newValue, oldValue) != oldValue);
            return newValue;
        }
    }
}
