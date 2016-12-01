using System.Collections.Generic;

namespace System.Threading.Tasks
{
    internal interface IContinuation
    {
        void Execute();
    }

    internal class AwaiterActionContinuation : IContinuation
    {
        private readonly Action action;

        public AwaiterActionContinuation(Action action)
        {
            this.action = action;
        }

        public void Execute()
        {
            if ((SynchronizationContext.Current == null || SynchronizationContext.Current.GetType() == typeof(SynchronizationContext)) && TaskScheduler.IsDefault)
            {
                action();
            }
            else
            {
                ThreadPool.QueueUserWorkItem(l => ((Action)l)(), action);
            }
        }
    }

    internal sealed class CountdownContinuation : IContinuation, IDisposable
    {
        private readonly CountdownEvent evt;
        private bool disposed;

        public CountdownContinuation(int initialCount)
        {
            this.evt = new CountdownEvent(initialCount);
        }

        public CountdownEvent Event
        {
            get
            {
                return evt;
            }
        }

        public void Dispose()
        {
            disposed = true;
            Thread.MemoryBarrier();

            evt.Dispose();
        }

        public void Execute()
        {
            if (!disposed)
                evt.Signal();
        }
    }

    internal sealed class DisposeContinuation : IContinuation
    {
        private readonly IDisposable instance;

        public DisposeContinuation(IDisposable instance)
        {
            this.instance = instance;
        }

        public void Execute()
        {
            instance.Dispose();
        }
    }

    internal sealed class ManualResetContinuation : IContinuation, IDisposable
    {
        private readonly ManualResetEventSlim evt;

        public ManualResetContinuation()
        {
            this.evt = new ManualResetEventSlim();
        }

        public ManualResetEventSlim Event
        {
            get
            {
                return evt;
            }
        }

        public void Dispose()
        {
            evt.Dispose();
        }

        public void Execute()
        {
            evt.Set();
        }
    }

    internal class SchedulerAwaitContinuation : IContinuation
    {
        private readonly Task task;

        public SchedulerAwaitContinuation(Task task)
        {
            this.task = task;
        }

        public void Execute()
        {
            task.RunSynchronouslyCore(task.scheduler, true);
        }
    }

    internal class SynchronizationContextContinuation : IContinuation
    {
        private readonly Action action;
        private readonly SynchronizationContext ctx;

        public SynchronizationContextContinuation(Action action, SynchronizationContext ctx)
        {
            this.action = action;
            this.ctx = ctx;
        }

        public void Execute()
        {
            if (ctx == SynchronizationContext.Current)
                action();
            else
                ctx.Post(l => ((Action)l)(), action);
        }
    }

    internal class TaskContinuation : IContinuation
    {
        private readonly TaskContinuationOptions continuationOptions;
        private readonly Task task;

        public TaskContinuation(Task task, TaskContinuationOptions continuationOptions)
        {
            this.task = task;
            this.continuationOptions = continuationOptions;
        }

        public void Execute()
        {
            if (!ContinuationStatusCheck(continuationOptions))
            {
                task.CancelReal(notifyParent: true);
                task.Dispose();
                return;
            }

            if (task.IsCompleted)
                return;

            if ((continuationOptions & TaskContinuationOptions.ExecuteSynchronously) != 0)
                task.RunSynchronouslyCore(task.scheduler, false);
            else
                task.Schedule(false);
        }

