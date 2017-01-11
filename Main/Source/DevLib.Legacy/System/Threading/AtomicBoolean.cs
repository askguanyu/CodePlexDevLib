namespace System.Threading
{
    internal struct AtomicBooleanValue
    {
        private const int Set = 1;
        private const int UnSet = 0;
        private int flag;

        public bool Value
        {
            get
            {
                return flag == Set;
            }
            set
            {
                Exchange(value);
            }
        }

        public static explicit operator bool(AtomicBooleanValue rhs)
        {
            return rhs.Value;
        }

        public static AtomicBooleanValue FromValue(bool value)
        {
            AtomicBooleanValue temp = new AtomicBooleanValue();
            temp.Value = value;

            return temp;
        }

        public static implicit operator AtomicBooleanValue(bool rhs)
        {
            return AtomicBooleanValue.FromValue(rhs);
        }

        public bool CompareAndExchange(bool expected, bool newVal)
        {
            int newTemp = newVal ? Set : UnSet;
            int expectedTemp = expected ? Set : UnSet;

            return Interlocked.CompareExchange(ref flag, newTemp, expectedTemp) == expectedTemp;
        }

        public bool Equals(AtomicBooleanValue rhs)
        {
            return this.flag == rhs.flag;
        }

        public override bool Equals(object rhs)
        {
            return rhs is AtomicBooleanValue ? Equals((AtomicBooleanValue)rhs) : false;
        }

        public bool Exchange(bool newVal)
        {
            int newTemp = newVal ? Set : UnSet;
            return Interlocked.Exchange(ref flag, newTemp) == Set;
        }

        public override int GetHashCode()
        {
            return flag.GetHashCode();
        }

        public bool TryRelaxedSet()
        {
            return flag == UnSet && !Exchange(true);
        }

        public bool TrySet()
        {
            return !Exchange(true);
        }
    }

    internal class AtomicBoolean
    {
        private const int Set = 1;
        private const int UnSet = 0;
        private int flag;

        public bool Value
        {
            get
            {
                return flag == Set;
            }
            set
            {
                Exchange(value);
            }
        }

        public static explicit operator bool(AtomicBoolean rhs)
        {
            return rhs.Value;
        }

        public static AtomicBoolean FromValue(bool value)
        {
            AtomicBoolean temp = new AtomicBoolean();
            temp.Value = value;

            return temp;
        }

        public static implicit operator AtomicBoolean(bool rhs)
        {
            return AtomicBoolean.FromValue(rhs);
        }

        public bool CompareAndExchange(bool expected, bool newVal)
        {
            int newTemp = newVal ? Set : UnSet;
            int expectedTemp = expected ? Set : UnSet;

            return Interlocked.CompareExchange(ref flag, newTemp, expectedTemp) == expectedTemp;
        }

        public bool Equals(AtomicBoolean rhs)
        {
            return this.flag == rhs.flag;
        }

        public override bool Equals(object rhs)
        {
            return rhs is AtomicBoolean ? Equals((AtomicBoolean)rhs) : false;
        }

        public bool Exchange(bool newVal)
        {
            int newTemp = newVal ? Set : UnSet;
            return Interlocked.Exchange(ref flag, newTemp) == Set;
        }

        public override int GetHashCode()
        {
            return flag.GetHashCode();
        }

        public bool TryRelaxedSet()
        {
            return flag == UnSet && !Exchange(true);
        }

        public bool TrySet()
        {
            return !Exchange(true);
        }
    }
}
