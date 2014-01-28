//-----------------------------------------------------------------------
// <copyright file="ProducerConsumerQueue.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a first-in, first-out collection of objects for <see cref="ProducerConsumer{T}" />.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements in the queue.</typeparam>
    public class ProducerConsumerQueue<T> : IProducerConsumerQueue<T>
    {
        /// <summary>
        /// The queue that contains the data items.
        /// </summary>
        private readonly Queue<T> _queue = new Queue<T>();

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ProducerConsumerQueue{T}" />.
        /// </summary>
        public long Count
        {
            get
            {
                return this._queue.Count;
            }
        }

        /// <summary>
        /// Adds an object to the end of the <see cref="ProducerConsumerQueue{T}" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ProducerConsumerQueue{T}" />. The value can be null for reference types.</param>
        public void Enqueue(T item)
        {
            this._queue.Enqueue(item);
        }

        /// <summary>
        /// Adds objects to the end of the <see cref="ProducerConsumerQueue{T}" />.
        /// </summary>
        /// <param name="items">The objects to add to the <see cref="ProducerConsumerQueue{T}" />. The value can be null for reference types.</param>
        public void Enqueue(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                this._queue.Enqueue(item);
            }
        }

        /// <summary>
        /// Removes and returns the object at the beginning of the <see cref="ProducerConsumerQueue{T}" />.
        /// </summary>
        /// <returns>The object that is removed from the beginning of the <see cref="ProducerConsumerQueue{T}" />.</returns>
        public T Dequeue()
        {
            return this._queue.Dequeue();
        }

        /// <summary>
        /// Returns the object at the beginning of the <see cref="ProducerConsumerQueue{T}" /> without removing it.
        /// </summary>
        /// <returns>The object at the beginning of the <see cref="ProducerConsumerQueue{T}" />.</returns>
        public T Peek()
        {
            return this._queue.Peek();
        }

        /// <summary>
        /// Determines whether an element is in the <see cref="ProducerConsumerQueue{T}" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ProducerConsumerQueue{T}" />. The value can be null for reference types.</param>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="ProducerConsumerQueue{T}" />; otherwise, false.</returns>
        public bool Contains(T item)
        {
            return this._queue.Contains(item);
        }

        /// <summary>
        /// Removes all objects from the <see cref="ProducerConsumerQueue{T}" />.
        /// </summary>
        public void Clear()
        {
            this._queue.Clear();
        }
    }
}
