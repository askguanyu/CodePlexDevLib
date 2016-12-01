namespace System.Threading.Tasks
{
    public class UnobservedTaskExceptionEventArgs : EventArgs
    {
        private AggregateException exception;
        private bool wasObserved;

        public UnobservedTaskExceptionEventArgs(AggregateException exception)
        {
            this.exception = exception;
        }

        public AggregateException Exception
        {
            get
            {
                return exception;
            }
        }

        public bool Observed
        {
            get
            {
                return wasObserved;
            }
        }

        public void SetObserved()
        {
            wasObserved = true;
        }
    }
}
