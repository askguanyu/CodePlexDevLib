namespace System.Threading
{
    public struct SpinWait
    {
        private const int maxTime = 200;

        private const int step = 10;

        private static readonly bool isSingleCpu = (Environment.ProcessorCount == 1);

        private int ntime;

        public int Count
        {
            get
            {
                return ntime;
            }
        }

        public bool NextSpinWillYield
        {
            get
            {
                return isSingleCpu ? true : ntime % step == 0;
            }
        }

        public static void SpinUntil(Func<bool> condition)
        {
            SpinWait sw = new SpinWait();
            while (!condition())
                sw.SpinOnce();
        }

        public static bool SpinUntil(Func<bool> condition, TimeSpan timeout)
        {
            return SpinUntil(condition, (int)timeout.TotalMilliseconds);
        }

        public static bool SpinUntil(Func<bool> condition, int millisecondsTimeout)
        {
            SpinWait sw = new SpinWait();
            Watch watch = Watch.StartNew();

            while (!condition())
            {
                if (watch.ElapsedMilliseconds > millisecondsTimeout)
                    return false;
                sw.SpinOnce();
            }

            return true;
        }

        public void Reset()
        {
            ntime = 0;
        }

        public void SpinOnce()
        {
            ntime += 1;

            if (isSingleCpu)
            {
                Thread.Sleep(0);
            }
            else
            {
                if (ntime % step == 0)
                    Thread.Sleep(0);
                else
                    Thread.SpinWait(Math.Min(ntime, maxTime) << 1);
            }
        }
    }
}
