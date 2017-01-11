namespace System.Threading
{
    [System.Diagnostics.DebuggerDisplayAttribute("Initial Count={InitialCount}, Current Count={CurrentCount}")]
    public class CountdownEvent : IDisposable
    {
        private ManualResetEventSlim evt;
        private int initial;
        private int initialCount;

        public CountdownEvent(int initialCount)
        {
            if (initialCount < 0)
                throw new ArgumentOutOfRangeException("initialCount");

            evt = new ManualResetEventSlim(initialCount == 0);
            this.initial = this.initialCount = initialCount;
        }

        public int CurrentCount
        {
            get
            {
                return initialCount;
            }
        }

        public int InitialCount
        {
            get
            {
                return initial;
            }
        }

        public bool IsSet
        {
            get
            {
                return initialCount == 0;
            }
        }

        public WaitHandle WaitHandle
        {
            get
            {
                return evt.WaitHandle;
            }
        }

        public void AddCount()
        {
            AddCount(1);
        }

        public void AddCount(int signalCount)
        {
            if (!TryAddCount(signalCount))
                throw new InvalidOperationException("The event is already signaled and cannot be incremented");
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Reset()
        {
            Reset(initial);
        }

        public void Reset(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            CheckDisposed();

            initialCount = initial = count;
            if (count == 0)
                evt.Set();
            else
                evt.Reset();
        }

        public bool Signal()
        {
            return Signal(1);
        }

        public bool Signal(int signalCount)
        {
            if (signalCount <= 0)
                throw new ArgumentOutOfRangeException("signalCount");

            CheckDisposed();

            int newValue;
            if (!ApplyOperation(-signalCount, out newValue))
                throw new InvalidOperationException("The event is already set");

            if (newValue == 0)
            {
                evt.Set();
                return true;
            }

            return false;
        }

        public bool TryAddCount()
        {
            return TryAddCount(1);
        }

        public bool TryAddCount(int signalCount)
        {
            if (signalCount <= 0)
                throw new ArgumentOutOfRangeException("signalCount");

            CheckDisposed();

            int temp;
            return ApplyOperation(signalCount, out temp);
        }

        public void Wait()
        {
            evt.Wait();
        }

        public void Wait(CancellationToken cancellationToken)
        {
            evt.Wait(cancellationToken);
        }

        public bool Wait(int millisecondsTimeout)
        {
            return evt.Wait(millisecondsTimeout);
        }

        public bool Wait(TimeSpan timeout)
        {
            return evt.Wait(timeout);
        }

        public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            return evt.Wait(millisecondsTimeout, cancellationToken);
        }

        public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            return evt.Wait(timeout, cancellationToken);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                evt.Dispose();
        }

        private bool ApplyOperation(int num, out int newValue)
        {
            int oldCount;

            do
            {
                oldCount = initialCount;
                if (oldCount == 0)
                {
                    newValue = 0;
                    return false;
                }

                newValue = oldCount + num;

                if (newValue < 0)
                    return false;
            } while (Interlocked.CompareExchange(ref initialCount, newValue, oldCount) != oldCount);

            return true;
        }

        private void CheckDisposed()
        {
            if (evt.disposed.Value)
                throw new ObjectDisposedException("CountdownEvent");
        }
    }
}
