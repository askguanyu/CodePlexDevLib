//-----------------------------------------------------------------------
// <copyright file="SubscriptionClient.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceBus
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the anchor class used in run-time operations related to a topic subscription.
    /// </summary>
    public class SubscriptionClient : IDisposable
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
        /// Field _consumerAction.
        /// </summary>
        private Action<BrokeredMessage> _consumerAction = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionClient"/> class.
        /// </summary>
        /// <param name="subscriptionName">Name of the subscription.</param>
        public SubscriptionClient(string subscriptionName)
        {
            this.Name = subscriptionName;
            this.CreatedAt = DateTime.Now;
            this.AccessedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
            this._producerConsumer = new ProducerConsumer<BrokeredMessage>(message => this.ConsumeMessage(message));
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SubscriptionClient" /> class.
        /// </summary>
        ~SubscriptionClient()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the name of current SubscriptionClient.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the SubscriptionClient created DateTime.
        /// </summary>
        /// <value>The created at.</value>
        public DateTime CreatedAt
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the SubscriptionClient last accessed DateTime.
        /// </summary>
        /// <value>The accessed at.</value>
        public DateTime AccessedAt
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the SubscriptionClient last updated DateTime.
        /// </summary>
        /// <value>The updated at.</value>
        public DateTime UpdatedAt
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the names of the topics.
        /// </summary>
        /// <value>The topics.</value>
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
        /// Gets a value indicating whether the current subscription is processing message.
        /// </summary>
        /// <value>true if the current subscription is processing message; otherwise, false.</value>
        public bool IsRunning
        {
            get
            {
                return this._producerConsumer.IsRunning;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="SubscriptionClient" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="SubscriptionClient" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called when a message received.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <returns>Current SubscriptionClient.</returns>
        public SubscriptionClient OnMessage(Action<BrokeredMessage> callback)
        {
            this.CheckDisposed();

            this._consumerAction = callback;

            return this;
        }

        /// <summary>
        /// Adds the topic.
        /// </summary>
        /// <param name="topicName">Name of the topic.</param>
        /// <returns>Current SubscriptionClient.</returns>
        public SubscriptionClient AddTopic(string topicName)
        {
            this.CheckDisposed();

            ServiceBusManager.LinkSubscriptionTopic(this.Name, topicName);

            return this;
        }

        /// <summary>
        /// Removes the topic.
        /// </summary>
        /// <param name="topicName">Name of the topic.</param>
        /// <returns>Current SubscriptionClient.</returns>
        public SubscriptionClient RemoveTopic(string topicName)
        {
            this.CheckDisposed();

            ServiceBusManager.RemoveLinkSubscriptionTopic(this.Name, topicName);

            return this;
        }

        /// <summary>
        /// Removes all topics.
        /// </summary>
        /// <returns>Current SubscriptionClient.</returns>
        public SubscriptionClient RemoveAllTopics()
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
                    ServiceBusManager.RemoveLinkSubscriptionTopic(this.Name, topic);
                }
            }

            return this;
        }

        /// <summary>
        /// Starts processing message.
        /// </summary>
        /// <returns>Current SubscriptionClient.</returns>
        public SubscriptionClient Start()
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
        /// <returns>Current SubscriptionClient.</returns>
        public SubscriptionClient Stop(bool continueReceive = true, bool clearBacklog = false)
        {
            this.CheckDisposed();

            this._producerConsumer.Stop(continueReceive, clearBacklog);

            this.AccessedAt = DateTime.Now;

            return this;
        }

        /// <summary>
        /// Receives the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void Receive(BrokeredMessage message)
        {
            this.CheckDisposed();

            message.ReceivedAt = DateTime.Now;
            message.DestSubscription = this.Name;

            this._producerConsumer.Enqueue(message);
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
        /// Releases all resources used by the current instance of the <see cref="SubscriptionClient" /> class.
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
                throw new ObjectDisposedException("DevLib.ServiceBus.SubscriptionClient");
            }
        }

        /// <summary>
        /// Consumes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void ConsumeMessage(BrokeredMessage message)
        {
            this.CheckDisposed();

            if (this._consumerAction != null)
            {
                this._consumerAction(message);
            }

            this.AccessedAt = DateTime.Now;
        }
    }
}
