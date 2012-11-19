//-----------------------------------------------------------------------
// <copyright file="AsyncSocketServerEventArgsPool.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.AsyncSocket
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Net.Sockets;

    /// <summary>
    /// Represents a collection of resusable SocketAsyncEventArgs objects.
    /// </summary>
    internal class AsyncSocketServerEventArgsPool
    {
        /// <summary>
        /// The SocketAsyncEventArgs object pool.
        /// </summary>
        private Stack<SocketAsyncEventArgs> _pool;

        /// <summary>
        /// Initializes the object pool to the specified size.
        /// </summary>
        public AsyncSocketServerEventArgsPool()
        {
            this._pool = new Stack<SocketAsyncEventArgs>();
        }

        /// <summary>
        /// The number of SocketAsyncEventArgs instances in the pool.
        /// </summary>
        public int Count
        {
            get
            {
                lock (((ICollection)this._pool).SyncRoot)
                {
                    return this._pool.Count;
                }
            }
        }

        /// <summary>
        /// Add a SocketAsyncEventArg instance to the pool.
        /// </summary>
        /// <param name="item">The SocketAsyncEventArgs instance to add to the pool.</param>
        public void Push(SocketAsyncEventArgs item)
        {
            lock (((ICollection)this._pool).SyncRoot)
            {
                this._pool.Push(item);
            }
        }

        /// <summary>
        /// Removes a SocketAsyncEventArgs instance from the pool.
        /// </summary>
        /// <returns>The object removed from the pool.</returns>
        public SocketAsyncEventArgs Pop()
        {
            lock (((ICollection)this._pool).SyncRoot)
            {
                return this._pool.Pop();
            }
        }
    }
}
