namespace System.Threading.Tasks
{
    [FlagsAttribute, SerializableAttribute]
    public enum TaskCreationOptions
    {
        None = 0x0,
        PreferFairness = 0x1,
        LongRunning = 0x2,
        AttachedToParent = 0x4,
        DenyChildAttach = 0x8,
        HideScheduler = 0x10
    }
}
