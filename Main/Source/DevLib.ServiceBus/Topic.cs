//-----------------------------------------------------------------------
// <copyright file="Topic.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceBus
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a topic.
    /// </summary>
    public class Topic : IDisposable
    {
        /// <summary>
        /// Field _publishers.
        /// </summary>
        private readonly Dictionary<string, PublisherClient> _publishers = new Dictionary<string, PublisherClient>();

        /// <summary>
        /// Field _subscriptions.
        /// </summary>
        private readonly Dictionary<string, SubscriptionClient> _subscriptions = new Dictionary<string, SubscriptionClient>();

        /// <summary>
        /// Field _producerConsumer.
        /// </summary>
        private readonly ProducerConsumer<BrokeredMessage> _producerConsumer;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Topic"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        internal Topic(string name)
        {
            this.Name = name;
            this.CreatedAt = DateTime.Now;
            this.AccessedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
            this._producerConsumer = new ProducerConsumer<BrokeredMessage>(message => this.DispatchMessage(message));
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Topic" /> class.
        /// </summary>
        ~Topic()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the Topic name.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Topic created DateTime.
        /// </summary>
        public DateTime CreatedAt
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Topic last accessed DateTime.
        /// </summary>
        public DateTime AccessedAt
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Topic last updated DateTime.
        /// </summary>
        public DateTime UpdatedAt
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the names of the publishers.
        /// </summary>
        /// <value>The publishers.</value>
        public List<string> Publishers
        {
            get
            {
                List<string> result = new List<string>();

                lock (Utilities.GetSyncRoot(this._publishers))
                {
                    foreach (var item in this._publishers.Keys)
                    {
                        result.Add(item);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the names of the subscriptions.
        /// </summary>
        /// <value>The subscriptions.</value>
        public List<string> Subscriptions
        {
            get
            {
                List<string> result = new List<string>();

                lock (Utilities.GetSyncRoot(this._subscriptions))
                {
                    foreach (var item in this._subscriptions.Keys)
                    {
                        result.Add(item);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="Topic" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="Topic" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Adds the publisher.
        /// </summary>
        /// <param name="publisherName">Name of the publisher.</param>
        /// <returns>Current Topic.</returns>
        public Topic AddPublisher(string publisherName)
        {
            this.CheckDisposed();

            ServiceBusManager.LinkPublisherTopic(publisherName, this.Name);

            return this;
        }

        /// <summary>
        /// Removes the publisher.
        /// </summary>
        /// <param name="publisherName">Name of the publisher.</param>
        /// <returns>Current Topic.</returns>
        public Topic RemovePublisher(string publisherName)
        {
            this.CheckDisposed();

            ServiceBusManager.RemoveLinkPublisherTopic(publisherName, this.Name);

            return this;
        }

        /// <summary>
        /// Adds the subscription.
        /// </summary>
        /// <param name="subscriptionName">Name of the subscription.</param>
        /// <returns>Current Topic.</returns>
        public Topic AddSubscription(string subscriptionName)
        {
            this.CheckDisposed();

            ServiceBusManager.LinkSubscriptionTopic(subscriptionName, this.Name);

            return this;
        }

        /// <summary>
        /// Removes the subscription.
        /// </summary>
        /// <param name="subscriptionName">Name of the subscription.</param>
        /// <returns>Current Topic.</returns>
        public Topic RemoveSubscription(string subscriptionName)
        {
            this.CheckDisposed();

            ServiceBusManager.RemoveLinkSubscriptionTopic(subscriptionName, this.Name);

            return this;
        }

        /// <summary>
        /// Removes all publishers.
        /// </summary>
        /// <returns>Current Topic.</returns>
        public Topic RemoveAllPublishers()
        {
            this.CheckDisposed();

            Dictionary<string, PublisherClient>.KeyCollection publishers = null;

            lock (Utilities.GetSyncRoot(this._publishers))
            {
                publishers = this._publishers.Keys;
            }

            if (publishers != null)
            {
                foreach (var publisher in publishers)
                {
                    ServiceBusManager.RemoveLinkPublisherTopic(publisher, this.Name);
                }
            }

            return this;
        }

        /// <summary>
        /// Removes all subscriptions.
        /// </summary>
        /// <returns>Current Topic.</returns>
        public Topic RemoveAllSubscriptions()
        {
            this.CheckDisposed();

            Dictionary<string, SubscriptionClient>.KeyCollection subscriptions = null;

            lock (Utilities.GetSyncRoot(this._subscriptions))
            {
                subscriptions = this._subscriptions.Keys;
            }

            if (subscriptions != null)
            {
                foreach (var subscription in subscriptions)
                {
                    ServiceBusManager.RemoveLinkSubscriptionTopic(subscription, this.Name);
                }
            }

            return this;
        }

        /// <summary>
        /// Starts processing message.
        /// </summary>
        /// <returns>Current Topic.</returns>
        public Topic Start()
        {
            this.CheckDisposed();

            this._producerConsumer.Start();

            this.AccessedAt = DateTime.Now;

            return this;
        }

        /// <summary>
        /// Stops processing message.
        /// </summary>
        /// <param name="continueReceive">When stopped whether continue receiving message or not.</param>
        /// <param name="clearBacklog">When stopped whether to clear backlog message or not.</param>
        /// <returns>Current Topic.</returns>
        public Topic Stop(bool continueReceive = true, bool clearBacklog = false)
        {
            this.CheckDisposed();

            this._producerConsumer.Stop(continueReceive, clearBacklog);

            this.AccessedAt = DateTime.Now;

            return this;
        }

        /// <summary>
        /// Accepts the specified message to current topic.
        /// </summary>
        /// <param name="message">The BrokeredMessages.</param>
        /// <returns>Current Topic.</returns>
        public Topic Accept(BrokeredMessage message)
        {
            this.CheckDisposed();

            BrokeredMessage messageClone = message.Clone();

            messageClone.SentAt = DateTime.Now;
            messageClone.ArrivedAt = DateTime.Now;
            messageClone.LastTopic = this.Name;

            if (Utilities.IsNullOrWhiteSpace(messageClone.FirstTopic))
            {
                messageClone.FirstTopic = this.Name;
            }

            this._producerConsumer.Enqueue(messageClone);

            this.AccessedAt = DateTime.Now;

            return this;
        }

        /// <summary>
        /// Accepts the specified messages to current topic.
        /// </summary>
        /// <param name="messages">The BrokeredMessages.</param>
        /// <returns>Current Topic.</returns>
        public Topic Accept(IEnumerable<BrokeredMessage> messages)
        {
            this.CheckDisposed();

            DateTime sentAt = DateTime.Now;

            foreach (var message in messages)
            {
                BrokeredMessage messageClone = message.Clone();

                messageClone.SentAt = sentAt;
                messageClone.ArrivedAt = sentAt;
                messageClone.LastTopic = this.Name;

                if (Utilities.IsNullOrWhiteSpace(messageClone.FirstTopic))
                {
                    messageClone.FirstTopic = this.Name;
                }

                this._producerConsumer.Enqueue(messageClone);
            }

            this.AccessedAt = DateTime.Now;

            return this;
        }

        /// <summary>
        /// Adds the publisher internal.
        /// </summary>
        /// <param name="publisher">The publisher.</param>
        internal void AddPublisherInternal(PublisherClient publisher)
        {
            this.CheckDisposed();

            lock (Utilities.GetSyncRoot(this._publishers))
            {
                if (!this._publishers.ContainsKey(publisher.Name))
                {
                    this._publishers[publisher.Name] = publisher;
                    this.UpdatedAt = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Removes the publisher internal.
        /// </summary>
        /// <param name="publisherName">Name of the publisher.</param>
        internal void RemovePublisherInternal(string publisherName)
        {
            this.CheckDisposed();

            lock (Utilities.GetSyncRoot(this._publishers))
            {
                if (this._publishers.ContainsKey(publisherName))
                {
                    this._publishers.Remove(publisherName);
                    this.UpdatedAt = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Adds the subscription internal.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        internal void AddSubscriptionInternal(SubscriptionClient subscription)
        {
            this.CheckDisposed();

            lock (Utilities.GetSyncRoot(this._publishers))
            {
                if (!this._subscriptions.ContainsKey(subscription.Name))
                {
                    this._subscriptions[subscription.Name] = subscription;
                    this.UpdatedAt = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Removes the subscription internal.
        /// </summary>
        /// <param name="subscriptionName">Name of the subscription.</param>
        internal void RemoveSubscriptionInternal(string subscriptionName)
        {
            this.CheckDisposed();

            lock (Utilities.GetSyncRoot(this._publishers))
            {
                if (this._subscriptions.ContainsKey(subscriptionName))
                {
                    this._subscriptions.Remove(subscriptionName);
                    this.UpdatedAt = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="Topic" /> class.
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
        /// Dispatches the message to subscriptions.
        /// </summary>
        /// <param name="message">The message.</param>
        private void DispatchMessage(BrokeredMessage message)
        {
            this.CheckDisposed();

            foreach (var item in this._subscriptions.Values)
            {
                item.Receive(message);
            }
        }

        /// <summary>
        /// Checks whether this instance is disposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.ServiceBus.Topic");
            }
        }
    }
}
