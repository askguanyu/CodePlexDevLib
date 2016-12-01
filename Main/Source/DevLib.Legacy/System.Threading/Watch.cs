namespace System.Threading
{
    internal struct Watch
    {
        private long startTicks;

        public long ElapsedMilliseconds
        {
            get
            {
                return (TicksNow() - startTicks) / TimeSpan.TicksPerMillisecond;
            }
        }

        public static Watch StartNew()
        {
            Watch watch = new Watch();
            watch.Start();
            return watch;
        }

        public void Start()
        {
            startTicks = TicksNow();
        }

        public void Stop()
        {
        }

        private static long TicksNow()
        {
            return DateTime.Now.Ticks;
        }
    }
}
