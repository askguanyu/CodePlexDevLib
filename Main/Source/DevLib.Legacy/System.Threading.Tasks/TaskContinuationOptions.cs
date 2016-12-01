namespace System.Threading.Tasks
{
    [System.FlagsAttribute, System.SerializableAttribute]
    public enum TaskContinuationOptions
    {
        None = 0x00000,
        PreferFairness = 0x00001,
        LongRunning = 0x00002,
        AttachedToParent = 0x00004,
        DenyChildAttach = 0x00008,
        HideScheduler = 0x00010,
        LazyCancellation = 0x00020,
        NotOnRanToCompletion = 0x10000,
        NotOnFaulted = 0x20000,
        NotOnCanceled = 0x40000,
        OnlyOnRanToCompletion = 0x60000,
        OnlyOnFaulted = 0x50000,
        OnlyOnCanceled = 0x30000,
        ExecuteSynchronously = 0x80000,
    }
}