        private bool ContinuationStatusCheck(TaskContinuationOptions kind)
        {
            if (kind == TaskContinuationOptions.None)
                return true;

            int kindCode = (int)kind;
            var status = task.ContinuationAncestor.Status;

            if (kindCode >= ((int)TaskContinuationOptions.NotOnRanToCompletion))
            {
                kind &= ~(TaskContinuationOptions.PreferFairness
                          | TaskContinuationOptions.LongRunning
                          | TaskContinuationOptions.AttachedToParent
                          | TaskContinuationOptions.ExecuteSynchronously);

                if (status == TaskStatus.Canceled)
                {
                    if (kind == TaskContinuationOptions.NotOnCanceled)
                        return false;
                    if (kind == TaskContinuationOptions.OnlyOnFaulted)
                        return false;
                    if (kind == TaskContinuationOptions.OnlyOnRanToCompletion)
                        return false;
                }
                else if (status == TaskStatus.Faulted)
                {
                    if (kind == TaskContinuationOptions.NotOnFaulted)
                        return false;
                    if (kind == TaskContinuationOptions.OnlyOnCanceled)
                        return false;
                    if (kind == TaskContinuationOptions.OnlyOnRanToCompletion)
                        return false;
                }
                else if (status == TaskStatus.RanToCompletion)
                {
                    if (kind == TaskContinuationOptions.NotOnRanToCompletion)
                        return false;
                    if (kind == TaskContinuationOptions.OnlyOnFaulted)
                        return false;
                    if (kind == TaskContinuationOptions.OnlyOnCanceled)
                        return false;
                }
            }

            return true;
        }
    }

    internal sealed class WhenAllContinuation : IContinuation
    {
        private readonly Task owner;
        private readonly IList<Task> tasks;
        private int counter;

        public WhenAllContinuation(Task owner, IList<Task> tasks)
        {
            this.owner = owner;
            this.counter = tasks.Count;
            this.tasks = tasks;
        }

        public void Execute()
        {
            if (Interlocked.Decrement(ref counter) != 0)
                return;

            owner.Status = TaskStatus.Running;

            bool canceled = false;
            List<Exception> exceptions = null;
            foreach (var task in tasks)
            {
                if (task.IsFaulted)
                {
                    if (exceptions == null)
                        exceptions = new List<Exception>();

                    exceptions.AddRange(task.Exception.InnerExceptions);
                    continue;
                }

                if (task.IsCanceled)
                {
                    canceled = true;
                }
            }

            if (exceptions != null)
            {
                owner.TrySetException(new AggregateException(exceptions), false, false);
                return;
            }

            if (canceled)
            {
                owner.CancelReal();
                return;
            }

            owner.Finish();
        }
    }

    internal sealed class WhenAllContinuation<TResult> : IContinuation
    {
        private readonly Task<TResult[]> owner;
        private readonly IList<Task<TResult>> tasks;
        private int counter;

        public WhenAllContinuation(Task<TResult[]> owner, IList<Task<TResult>> tasks)
        {
            this.owner = owner;
            this.counter = tasks.Count;
            this.tasks = tasks;
        }

        public void Execute()
        {
            if (Interlocked.Decrement(ref counter) != 0)
                return;

            bool canceled = false;
            List<Exception> exceptions = null;
            TResult[] results = null;
            for (int i = 0; i < tasks.Count; ++i)
            {
                var task = tasks[i];
                if (task.IsFaulted)
                {
                    if (exceptions == null)
                        exceptions = new List<Exception>();

                    exceptions.AddRange(task.Exception.InnerExceptions);
                    continue;
                }

                if (task.IsCanceled)
                {
                    canceled = true;
                    continue;
                }

                if (results == null)
                {
                    if (canceled || exceptions != null)
                        continue;

                    results = new TResult[tasks.Count];
                }

                results[i] = task.Result;
            }

            if (exceptions != null)
            {
                owner.TrySetException(new AggregateException(exceptions), false, false);
                return;
            }

            if (canceled)
            {
                owner.CancelReal();
                return;
            }

            owner.TrySetResult(results);
        }
    }

    internal sealed class WhenAnyContinuation<T> : IContinuation where T : Task
    {
        private readonly Task<T> owner;
        private readonly IList<T> tasks;
        private AtomicBooleanValue executed;

        public WhenAnyContinuation(Task<T> owner, IList<T> tasks)
        {
            this.owner = owner;
            this.tasks = tasks;
            executed = new AtomicBooleanValue();
        }

        public void Execute()
        {
            if (!executed.TryRelaxedSet())
                return;

            bool owner_notified = false;
            for (int i = 0; i < tasks.Count; ++i)
            {
                var task = tasks[i];
                if (!task.IsCompleted)
                {
                    task.RemoveContinuation(this);
                    continue;
                }

                if (owner_notified)
                    continue;

                owner.TrySetResult(task);
                owner_notified = true;
            }
        }
    }
}
