namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    [Serializable]
    public sealed class IteratorStateMachineAttribute : StateMachineAttribute
    {
        public IteratorStateMachineAttribute(Type stateMachineType)
            : base(stateMachineType)
        {
        }
    }
}
