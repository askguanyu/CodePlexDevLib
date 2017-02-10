//-----------------------------------------------------------------------
// <copyright file="BrokeredMessageExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceBus
{
    /// <summary>
    /// BrokeredMessage extension methods.
    /// </summary>
    public static class BrokeredMessageExtensions
    {
        /// <summary>
        /// Sets source object to the brokered message body.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="message">The brokered message.</param>
        /// <returns>The BrokeredMessage instance.</returns>
        public static BrokeredMessage ToBrokeredMessage(this object source, BrokeredMessage message = null)
        {
            if (message != null)
            {
                return message.SetBody(source);
            }

            return new BrokeredMessage(source);
        }

        /// <summary>
        /// Gets the object from the brokered message body.
        /// </summary>
        /// <param name="source">The source BrokeredMessage.</param>
        /// <returns>The object from the brokered message body.</returns>
        public static object ToObject(this BrokeredMessage source)
        {
            if (source == null)
            {
                return null;
            }

            return source.GetBody();
        }

        /// <summary>
        /// Gets the object from the brokered message body.
        /// </summary>
        /// <typeparam name="T">The type to which the message body will be deserialized.</typeparam>
        /// <param name="source">The source BrokeredMessage.</param>
        /// <returns>The object from the brokered message body.</returns>
        public static T ToObject<T>(this BrokeredMessage source)
        {
            if (source == null)
            {
                return default(T);
            }

            return source.GetBody<T>();
        }
    }
}
