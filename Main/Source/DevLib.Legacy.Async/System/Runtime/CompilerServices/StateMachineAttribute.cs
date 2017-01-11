namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    [Serializable]
    public class StateMachineAttribute : Attribute
    {
        public StateMachineAttribute(Type stateMachineType)
        {
            this.StateMachineType = stateMachineType;
        }

        public Type StateMachineType
        {
            get;
            private set;
        }
    }
}