using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Generic.DevLib;

namespace System.Threading
{
    public class CancellationTokenSource : IDisposable
    {
        internal static readonly CancellationTokenSource CanceledSource = new CancellationTokenSource();
        internal static readonly CancellationTokenSource NoneSource = new CancellationTokenSource();
        private const int StateCanceled = 1 << 1;
        private const int StateDisposed = 1 << 2;
        private const int StateValid = 0;
        private static readonly TimerCallback timer_callback;
        private object _linkedTokens;
        private ConcurrentDictionary<CancellationTokenRegistration, Action> callbacks;
        private int currId = int.MinValue;
        private ManualResetEvent handle;
        private int state;
        private object timer;

        static CancellationTokenSource()
        {
            CanceledSource.state = StateCanceled;

            timer_callback = token =>
            {
                var cts = (CancellationTokenSource)token;
                cts.CancelSafe();
            };
        }

        public CancellationTokenSource()
        {
            callbacks = new ConcurrentDictionary<CancellationTokenRegistration, Action>(new GenericEqualityComparer<CancellationTokenRegistration>());
            handle = new ManualResetEvent(false);
        }

        public CancellationTokenSource(int millisecondsDelay)
            : this()
        {
            if (millisecondsDelay < -1)
                throw new ArgumentOutOfRangeException("millisecondsDelay");

            if (millisecondsDelay != Timeout.Infinite)
                timer = new Timer(timer_callback, this, millisecondsDelay, Timeout.Infinite);
        }

        public CancellationTokenSource(TimeSpan delay)
            : this(CheckTimeout(delay))
        {
        }

        public bool IsCancellationRequested
        {
            get
            {
                return (state & StateCanceled) != 0;
            }
        }

        public CancellationToken Token
        {
            get
            {
                CheckDisposed();
                return new CancellationToken(this);
            }
        }

        internal WaitHandle WaitHandle
        {
            get
            {
                CheckDisposed();
                return handle;
            }
        }

        private CancellationTokenRegistration[] linkedTokens
        {
            get { return (CancellationTokenRegistration[])_linkedTokens; }
        }

        private Timer Timer
        {
            get { return (Timer)timer; }
        }

        public static CancellationTokenSource CreateLinkedTokenSource(CancellationToken token1, CancellationToken token2)
        {
            return CreateLinkedTokenSource(new[] { token1, token2 });
        }

        public static CancellationTokenSource CreateLinkedTokenSource(params CancellationToken[] tokens)
        {
            if (tokens == null)
                throw new ArgumentNullException("tokens");

            if (tokens.Length == 0)
                throw new ArgumentException("Empty tokens array");

            CancellationTokenSource src = new CancellationTokenSource();
            Action action = src.CancelSafe;
            var registrations = new List<CancellationTokenRegistration>(tokens.Length);

            foreach (CancellationToken token in tokens)
            {
                if (token.CanBeCanceled)
                    registrations.Add(token.Register(action));
            }
            src._linkedTokens = registrations.ToArray();

            return src;
        }

        public void Cancel()
        {
            Cancel(false);
        }

        public void Cancel(bool throwOnFirstException)
        {
            CheckDisposed();
            Cancellation(throwOnFirstException);
        }

        public void CancelAfter(TimeSpan delay)
        {
            CancelAfter(CheckTimeout(delay));
        }

        public void CancelAfter(int millisecondsDelay)
        {
            if (millisecondsDelay < -1)
                throw new ArgumentOutOfRangeException("millisecondsDelay");

            CheckDisposed();

            if (IsCancellationRequested || millisecondsDelay == Timeout.Infinite)
                return;

            if (timer == null)
            {
                var t = new Timer(timer_callback, this, Timeout.Infinite, Timeout.Infinite);
                if (Interlocked.CompareExchange(ref timer, t, null) != null)
                    t.Dispose();
            }

            Timer.Change(millisecondsDelay, Timeout.Infinite);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        internal CancellationTokenRegistration Register(Action callback, bool useSynchronizationContext)
        {
            CheckDisposed();

            var tokenReg = new CancellationTokenRegistration(Interlocked.Increment(ref currId), this);

            if (IsCancellationRequested)
                callback();
            else
            {
                callbacks.TryAdd(tokenReg, callback);
                if (IsCancellationRequested && callbacks.TryRemove(tokenReg, out callback))
                    callback();
            }

            return tokenReg;
        }

        internal void RemoveCallback(CancellationTokenRegistration reg)
        {
            if ((state & StateDisposed) != 0)
                return;
            Action dummy;
            var cbs = callbacks;
            if (cbs != null)
                cbs.TryRemove(reg, out dummy);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && (state & StateDisposed) == 0)
            {
                if (Interlocked.CompareExchange(ref state, StateDisposed, StateValid) == StateValid)
                {
                    UnregisterLinkedTokens();
                    callbacks = null;
                }
                else
                {
                    if (handle != null)
                        handle.WaitOne();

                    state |= StateDisposed;
                    Thread.MemoryBarrier();
                }

                if (timer != null)
                    Timer.Dispose();

                ((IDisposable)handle).Dispose();
                handle = null;
            }
        }

        private static int CheckTimeout(TimeSpan delay)
        {
            try
            {
                return checked((int)delay.TotalMilliseconds);
            }
            catch (OverflowException)
            {
                throw new ArgumentOutOfRangeException("delay");
            }
        }

        private void Cancellation(bool throwOnFirstException)
        {
            if (Interlocked.CompareExchange(ref state, StateCanceled, StateValid) != StateValid)
                return;

            handle.Set();

            if (linkedTokens != null)
                UnregisterLinkedTokens();

            var cbs = callbacks;
            if (cbs == null)
                return;

            List<Exception> exceptions = null;

            try
            {
                Action cb;
                for (int id = currId; id != int.MinValue; id--)
                {
                    if (!cbs.TryRemove(new CancellationTokenRegistration(id, this), out cb))
                        continue;
                    if (cb == null)
                        continue;

                    if (throwOnFirstException)
                    {
                        cb();
                    }
                    else
                    {
                        try
                        {
                            cb();
                        }
                        catch (Exception e)
                        {
                            if (exceptions == null)
                                exceptions = new List<Exception>();

                            exceptions.Add(e);
                        }
                    }
                }
            }
            finally
            {
                cbs.Clear();
            }

            if (exceptions != null)
                throw new AggregateException(exceptions);
        }

        private void CancelSafe()
        {
            if (state == StateValid)
                Cancellation(true);
        }

        private void CheckDisposed()
        {
            if ((state & StateDisposed) != 0)
                throw new ObjectDisposedException(GetType().Name);
        }

        private void UnregisterLinkedTokens()
        {
            var registrations = Interlocked.Exchange(ref _linkedTokens, null);
            if (registrations == null)
                return;
            foreach (var linked in (CancellationTokenRegistration[])registrations)
                linked.Dispose();
        }
    }
}
