using System.Collections.Generic;

namespace System.Threading.Tasks
{
    internal class CircularArray<T>
    {
        public readonly T[] segment;
        public readonly int size;
        private readonly int baseSize;

        public CircularArray(int baseSize)
        {
            this.baseSize = baseSize;
            this.size = 1 << baseSize;
            this.segment = new T[size];
        }

        public int Size
        {
            get
            {
                return size;
            }
        }

        public T this[int index]
        {
            get
            {
                return segment[index % size];
            }
            set
            {
                segment[index % size] = value;
            }
        }

        public IEnumerable<T> GetEnumerable(int bottom, ref int top)
        {
            int instantTop = top;
            T[] slice = new T[bottom - instantTop];
            int destIndex = -1;
            for (int i = instantTop; i < bottom; i++)
                slice[++destIndex] = segment[i % size];

            return RealGetEnumerable(slice, bottom, top, instantTop);
        }

        public CircularArray<T> Grow(int bottom, int top)
        {
            var grow = new CircularArray<T>(baseSize + 1);

            for (int i = top; i < bottom; i++)
            {
                grow.segment[i] = segment[i % size];
            }

            return grow;
        }

        private IEnumerable<T> RealGetEnumerable(T[] slice, int bottom, int realTop, int initialTop)
        {
            int destIndex = (int)(realTop - initialTop - 1);
            for (int i = realTop; i < bottom; ++i)
                yield return slice[++destIndex];
        }
    }

    internal class CyclicDeque<T> : IConcurrentDeque<T>
    {
        private const int BaseSize = 11;

        private CircularArray<T> array = new CircularArray<T>(BaseSize);
        private int bottom;
        private int top;
        private int upperBound;

        public bool IsEmpty
        {
            get
            {
                int t = top;
                int b = bottom;
                return b - t <= 0;
            }
        }

        public IEnumerable<T> GetEnumerable()
        {
            var a = array;
            return a.GetEnumerable(bottom, ref top);
        }

        public bool PeekBottom(out T obj)
        {
            obj = default(T);

            int b = Interlocked.Decrement(ref bottom);
            var a = array;
            int t = top;
            int size = b - t;

            if (size < 0)
                return false;

            obj = a.segment[b % a.size];
            return true;
        }

        public PopResult PopBottom(out T obj)
        {
            obj = default(T);

            int b = Interlocked.Decrement(ref bottom);
            var a = array;
            int t = top;
            int size = b - t;

            if (size < 0)
            {
                Interlocked.Add(ref bottom, t - b);
                return PopResult.Empty;
            }

            obj = a.segment[b % a.size];
            if (size > 0)
                return PopResult.Succeed;
            Interlocked.Add(ref bottom, t + 1 - b);

            if (Interlocked.CompareExchange(ref top, t + 1, t) != t)
                return PopResult.Empty;

            return PopResult.Succeed;
        }

        public PopResult PopTop(out T obj)
        {
            obj = default(T);

            int t = top;
            int b = bottom;

            if (b - t <= 0)
                return PopResult.Empty;

            if (Interlocked.CompareExchange(ref top, t + 1, t) != t)
                return PopResult.Abort;

            var a = array;
            obj = a.segment[t % a.size];

            return PopResult.Succeed;
        }

        public void PushBottom(T obj)
        {
            int b = bottom;
            var a = array;

            var size = b - top - upperBound;
            if (size > a.Size)
            {
                upperBound = top;
                a = a.Grow(b, upperBound);
                array = a;
            }

            a.segment[b % a.size] = obj;
            Interlocked.Increment(ref bottom);
        }

        internal bool PeekTop(out T obj)
        {
            obj = default(T);

            int t = top;
            int b = bottom;

            if (b - t <= 0)
                return false;

            var a = array;
            obj = a.segment[t % a.size];

            return true;
        }
    }
}
