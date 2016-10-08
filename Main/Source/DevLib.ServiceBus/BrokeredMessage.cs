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
    [Serializable]
    public class BrokeredMessage
    {
        /// <summary>
        /// Field _bodyObject.
        /// </summary>
        private byte[] _bodyObject;

        /// <summary>
        /// Field _properties.
        /// </summary>
        private Dictionary<string, object> _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrokeredMessage" /> class.
        /// </summary>
        public BrokeredMessage()
        {
            this.Id = Utilities.NewSequentialGuid();
            this.CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrokeredMessage"/> class.
        /// </summary>
        /// <param name="serializableObject">The serializable object.</param>
        public BrokeredMessage(object serializableObject)
        {
            this.Id = Utilities.NewSequentialGuid();
            this.CreatedAt = DateTime.Now;
            this._bodyObject = Utilities.SerializeXmlBinary(serializableObject);
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
            this.FirstPublisher = originalMessage.FirstPublisher;
            this.FirstTopic = originalMessage.FirstTopic;
            this.LastPublisher = originalMessage.LastPublisher;
            this.LastTopic = originalMessage.LastTopic;
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
        /// Gets the first access publisher.
        /// </summary>
        public string FirstPublisher
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the first access topic.
        /// </summary>
        public string FirstTopic
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the last access publisher.
        /// </summary>
        public string LastPublisher
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the last access topic.
        /// </summary>
        public string LastTopic
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
            return Utilities.DeserializeXmlBinary<T>(this._bodyObject);
        }

        /// <summary>
        /// Deserializes the brokered message body into a Xml string.
        /// </summary>
        /// <returns>The deserialized object or graph Xml string.</returns>
        public string GetBody()
        {
            return Utilities.DeserializeXmlBinaryString(this._bodyObject);
        }

        /// <summary>
        /// Serializes the object into brokered message body.
        /// </summary>
        /// <param name="serializableObject">The serializable object.</param>
        /// <returns>Current BrokeredMessage.</returns>
        public BrokeredMessage SetBody(object serializableObject)
        {
            this._bodyObject = Utilities.SerializeXmlBinary(serializableObject);

            return this;
        }

        /// <summary>
        /// Sends back the specified message to its source publisher or source topic.
        /// </summary>
        /// <param name="returnToTopic">true to return message to its source topic; otherwise, to its source publisher if possible.</param>
        public void Return(bool returnToTopic = false)
        {
            var returnMessage = this.Clone();
            returnMessage.IsReturned = true;

            if (returnToTopic || Utilities.IsNullOrWhiteSpace(returnMessage.LastPublisher))
            {
                if (!Utilities.IsNullOrWhiteSpace(returnMessage.LastTopic))
                {
                    Topic topic = ServiceBusManager.GetTopic(returnMessage.LastTopic);

                    if (topic != null)
                    {
                        topic.Accept(returnMessage);
                    }
                }
            }
            else
            {
                PublisherClient publisher = ServiceBusManager.GetPublisher(returnMessage.LastPublisher);

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
                "BrokeredMessageId={0}, CreatedAt={1}, SentAt={2}, ArrivedAt={3}, ReceivedAt={4}, FirstPublisher={5}, FirstTopic={6}, LastPublisher={7}, LastTopic={8}, DestSubscription={9}, IsReturned={10}, Body={11}",
                this.Id.ToString(),
                this.CreatedAt.ToString(Utilities.DateTimeFormat, CultureInfo.InvariantCulture),
                this.SentAt.ToString(Utilities.DateTimeFormat, CultureInfo.InvariantCulture),
                this.ArrivedAt.ToString(Utilities.DateTimeFormat, CultureInfo.InvariantCulture),
                this.ReceivedAt.ToString(Utilities.DateTimeFormat, CultureInfo.InvariantCulture),
                this.FirstPublisher ?? string.Empty,
                this.FirstTopic ?? string.Empty,
                this.LastPublisher ?? string.Empty,
                this.LastTopic ?? string.Empty,
                this.DestSubscription ?? string.Empty,
                this.IsReturned,
                this.GetBody() ?? string.Empty);
        }
    }
}
