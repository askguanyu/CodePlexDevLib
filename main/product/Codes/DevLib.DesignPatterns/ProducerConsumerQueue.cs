//-----------------------------------------------------------------------
// <copyright file="ProducerConsumerQueue.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a first-in, first-out collection of objects for <see cref="ProducerConsumer" />.
    /// </summary>
    public class ProducerConsumerQueue : IProducerConsumerQueue
    {
        /// <summary>
        /// The queue that contains the data items.
        /// </summary>
        private readonly Queue _queue = new Queue();

        /// <summary>
        /// Gets the number of elements contained in the <see cref="IProducerConsumerQueue" />.
        /// </summary>
        /// <returns>The number of elements.</returns>
        public long Count()
        {
            return this._queue.Count;
        }

        /// <summary>
        /// Adds an object to the end of the <see cref="ProducerConsumerQueue" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ProducerConsumerQueue" />. The value can be null for reference types.</param>
        public void Enqueue(object item)
        {
            this._queue.Enqueue(item);
        }

        /// <summary>
        /// Adds objects to the end of the <see cref="ProducerConsumerQueue" />.
        /// </summary>
        /// <param name="items">The objects to add to the <see cref="ProducerConsumerQueue" />. The value can be null for reference types.</param>
        /// <returns>Items count.</returns>
        public long Enqueue(IEnumerable items)
        {
            long result = 0;

            if (items != null)
            {
                foreach (var item in items)
                {
                    this._queue.Enqueue(item);
                    result++;
                }
            }

            return result;
        }

        /// <summary>
        /// Removes and returns the object at the beginning of the <see cref="ProducerConsumerQueue" />.
        /// </summary>
        /// <returns>The object that is removed from the beginning of the <see cref="ProducerConsumerQueue" />.</returns>
        public object Dequeue()
        {
            return this._queue.Dequeue();
        }

        /// <summary>
        /// Returns the object at the beginning of the <see cref="ProducerConsumerQueue{T}" /> without removing it.
        /// </summary>
        /// <returns>The object at the beginning of the <see cref="ProducerConsumerQueue{T}" />.</returns>
        public object Peek()
        {
            return this._queue.Peek();
        }

        /// <summary>
        /// Determines whether an element is in the <see cref="ProducerConsumerQueue{T}" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ProducerConsumerQueue{T}" />. The value can be null for reference types.</param>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="ProducerConsumerQueue{T}" />; otherwise, false.</returns>
        public bool Contains(object item)
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
        /// Gets the number of elements contained in the <see cref="IProducerConsumerQueue{T}" />.
        /// </summary>
        /// <returns>The number of elements.</returns>
        public long Count()
        {
            return this._queue.Count;
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
        /// Adds an object to the end of the <see cref="ProducerConsumerQueue{T}" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ProducerConsumerQueue{T}" />. The value can be null for reference types.</param>
        public void Enqueue(object item)
        {
            this._queue.Enqueue((T)item);
        }

        /// <summary>
        /// Adds objects to the end of the <see cref="ProducerConsumerQueue{T}" />.
        /// </summary>
        /// <param name="items">The objects to add to the <see cref="ProducerConsumerQueue{T}" />. The value can be null for reference types.</param>
        /// <returns>Items count.</returns>
        public long Enqueue(IEnumerable<T> items)
        {
            long result = 0;

            if (items != null)
            {
                foreach (var item in items)
                {
                    this._queue.Enqueue(item);
                    result++;
                }
            }

            return result;
        }

        /// <summary>
        /// Adds objects to the end of the <see cref="ProducerConsumerQueue{T}" />.
        /// </summary>
        /// <param name="items">The objects to add to the <see cref="ProducerConsumerQueue{T}" />. The value can be null for reference types.</param>
        /// <returns>Items count.</returns>
        public long Enqueue(IEnumerable items)
        {
            long result = 0;

            if (items != null)
            {
                foreach (var item in items)
                {
                    this._queue.Enqueue((T)item);
                    result++;
                }
            }

            return result;
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
        /// Determines whether an element is in the <see cref="ProducerConsumerQueue{T}" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ProducerConsumerQueue{T}" />. The value can be null for reference types.</param>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="ProducerConsumerQueue{T}" />; otherwise, false.</returns>
        public bool Contains(object item)
        {
            return this._queue.Contains((T)item);
        }

        /// <summary>
        /// Removes all objects from the <see cref="ProducerConsumerQueue{T}" />.
        /// </summary>
        public void Clear()
        {
            this._queue.Clear();
        }

        /// <summary>
        /// Removes and returns the object at the beginning of the <see cref="ProducerConsumerQueue{T}" />.
        /// </summary>
        /// <returns>The object that is removed from the beginning of the <see cref="ProducerConsumerQueue{T}" />.</returns>
        object IProducerConsumerQueue.Dequeue()
        {
            return this._queue.Dequeue();
        }

        /// <summary>
        /// Returns the object at the beginning of the <see cref="ProducerConsumerQueue{T}" /> without removing it.
        /// </summary>
        /// <returns>The object at the beginning of the <see cref="ProducerConsumerQueue{T}" />.</returns>
        object IProducerConsumerQueue.Peek()
        {
            return this._queue.Peek();
        }
    }
}
