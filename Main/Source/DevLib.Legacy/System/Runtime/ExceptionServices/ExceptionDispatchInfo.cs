namespace System.Runtime.ExceptionServices
{
    public sealed class ExceptionDispatchInfo
    {
        private readonly Exception exception;

        private ExceptionDispatchInfo(Exception source)
        {
            this.exception = source;
        }

        public Exception SourceException
        {
            get
            {
                return exception;
            }
        }

        public static ExceptionDispatchInfo Capture(Exception source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return new ExceptionDispatchInfo(source);
        }

        public void Throw()
        {
            throw exception;
        }
    }
}
