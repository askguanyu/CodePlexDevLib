//-----------------------------------------------------------------------
// <copyright file="PipeMessageExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    /// <summary>
    /// PipeMessage extension methods.
    /// </summary>
    public static class PipeMessageExtensions
    {
        /// <summary>
        /// Sets source object to the pipe message body.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="message">The pipe message.</param>
        /// <returns>The PipeMessage instance.</returns>
        public static PipeMessage ToPipeMessageBody(this object source, PipeMessage message = null)
        {
            if (message != null)
            {
                return message.SetBody(source);
            }

            return new PipeMessage(source);
        }

        /// <summary>
        /// Gets the object from the pipe message body.
        /// </summary>
        /// <param name="source">The source PipeMessage.</param>
        /// <returns>The object from the pipe message body.</returns>
        public static object ToBodyObject(this PipeMessage source)
        {
            if (source == null)
            {
                return null;
            }

            return source.GetBody();
        }

        /// <summary>
        /// Gets the object from the pipe message body.
        /// </summary>
        /// <typeparam name="T">The type to which the message body will be deserialized.</typeparam>
        /// <param name="source">The source PipeMessage.</param>
        /// <returns>The object from the pipe message body.</returns>
        public static T ToBodyObject<T>(this PipeMessage source)
        {
            if (source == null)
            {
                return default(T);
            }

            return source.GetBody<T>();
        }

        /// <summary>
        /// Sets source object to the pipe message value.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="message">The pipe message.</param>
        /// <returns>The PipeMessage instance.</returns>
        public static PipeMessage ToPipeMessageValue(this object source, PipeMessage message = null)
        {
            if (message != null)
            {
                return message.SetValue(source);
            }

            return new PipeMessage { Value = source };
        }

        /// <summary>
        /// Gets the object from the pipe message value.
        /// </summary>
        /// <param name="source">The source PipeMessage.</param>
        /// <returns>The object from the pipe message value.</returns>
        public static object ToValueObject(this PipeMessage source)
        {
            if (source == null)
            {
                return null;
            }

            return source.Value;
        }

        /// <summary>
        /// Gets the object from the pipe message value.
        /// </summary>
        /// <typeparam name="T">The type of the message value.</typeparam>
        /// <param name="source">The source PipeMessage.</param>
        /// <returns>The object from the pipe message value.</returns>
        public static T ToValueObject<T>(this PipeMessage source)
        {
            if (source == null)
            {
                return default(T);
            }

            return source.GetValue<T>();
        }
    }
}
