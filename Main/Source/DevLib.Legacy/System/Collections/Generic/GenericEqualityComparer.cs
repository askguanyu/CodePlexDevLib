namespace System.Collections.Generic.DevLib
{
    [Serializable]
    public sealed class GenericEqualityComparer<T> : IEqualityComparer, IEqualityComparer<T> where T : IEquatable<T>
    {
        public bool Equals(T x, T y)
        {
            if (x == null)
                return y == null;

            return x.Equals(y);
        }

#pragma warning disable 108
        public bool Equals(object x, object y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            if (!(x is T) || !(y is T))
            {
                return false;
            }

            return Equals((T)x, (T)y);
        }
#pragma warning restore

        public int GetHashCode(T obj)
        {
            if (obj == null)
                return 0;

            return obj.GetHashCode();
        }

        public int GetHashCode(object obj)
        {
            if (obj == null)
                return 0;

            if (!(obj is T))
                throw new ArgumentException("Argument is not compatible", "obj");

            return GetHashCode((T)obj);
        }
    }
}
