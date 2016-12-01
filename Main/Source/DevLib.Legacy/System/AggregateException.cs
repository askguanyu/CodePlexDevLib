using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace System
{
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerDisplay("Count = {InnerExceptions.Count}")]
    public class AggregateException : Exception
    {
        private const string defaultMessage = "One or more errors occurred";
        private List<Exception> innerExceptions = new List<Exception>();

        public AggregateException()
            : base(defaultMessage)
        {
        }

        public AggregateException(string message)
            : base(message)
        {
        }

        public AggregateException(string message, Exception innerException)
            : base(message, innerException)
        {
            if (innerException == null)
                throw new ArgumentNullException("innerException");
            innerExceptions.Add(innerException);
        }

        public AggregateException(params Exception[] innerExceptions)
            : this(defaultMessage, innerExceptions)
        {
        }

        public AggregateException(string message, params Exception[] innerExceptions)
            : base(message, innerExceptions == null || innerExceptions.Length == 0 ? null : innerExceptions[0])
        {
            if (innerExceptions == null)
                throw new ArgumentNullException("innerExceptions");
            foreach (var exception in innerExceptions)
                if (exception == null)
                    throw new ArgumentException("One of the inner exception is null", "innerExceptions");

            this.innerExceptions.AddRange(innerExceptions);
        }

        public AggregateException(IEnumerable<Exception> innerExceptions)
            : this(defaultMessage, innerExceptions)
        {
        }

        public AggregateException(string message, IEnumerable<Exception> innerExceptions)
            : this(message, new List<Exception>(innerExceptions).ToArray())
        {
        }

        protected AggregateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ReadOnlyCollection<Exception> InnerExceptions
        {
            get
            {
                return innerExceptions.AsReadOnly();
            }
        }

        public AggregateException Flatten()
        {
            List<Exception> inner = new List<Exception>();

            foreach (Exception e in innerExceptions)
            {
                AggregateException aggEx = e as AggregateException;
                if (aggEx != null)
                {
                    inner.AddRange(aggEx.Flatten().InnerExceptions);
                }
                else
                {
                    inner.Add(e);
                }
            }

            return new AggregateException(inner);
        }

        public override Exception GetBaseException()
        {
            Exception inner = this;
            for (var ae = this; ae.innerExceptions.Count == 1; )
            {
                inner = ae.InnerExceptions[0];

                var aei = inner as AggregateException;
                if (aei == null)
                    break;

                ae = aei;
            }

            return inner;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("InnerExceptions", innerExceptions.ToArray(), typeof(Exception[]));
        }

        public void Handle(Func<Exception, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            List<Exception> failed = new List<Exception>();
            foreach (var e in innerExceptions)
            {
                if (!predicate(e))
                    failed.Add(e);
            }

            if (failed.Count > 0)
                throw new AggregateException(failed);
        }

        public override string ToString()
        {
            System.Text.StringBuilder finalMessage = new System.Text.StringBuilder(base.ToString());

            int currentIndex = -1;
            foreach (Exception e in innerExceptions)
            {
                finalMessage.Append(Environment.NewLine);
                finalMessage.Append(" --> (Inner exception ");
                finalMessage.Append(++currentIndex);
                finalMessage.Append(") ");
                finalMessage.Append(e.ToString());
                finalMessage.Append(Environment.NewLine);
            }
            return finalMessage.ToString();
        }

        internal void AddChildException(AggregateException childEx)
        {
            if (innerExceptions == null)
                innerExceptions = new List<Exception>();
            if (childEx == null)
                return;

            innerExceptions.Add(childEx);
        }
    }
}
