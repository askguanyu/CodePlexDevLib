//-----------------------------------------------------------------------
// <copyright file="EventHandlerExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Threading;

    /// <summary>
    /// EventHandler Extensions.
    /// </summary>
    public static class EventHandlerExtensions
    {
        /// <summary>
        /// Thread safety raise event.
        /// </summary>
        /// <param name="source">Source EventHandler.</param>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        public static void RaiseEvent(this EventHandler source, object sender, EventArgs e = null)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler safeHandler = Interlocked.CompareExchange(ref source, null, null);

            if (safeHandler != null)
            {
                safeHandler(sender, e);
            }
        }

        /// <summary>
        /// Thread safety raise event.
        /// </summary>
        /// <typeparam name="T">The type of the event.</typeparam>
        /// <param name="source">Source EventHandler{T}.</param>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        public static void RaiseEvent<T>(this EventHandler<T> source, object sender, T e = null) where T : EventArgs
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<T> safeHandler = Interlocked.CompareExchange(ref source, null, null);

            if (safeHandler != null)
            {
                safeHandler(sender, e);
            }
        }
    }
}
