//-----------------------------------------------------------------------
// <copyright file="RemotingEventSink.cs" company="YuGuan Corporation">
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
    public class RemotingEventSink : MarshalByRefObject
    {
        /// <summary>
        /// Represents the event that will raises in transparent proxy of remoting object.
        /// </summary>
        public event EventHandler RemotingEventOccurred;

        /// <summary>
        /// Thread safety raise event in the real remoting object type.
        /// </summary>
        /// <param name="eventHandler">Source EventHandler.</param>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        public static void RaiseEvent(EventHandler eventHandler, object sender, EventArgs e = null)
        {
            EventHandler safeSourceHandler = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (safeSourceHandler != null)
            {
                EventHandler tempHandler = null;

                foreach (Delegate invocation in safeSourceHandler.GetInvocationList())
                {
                    tempHandler = invocation as EventHandler;

                    EventHandler safeTempHandler = Interlocked.CompareExchange(ref tempHandler, null, null);

                    if (safeTempHandler != null)
                    {
                        try
                        {
                            safeTempHandler(sender, e);
                        }
                        catch
                        {
                            safeSourceHandler -= safeTempHandler;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Raise event method for transparent proxy of remoting object.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        public void RaiseRemotingEvent(object sender, EventArgs e)
        {
            EventHandler safeHandler = Interlocked.CompareExchange(ref this.RemotingEventOccurred, null, null);

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
    public class RemotingEventSink<T> : MarshalByRefObject where T : EventArgs
    {
        /// <summary>
        /// Represents the event that will raises in transparent proxy of remoting object.
        /// </summary>
        public event EventHandler<T> RemotingEventOccurred;

        /// <summary>
        /// Thread safety raise event in the real remoting object type.
        /// </summary>
        /// <param name="eventHandler">Source EventHandler{T}.</param>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        public static void RaiseEvent(EventHandler<T> eventHandler, object sender, T e)
        {
            EventHandler<T> safeSourceHandler = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (safeSourceHandler != null)
            {
                EventHandler<T> tempHandler = null;

                foreach (Delegate invocation in safeSourceHandler.GetInvocationList())
                {
                    tempHandler = invocation as EventHandler<T>;

                    EventHandler<T> safeTempHandler = Interlocked.CompareExchange(ref tempHandler, null, null);

                    if (safeTempHandler != null)
                    {
                        try
                        {
                            safeTempHandler(sender, e);
                        }
                        catch
                        {
                            safeSourceHandler -= safeTempHandler;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Raise event method for transparent proxy of remoting object.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        public void RaiseRemotingEvent(object sender, T e)
        {
            EventHandler<T> safeHandler = Interlocked.CompareExchange(ref this.RemotingEventOccurred, null, null);

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
