//-----------------------------------------------------------------------
// <copyright file="PublisherClient.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceBus
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the anchor class used in run-time operations related to a topic publisher.
    /// </summary>
    public class PublisherClient : IDisposable
    {
        /// <summary>
        /// Field _topics.
        /// </summary>
        private readonly Dictionary<string, Topic> _topics = new Dictionary<string, Topic>();

        /// <summary>
        /// Field _producerConsumer.
        /// </summary>
        private readonly ProducerConsumer<BrokeredMessage> _producerConsumer;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublisherClient"/> class.
        /// </summary>
        /// <param name="publisherName">Name of the publisher.</param>
        internal PublisherClient(string publisherName)
        {
            this.Name = publisherName;
            this.CreatedAt = DateTime.Now;
            this.AccessedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
            this._producerConsumer = new ProducerConsumer<BrokeredMessage>(message => this.DispatchMessage(message));
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="PublisherClient" /> class.
        /// </summary>
        ~PublisherClient()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the name of current PublisherClient.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the PublisherClient created DateTime.
        /// </summary>
        public DateTime CreatedAt
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the PublisherClient last accessed DateTime.
        /// </summary>
        public DateTime AccessedAt
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the PublisherClient last updated DateTime.
        /// </summary>
        public DateTime UpdatedAt
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the names of the topics.
        /// </summary>
        public List<string> Topics
        {
            get
            {
                List<string> result = new List<string>();

                lock (Utilities.GetSyncRoot(this._topics))
                {
                    foreach (var item in this._topics.Keys)
                    {
                        result.Add(item);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="PublisherClient" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="PublisherClient" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Adds the topic to current PublisherClient publish list.
        /// </summary>
        /// <param name="topicName">Name of the topic.</param>
        /// <returns>Current PublisherClient.</returns>
        public PublisherClient AddTopic(string topicName)
        {
            this.CheckDisposed();

            ServiceBusManager.LinkPublisherTopic(this.Name, topicName);

            return this;
        }

        /// <summary>
        /// Removes the topic from current PublisherClient publish list.
        /// </summary>
        /// <param name="topicName">Name of the topic.</param>
        /// <returns>Current PublisherClient.</returns>
        public PublisherClient RemoveTopic(string topicName = null)
        {
            this.CheckDisposed();

            ServiceBusManager.RemoveLinkPublisherTopic(this.Name, topicName);

            return this;
        }

        /// <summary>
        /// Removes all topics from current PublisherClient publish list.
        /// </summary>
        /// <returns>Current PublisherClient.</returns>
        public PublisherClient RemoveAllTopics()
        {
            this.CheckDisposed();

            Dictionary<string, Topic>.KeyCollection topics = null;

            lock (Utilities.GetSyncRoot(this._topics))
            {
                topics = this._topics.Keys;
            }

            if (topics != null)
            {
                foreach (var topic in topics)
                {
                    ServiceBusManager.RemoveLinkPublisherTopic(this.Name, topic);
                }
            }

            return this;
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The BrokeredMessage.</param>
        /// <returns>Current PublisherClient.</returns>
        public PublisherClient Send(BrokeredMessage message)
        {
            this.CheckDisposed();

            message.SentAt = DateTime.Now;
            message.SourcePublisher = this.Name;

            this._producerConsumer.Enqueue(message);

            this.AccessedAt = DateTime.Now;

            return this;
        }

        /// <summary>
        /// Sends the specified messages.
        /// </summary>
        /// <param name="messages">The BrokeredMessages.</param>
        /// <returns>Current PublisherClient.</returns>
        public PublisherClient Send(IEnumerable<BrokeredMessage> messages)
        {
            this.CheckDisposed();

            DateTime sentAt = DateTime.Now;
            foreach (var message in messages)
            {
                message.SentAt = sentAt;
                message.SourcePublisher = this.Name;
            }

            this._producerConsumer.Enqueue(messages);

            this.AccessedAt = DateTime.Now;

            return this;
        }

        /// <summary>
        /// Adds the topic internal.
        /// </summary>
        /// <param name="topic">The topic.</param>
        internal void AddTopicInternal(Topic topic)
        {
            this.CheckDisposed();

            lock (Utilities.GetSyncRoot(this._topics))
            {
                if (!this._topics.ContainsKey(topic.Name))
                {
                    this._topics[topic.Name] = topic;
                    this.UpdatedAt = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Removes the topic internal.
        /// </summary>
        /// <param name="topicName">Name of the topic.</param>
        internal void RemoveTopicInternal(string topicName)
        {
            this.CheckDisposed();

            lock (Utilities.GetSyncRoot(this._topics))
            {
                if (this._topics.ContainsKey(topicName))
                {
                    this._topics.Remove(topicName);
                    this.UpdatedAt = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="PublisherClient" /> class.
        /// protected virtual for non-sealed class; private for sealed class.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;

            if (disposing)
            {
                // dispose managed resources
                ////if (managedResource != null)
                ////{
                ////    managedResource.Dispose();
                ////    managedResource = null;
                ////}

                if (this._producerConsumer != null)
                {
                    this._producerConsumer.Dispose();
                }
            }

            // free native resources
            ////if (nativeResource != IntPtr.Zero)
            ////{
            ////    Marshal.FreeHGlobal(nativeResource);
            ////    nativeResource = IntPtr.Zero;
            ////}
        }

        /// <summary>
        /// Checks whether this instance is disposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.ServiceBus.PublisherClient");
            }
        }

        /// <summary>
        /// Dispatches the message to topics.
        /// </summary>
        /// <param name="message">The message.</param>
        private void DispatchMessage(BrokeredMessage message)
        {
            this.CheckDisposed();

            foreach (var item in this._topics.Values)
            {
                item.Accept(message.Clone());
            }
        }
    }
}
