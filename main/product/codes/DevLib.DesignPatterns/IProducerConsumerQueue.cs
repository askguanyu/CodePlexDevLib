//-----------------------------------------------------------------------
// <copyright file="IProducerConsumerQueue.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    /// <summary>
    /// Represents a first-in, first-out collection of objects interface.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements in the queue.</typeparam>
    public interface IProducerConsumerQueue<T>
    {
        /// <summary>
        /// Gets the number of elements contained in the <see cref="IProducerConsumerQueue{T}" />.
        /// </summary>
        int Count
        {
            get;
        }

        /// <summary>
        /// Removes all objects from the <see cref="IProducerConsumerQueue{T}" />.
        /// </summary>
        void Clear();

        /// <summary>
        /// Determines whether an element is in the <see cref="IProducerConsumerQueue{T}" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="IProducerConsumerQueue{T}" />. The value can be null for reference types.</param>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="IProducerConsumerQueue{T}" />; otherwise, false.</returns>
        bool Contains(T item);

        /// <summary>
        /// Removes and returns the object at the beginning of the <see cref="IProducerConsumerQueue{T}" />.
        /// </summary>
        /// <returns>The object that is removed from the beginning of the <see cref="IProducerConsumerQueue{T}" />.</returns>
        T Dequeue();

        /// <summary>
        /// Adds an object to the end of the <see cref="IProducerConsumerQueue{T}" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="IProducerConsumerQueue{T}" />. The value can be null for reference types.</param>
        void Enqueue(T item);

        /// <summary>
        /// Returns the object at the beginning of the <see cref="IProducerConsumerQueue{T}" /> without removing it.
        /// </summary>
        /// <returns>The object at the beginning of the <see cref="IProducerConsumerQueue{T}" />.</returns>
        T Peek();
    }
}
