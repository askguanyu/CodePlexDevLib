using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;

namespace System.Couchbase
{
    [Serializable]
    [ComVisible(true)]
    public class OperationCanceledException : System.OperationCanceledException
    {
        private const int Result = unchecked((int)0x8013153b);
        private readonly CancellationToken? token;

        public OperationCanceledException()
            : base("The operation was canceled.")
        {
        }

        public OperationCanceledException(string message)
            : base(message)
        {
        }

        public OperationCanceledException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public OperationCanceledException(CancellationToken token)
            : this()
        {
            this.token = token;
        }

        public OperationCanceledException(string message, CancellationToken token)
            : this(message)
        {
            this.token = token;
        }

        public OperationCanceledException(string message, Exception innerException, CancellationToken token)
            : base(message, innerException)
        {
            this.token = token;
        }

        protected OperationCanceledException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public CancellationToken CancellationToken
        {
            get
            {
                if (token == null)
                    return CancellationToken.None;
                return token.Value;
            }
        }
    }
}
