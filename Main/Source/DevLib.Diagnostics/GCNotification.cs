//-----------------------------------------------------------------------
// <copyright file="GCNotification.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Diagnostics
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    /// GC notification.
    /// </summary>
    public static class GCNotification
    {
        /// <summary>
        /// Field _gcDone.
        /// </summary>
        private static Action<int> _gcDone = null;

        /// <summary>
        /// Occurs when GC is done.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "Reviewed.")]
        public static event Action<int> GCDone
        {
            add
            {
                if (_gcDone == null)
                {
                    new GenObject(0);
                    new GenObject(2);
                }

                _gcDone += value;
            }

            remove
            {
                _gcDone -= value;
            }
        }

        /// <summary>
        /// Generation object.
        /// </summary>
        private sealed class GenObject
        {
            /// <summary>
            /// Field _generation.
            /// </summary>
            private int _generation;

            /// <summary>
            /// Initializes a new instance of the <see cref="GenObject"/> class.
            /// </summary>
            /// <param name="generation">The generation.</param>
            public GenObject(int generation)
            {
                this._generation = generation;
            }

            /// <summary>
            /// Finalizes an instance of the <see cref="GenObject"/> class.
            /// </summary>
            ~GenObject()
            {
                if (GC.GetGeneration(this) >= this._generation)
                {
                    Action<int> temp = VolatileRead(ref _gcDone);

                    if (temp != null)
                    {
                        temp(this._generation);
                    }
                }

                if (_gcDone != null
                    && !AppDomain.CurrentDomain.IsFinalizingForUnload()
                    && !Environment.HasShutdownStarted)
                {
                    if (this._generation == 0)
                    {
                        new GenObject(0);
                    }
                    else
                    {
                        GC.ReRegisterForFinalize(this);
                    }
                }
            }

            /// <summary>
            /// Reads the object reference from the specified field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method.
            /// </summary>
            /// <typeparam name="T">The type of field to read. This must be a reference type, not a value type.</typeparam>
            /// <param name="location">The field to read.</param>
            /// <returns>The reference to T that was read. This reference is the latest written by any processor in the computer, regardless of the number of processors or the state of processor cache. </returns>
            private static T VolatileRead<T>(ref T location) where T : class
            {
                T value = location;
                Thread.MemoryBarrier();
                return value;
            }
        }
    }
}
