namespace System.Threading
{
    public struct CancellationTokenRegistration : IDisposable, IEquatable<CancellationTokenRegistration>
    {
        private readonly int id;
        private readonly CancellationTokenSource source;

        internal CancellationTokenRegistration(int id, CancellationTokenSource source)
        {
            this.id = id;
            this.source = source;
        }

        public static bool operator !=(CancellationTokenRegistration left, CancellationTokenRegistration right)
        {
            return !left.Equals(right);
        }

        public static bool operator ==(CancellationTokenRegistration left, CancellationTokenRegistration right)
        {
            return left.Equals(right);
        }

        public void Dispose()
        {
            if (source != null)
                source.RemoveCallback(this);
        }

        public bool Equals(CancellationTokenRegistration other)
        {
            return id == other.id && source == other.source;
        }

        public override bool Equals(object obj)
        {
            return (obj is CancellationTokenRegistration) && Equals((CancellationTokenRegistration)obj);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode() ^ (source == null ? 0 : source.GetHashCode());
        }
    }
}
