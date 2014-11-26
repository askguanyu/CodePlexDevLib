//-----------------------------------------------------------------------
// <copyright file="RemotingObjectEventProxy.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Remoting
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Permissions;
    using System.Threading;

    /// <summary>
    /// Provides a proxy mechanism for communicating that allows objects events to interact with each other across application domains or processes.
    /// </summary>
    [Serializable]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed.")]
    public class RemotingObjectEventProxy : MarshalByRefObject
    {
        /// <summary>
        /// Represents the event that will raises in transparent proxy of RemotingObject.
        /// </summary>
        public event EventHandler RemotingObjectEventOccurred;

        /// <summary>
        /// Thread safety raise event in the real RemotingObject type.
        /// </summary>
        /// <param name="source">Source EventHandler.</param>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        public static void RaiseEvent(EventHandler source, object sender, EventArgs e = null)
        {
            EventHandler safeHandler = Interlocked.CompareExchange(ref source, null, null);

            if (safeHandler != null)
            {
                EventHandler tempHandler = null;

                foreach (Delegate invocation in source.GetInvocationList())
                {
                    tempHandler = invocation as EventHandler;

                    if (tempHandler != null)
                    {
                        try
                        {
                            tempHandler(sender, e);
                        }
                        catch
                        {
                            source -= tempHandler;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Raise event method for transparent proxy of RemotingObject.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        public void OnRemotingObjectEvent(object sender, EventArgs e)
        {
            EventHandler safeHandler = Interlocked.CompareExchange(ref this.RemotingObjectEventOccurred, null, null);

            if (safeHandler != null)
            {
                safeHandler(sender, e);
            }
        }

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns>An infinite lifetime.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }

    /// <summary>
    /// Provides a proxy mechanism for communicating that allows objects events to interact with each other across application domains or processes.
    /// </summary>
    /// <typeparam name="T">The type of the event.</typeparam>
    [Serializable]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed.")]
    public class RemotingObjectEventProxy<T> : MarshalByRefObject where T : EventArgs
    {
        /// <summary>
        /// Represents the event that will raises in transparent proxy of RemotingObject.
        /// </summary>
        public event EventHandler<T> RemotingObjectEventOccurred;

        /// <summary>
        /// Thread safety raise event in the real RemotingObject type.
        /// </summary>
        /// <param name="source">Source EventHandler{T}.</param>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        public static void RaiseEvent(EventHandler<T> source, object sender, T e)
        {
            EventHandler<T> safeHandler = Interlocked.CompareExchange(ref source, null, null);

            if (safeHandler != null)
            {
                EventHandler<T> tempHandler = null;

                foreach (Delegate invocation in source.GetInvocationList())
                {
                    tempHandler = invocation as EventHandler<T>;

                    if (tempHandler != null)
                    {
                        try
                        {
                            tempHandler(sender, e);
                        }
                        catch
                        {
                            source -= tempHandler;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Raise event method for transparent proxy of RemotingObject.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        public void OnRemotingObjectEvent(object sender, T e)
        {
            EventHandler<T> safeHandler = Interlocked.CompareExchange(ref this.RemotingObjectEventOccurred, null, null);

            if (safeHandler != null)
            {
                safeHandler(sender, e);
            }
        }

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns>An infinite lifetime.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
