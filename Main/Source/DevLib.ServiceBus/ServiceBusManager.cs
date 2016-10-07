//-----------------------------------------------------------------------
// <copyright file="ServiceBusManager.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceBus
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an anchor class used in managing entities, such as publishers, topics, subscriptions.
    /// </summary>
    public static class ServiceBusManager
    {
        /// <summary>
        /// Field Topics.
        /// </summary>
        private static readonly Dictionary<string, Topic> Topics = new Dictionary<string, Topic>();

        /// <summary>
        /// Field Publishers.
        /// </summary>
        private static readonly Dictionary<string, PublisherClient> Publishers = new Dictionary<string, PublisherClient>();

        /// <summary>
        /// Field Subscriptions.
        /// </summary>
        private static readonly Dictionary<string, SubscriptionClient> Subscriptions = new Dictionary<string, SubscriptionClient>();

        /// <summary>
        /// Gets or creates a topic.
        /// </summary>
        /// <param name="topicName">Name of the topic.</param>
        /// <returns>Topic instance.</returns>
        public static Topic GetOrCreateTopic(string topicName)
        {
            if (Utilities.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentNullException("topicName", "Topic name cannot be null or empty.");
            }

            lock (Utilities.GetSyncRoot(Topics))
            {
                if (Topics.ContainsKey(topicName))
                {
                    return Topics[topicName];
                }
                else
                {
                    Topic result = new Topic(topicName);

                    Topics.Add(topicName, result);

                    return result;
                }
            }
        }

        /// <summary>
        /// Gets a topic.
        /// </summary>
        /// <param name="topicName">Name of the topic.</param>
        /// <returns>Topic instance.</returns>
        public static Topic GetTopic(string topicName)
        {
            if (Utilities.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentNullException("topicName", "Topic name cannot be null or empty.");
            }

            lock (Utilities.GetSyncRoot(Topics))
            {
                if (Topics.ContainsKey(topicName))
                {
                    return Topics[topicName];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets or creates a publisher.
        /// </summary>
        /// <param name="publisherName">Name of the publisher.</param>
        /// <returns>PublisherClient instance.</returns>
        public static PublisherClient GetOrCreatePublisher(string publisherName)
        {
            if (Utilities.IsNullOrWhiteSpace(publisherName))
            {
                throw new ArgumentNullException("publisherName", "Publisher name cannot be null or empty.");
            }

            lock (Utilities.GetSyncRoot(Publishers))
            {
                if (Publishers.ContainsKey(publisherName))
                {
                    return Publishers[publisherName];
                }
                else
                {
                    PublisherClient result = new PublisherClient(publisherName);

                    Publishers.Add(publisherName, result);

                    return result;
                }
            }
        }

        /// <summary>
        /// Gets a publisher.
        /// </summary>
        /// <param name="publisherName">Name of the publisher.</param>
        /// <returns>PublisherClient instance.</returns>
        public static PublisherClient GetPublisher(string publisherName)
        {
            if (Utilities.IsNullOrWhiteSpace(publisherName))
            {
                throw new ArgumentNullException("publisherName", "Publisher name cannot be null or empty.");
            }

            lock (Utilities.GetSyncRoot(Publishers))
            {
                if (Publishers.ContainsKey(publisherName))
                {
                    return Publishers[publisherName];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets or creates a publisher associated to a topic.
        /// </summary>
        /// <param name="publisherName">Name of the publisher.</param>
        /// <param name="topicName">Name of the topic.</param>
        /// <returns>PublisherClient instance.</returns>
        public static PublisherClient GetOrCreatePublisher(string publisherName, string topicName)
        {
            return LinkPublisherTopic(publisherName, topicName);
        }

        /// <summary>
        /// Gets or creates a subscription.
        /// </summary>
        /// <param name="subscriptionName">Name of the subscription.</param>
        /// <returns>SubscriptionClient instance.</returns>
        public static SubscriptionClient GetOrCreateSubscription(string subscriptionName)
        {
            if (Utilities.IsNullOrWhiteSpace(subscriptionName))
            {
                throw new ArgumentNullException("subscriptionName", "Subscription name cannot be null or empty.");
            }

            lock (Utilities.GetSyncRoot(Subscriptions))
            {
                if (Subscriptions.ContainsKey(subscriptionName))
                {
                    return Subscriptions[subscriptionName];
                }
                else
                {
                    SubscriptionClient result = new SubscriptionClient(subscriptionName);

                    Subscriptions.Add(subscriptionName, result);

                    return result;
                }
            }
        }

        /// <summary>
        /// Gets a subscription.
        /// </summary>
        /// <param name="subscriptionName">Name of the subscription.</param>
        /// <returns>SubscriptionClient instance.</returns>
        public static SubscriptionClient GetSubscription(string subscriptionName)
        {
            if (Utilities.IsNullOrWhiteSpace(subscriptionName))
            {
                throw new ArgumentNullException("subscriptionName", "Subscription name cannot be null or empty.");
            }

            lock (Utilities.GetSyncRoot(Subscriptions))
            {
                if (Subscriptions.ContainsKey(subscriptionName))
                {
                    return Subscriptions[subscriptionName];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets or creates a subscription associated to a topic.
        /// </summary>
        /// <param name="subscriptionName">Name of the subscription.</param>
        /// <param name="topicName">Name of the topic.</param>
        /// <returns>SubscriptionClient instance.</returns>
        public static SubscriptionClient GetOrCreateSubscription(string subscriptionName, string topicName)
        {
            return LinkSubscriptionTopic(subscriptionName, topicName);
        }

        /// <summary>
        /// Determines whether a topic exists in the service bus.
        /// </summary>
        /// <param name="topicName">Name of the topic.</param>
        /// <returns>true if a topic exists in the service namespace; otherwise, false.</returns>
        public static bool TopicExists(string topicName)
        {
            if (Utilities.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentNullException("topicName", "Topic name cannot be null or empty.");
            }

            lock (Utilities.GetSyncRoot(Topics))
            {
                return Topics.ContainsKey(topicName);
            }
        }

        /// <summary>
        /// Determines whether a publisher exists in the service bus.
        /// </summary>
        /// <param name="publisherName">Name of the publisher.</param>
        /// <returns>true if a publisher exists in the service namespace; otherwise, false.</returns>
        public static bool PublisherExists(string publisherName)
        {
            if (Utilities.IsNullOrWhiteSpace(publisherName))
            {
                throw new ArgumentNullException("publisherName", "Publisher name cannot be null or empty.");
            }

            lock (Utilities.GetSyncRoot(Publishers))
            {
                return Publishers.ContainsKey(publisherName);
            }
        }

        /// <summary>
        /// Determines whether a subscription exists in the service bus.
        /// </summary>
        /// <param name="subscriptionName">Name of the subscription.</param>
        /// <returns>true if a subscription exists in the service namespace; otherwise, false.</returns>
        public static bool SubscriptionExists(string subscriptionName)
        {
            if (Utilities.IsNullOrWhiteSpace(subscriptionName))
            {
                throw new ArgumentNullException("subscriptionName", "Subscription name cannot be null or empty.");
            }

            lock (Utilities.GetSyncRoot(Subscriptions))
            {
                return Subscriptions.ContainsKey(subscriptionName);
            }
        }

        /// <summary>
        /// Deletes the publisher.
        /// </summary>
        /// <param name="publisherName">Name of the publisher.</param>
        public static void DeletePublisher(string publisherName)
        {
            if (Utilities.IsNullOrWhiteSpace(publisherName))
            {
                throw new ArgumentNullException("publisherName", "Publisher name cannot be null or empty.");
            }

            lock (Utilities.GetSyncRoot(Publishers))
            {
                if (Publishers.ContainsKey(publisherName))
                {
                    PublisherClient publisher = Publishers[publisherName];

                    foreach (var topicName in publisher.Topics)
                    {
                        publisher.RemoveTopicInternal(topicName);

                        Topic topic = GetTopic(topicName);

                        if (topic != null)
                        {
                            topic.RemovePublisherInternal(publisherName);
                        }
                    }

                    Publishers.Remove(publisherName);
                }
            }
        }

        /// <summary>
        /// Deletes the topic.
        /// </summary>
        /// <param name="topicName">Name of the topic.</param>
        public static void DeleteTopic(string topicName)
        {
            if (Utilities.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentNullException("topicName", "Topic name cannot be null or empty.");
            }

            lock (Utilities.GetSyncRoot(Topics))
            {
                if (Topics.ContainsKey(topicName))
                {
                    Topic topic = Topics[topicName];

                    foreach (var publisherName in topic.Publishers)
                    {
                        topic.RemovePublisherInternal(publisherName);

                        PublisherClient publisher = GetPublisher(publisherName);

                        if (publisher != null)
                        {
                            publisher.RemoveTopicInternal(topicName);
                        }
                    }

                    foreach (var subscriptionName in topic.Subscriptions)
                    {
                        topic.RemovePublisherInternal(subscriptionName);

                        SubscriptionClient subscription = GetSubscription(subscriptionName);

                        if (subscription != null)
                        {
                            subscription.RemoveTopicInternal(topicName);
                        }
                    }

                    Topics.Remove(topicName);
                }
            }
        }

        /// <summary>
        /// Deletes the subscription.
        /// </summary>
        /// <param name="subscriptionName">Name of the subscription.</param>
        public static void DeleteSubscription(string subscriptionName)
        {
            if (Utilities.IsNullOrWhiteSpace(subscriptionName))
            {
                throw new ArgumentNullException("subscriptionName", "Subscription name cannot be null or empty.");
            }

            lock (Utilities.GetSyncRoot(Subscriptions))
            {
                if (Subscriptions.ContainsKey(subscriptionName))
                {
                    SubscriptionClient subscription = Subscriptions[subscriptionName];

                    foreach (var topicName in subscription.Topics)
                    {
                        subscription.RemoveTopicInternal(topicName);

                        Topic topic = GetTopic(topicName);

                        if (topic != null)
                        {
                            topic.RemovePublisherInternal(subscriptionName);
                        }
                    }

                    Subscriptions.Remove(subscriptionName);
                }
            }
        }

        /// <summary>
        /// Links the publisher to a topic.
        /// </summary>
        /// <param name="publisherName">Name of the publisher.</param>
        /// <param name="topicName">Name of the topic.</param>
        /// <returns>PublisherClient instance.</returns>
        internal static PublisherClient LinkPublisherTopic(string publisherName, string topicName)
        {
            if (Utilities.IsNullOrWhiteSpace(publisherName))
            {
                throw new ArgumentNullException("publisherName", "Publisher name cannot be null or empty.");
            }

            if (Utilities.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentNullException("topicName", "Topic name cannot be null or empty.");
            }

            PublisherClient publisher = GetOrCreatePublisher(publisherName);
            Topic topic = GetOrCreateTopic(topicName);

            publisher.AddTopicInternal(topic);
            topic.AddPublisherInternal(publisher);

            return publisher;
        }

        /// <summary>
        /// Removes the link between a publisher and a topic.
        /// </summary>
        /// <param name="publisherName">Name of the publisher.</param>
        /// <param name="topicName">Name of the topic.</param>
        internal static void RemoveLinkPublisherTopic(string publisherName, string topicName)
        {
            if (Utilities.IsNullOrWhiteSpace(publisherName))
            {
                throw new ArgumentNullException("publisherName", "Publisher name cannot be null or empty.");
            }

            if (Utilities.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentNullException("topicName", "Topic name cannot be null or empty.");
            }

            PublisherClient publisher = GetPublisher(publisherName);

            if (publisher != null)
            {
                publisher.RemoveTopicInternal(topicName);
            }

            Topic topic = GetTopic(topicName);

            if (topic != null)
            {
                topic.RemovePublisherInternal(publisherName);
            }
        }

        /// <summary>
        /// Links the subscription to a topic.
        /// </summary>
        /// <param name="subscriptionName">Name of the subscription.</param>
        /// <param name="topicName">Name of the topic.</param>
        /// <returns>SubscriptionClient instance.</returns>
        internal static SubscriptionClient LinkSubscriptionTopic(string subscriptionName, string topicName)
        {
            if (Utilities.IsNullOrWhiteSpace(subscriptionName))
            {
                throw new ArgumentNullException("subscriptionName", "Subscription name cannot be null or empty.");
            }

            if (Utilities.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentNullException("topicName", "Topic name cannot be null or empty.");
            }

            SubscriptionClient subscription = GetOrCreateSubscription(subscriptionName);
            Topic topic = GetOrCreateTopic(topicName);

            subscription.AddTopicInternal(topic);
            topic.AddSubscriptionInternal(subscription);

            return subscription;
        }

        /// <summary>
        /// Removes the link between a subscription and a topic.
        /// </summary>
        /// <param name="subscriptionName">Name of the subscription.</param>
        /// <param name="topicName">Name of the topic.</param>
        internal static void RemoveLinkSubscriptionTopic(string subscriptionName, string topicName)
        {
            if (Utilities.IsNullOrWhiteSpace(subscriptionName))
            {
                throw new ArgumentNullException("subscriptionName", "Subscription name cannot be null or empty.");
            }

            if (Utilities.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentNullException("topicName", "Topic name cannot be null or empty.");
            }

            SubscriptionClient subscription = GetSubscription(subscriptionName);

            if (subscription != null)
            {
                subscription.RemoveTopicInternal(topicName);
            }

            Topic topic = GetTopic(topicName);

            if (topic != null)
            {
                topic.RemoveSubscriptionInternal(subscriptionName);
            }
        }
    }
}
