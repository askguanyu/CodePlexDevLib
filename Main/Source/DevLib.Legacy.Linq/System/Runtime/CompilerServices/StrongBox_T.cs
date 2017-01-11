namespace System.Runtime.CompilerServices
{
    public class StrongBox<T> : IStrongBox
    {
        public T Value;

#if NET_4_0 || BOOTSTRAP_NET_4_0 || MOONLIGHT

        public StrongBox()
        {
        }

#endif

        public StrongBox(T value)
        {
            Value = value;
        }

        object IStrongBox.Value
        {
            get { return Value; }
            set { Value = (T)value; }
        }
    }
}
