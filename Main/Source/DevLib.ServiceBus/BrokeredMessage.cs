//-----------------------------------------------------------------------
// <copyright file="BrokeredMessage.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;

    /// <summary>
    /// Represents the unit of communication between ServiceBus clients.
    /// </summary>
    public class BrokeredMessage
    {
        /// <summary>
        /// Field _bodyObject.
        /// </summary>
        private readonly byte[] _bodyObject;

        /// <summary>
        /// Field _properties.
        /// </summary>
        private Dictionary<string, object> _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrokeredMessage"/> class.
        /// </summary>
        /// <param name="serializableObject">The serializable object.</param>
        public BrokeredMessage(object serializableObject)
        {
            this.Id = Utilities.NewSequentialGuid();
            this.CreatedAt = DateTime.Now;
            this._bodyObject = Utilities.Serialize(serializableObject);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrokeredMessage"/> class.
        /// </summary>
        /// <param name="originalMessage">The original message.</param>
        private BrokeredMessage(BrokeredMessage originalMessage)
        {
            this.Id = originalMessage.Id;
            this.CreatedAt = originalMessage.CreatedAt;
            this.SentAt = originalMessage.SentAt;
            this.ArrivedAt = originalMessage.ArrivedAt;
            this.ReceivedAt = originalMessage.ReceivedAt;
            this.SourcePublisher = originalMessage.SourcePublisher;
            this.SourceTopic = originalMessage.SourceTopic;
            this.DestSubscription = originalMessage.DestSubscription;
            this.IsReturned = originalMessage.IsReturned;
            this._bodyObject = originalMessage._bodyObject;
            this._properties = originalMessage._properties;
        }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        public Dictionary<string, object> Properties
        {
            get
            {
                if (this._properties == null)
                {
                    Interlocked.CompareExchange<Dictionary<string, object>>(ref this._properties, new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase), null);
                }

                return this._properties;
            }
        }

        /// <summary>
        /// Gets the identifier of the message.
        /// </summary>
        public Guid Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the message created DateTime.
        /// </summary>
        public DateTime CreatedAt
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the message sent DateTime.
        /// </summary>
        public DateTime SentAt
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the message arrived at Topic DateTime.
        /// </summary>
        public DateTime ArrivedAt
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the message received DateTime.
        /// </summary>
        public DateTime ReceivedAt
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the source publisher.
        /// </summary>
        public string SourcePublisher
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the source topic.
        /// </summary>
        public string SourceTopic
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the destination subscription.
        /// </summary>
        public string DestSubscription
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is returned to its source publisher.
        /// </summary>
        public bool IsReturned
        {
            get;
            internal set;
        }

        /// <summary>
        /// Deserializes the brokered message body into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type to which the message body will be deserialized.</typeparam>
        /// <returns>The deserialized object or graph.</returns>
        public T GetBody<T>()
        {
            return Utilities.Deserialize<T>(this._bodyObject);
        }

        /// <summary>
        /// Deserializes the brokered message body into an object.
        /// </summary>
        /// <returns>The deserialized object or graph.</returns>
        public object GetBody()
        {
            return Utilities.Deserialize(this._bodyObject);
        }

        /// <summary>
        /// Sends back the specified message to its source publisher or source topic.
        /// </summary>
        public void Return()
        {
            var returnMessage = this.Clone();
            returnMessage.IsReturned = true;

            if (Utilities.IsNullOrWhiteSpace(returnMessage.SourcePublisher))
            {
                if (!Utilities.IsNullOrWhiteSpace(returnMessage.SourceTopic))
                {
                    Topic topic = ServiceBusManager.GetTopic(returnMessage.SourceTopic);

                    if (topic != null)
                    {
                        topic.Accept(returnMessage);
                    }
                }
            }
            else
            {
                PublisherClient publisher = ServiceBusManager.GetPublisher(returnMessage.SourcePublisher);

                if (publisher != null)
                {
                    publisher.Send(returnMessage);
                }
            }
        }

        /// <summary>
        /// Clones a message, so that it is possible to send a clone of a message as a new message.
        /// </summary>
        /// <returns>The BrokeredMessage that contains the cloned message.</returns>
        public BrokeredMessage Clone()
        {
            return new BrokeredMessage(this);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format(
                "BrokeredMessageId={0}, CreatedAt={1}, SentAt={2}, ArrivedAt={3}, ReceivedAt={4}, SourcePublisher={5}, SourceTopic={6}, DestSubscription={7}, IsReturned={8}, Body={9}",
                this.Id.ToString(),
                this.CreatedAt.ToString(Utilities.DateTimeFormat, CultureInfo.InvariantCulture),
                this.SentAt.ToString(Utilities.DateTimeFormat, CultureInfo.InvariantCulture),
                this.ArrivedAt.ToString(Utilities.DateTimeFormat, CultureInfo.InvariantCulture),
                this.ReceivedAt.ToString(Utilities.DateTimeFormat, CultureInfo.InvariantCulture),
                this.SourcePublisher ?? string.Empty,
                this.SourceTopic ?? string.Empty,
                this.DestSubscription ?? string.Empty,
                this.IsReturned,
                (this.GetBody() ?? string.Empty).ToString());
        }
    }
}
