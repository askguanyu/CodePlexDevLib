using System.Threading;

namespace System
{
    internal static class ProgressStatics
    {
        internal static readonly SynchronizationContext DefaultContext = new SynchronizationContext();
    }
}
