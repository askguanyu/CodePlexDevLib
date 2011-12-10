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
        /// <param name="handler"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void RaiseEvent<T>(this EventHandler<T> handler, object sender, T e) where T : EventArgs
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<T> safeHandler = Interlocked.CompareExchange(ref handler, null, null);

            if (safeHandler != null)
            {
                safeHandler(sender, e);
            }
        }
    }
}
