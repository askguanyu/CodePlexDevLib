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
    /// EventHandler Extensions
    /// </summary>
    public static class EventHandlerExtensions
    {
        /// <summary>
        /// Thread safety raise event
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void RaiseEvent<T>(this EventHandler<T> source, object sender, T e) where T : EventArgs
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
