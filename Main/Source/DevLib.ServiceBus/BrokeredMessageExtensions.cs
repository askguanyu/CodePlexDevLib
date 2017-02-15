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
        public static BrokeredMessage ToBrokeredMessageBody(this object source, BrokeredMessage message = null)
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
        public static object ToBodyObject(this BrokeredMessage source)
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
        public static T ToBodyObject<T>(this BrokeredMessage source)
        {
            if (source == null)
            {
                return default(T);
            }

            return source.GetBody<T>();
        }

        /// <summary>
        /// Sets source object to the brokered message value.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="message">The brokered message.</param>
        /// <returns>The BrokeredMessage instance.</returns>
        public static BrokeredMessage ToBrokeredMessageValue(this object source, BrokeredMessage message = null)
        {
            if (message != null)
            {
                return message.SetValue(source);
            }

            return new BrokeredMessage { Value = source };
        }

        /// <summary>
        /// Gets the object from the brokered message value.
        /// </summary>
        /// <param name="source">The source BrokeredMessage.</param>
        /// <returns>The object from the brokered message value.</returns>
        public static object ToValueObject(this BrokeredMessage source)
        {
            if (source == null)
            {
                return null;
            }

            return source.Value;
        }

        /// <summary>
        /// Gets the object from the brokered message value.
        /// </summary>
        /// <typeparam name="T">The type of the message value.</typeparam>
        /// <param name="source">The source BrokeredMessage.</param>
        /// <returns>The object from the brokered message value.</returns>
        public static T ToValueObject<T>(this BrokeredMessage source)
        {
            if (source == null)
            {
                return default(T);
            }

            return source.GetValue<T>();
        }
    }
}
