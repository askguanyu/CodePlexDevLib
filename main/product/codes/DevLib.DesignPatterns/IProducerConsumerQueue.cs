//-----------------------------------------------------------------------
// <copyright file="IProducerConsumerQueue.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a first-in, first-out collection of objects interface.
    /// </summary>
    public interface IProducerConsumerQueue
    {
        /// <summary>
        /// Gets the number of elements contained in the <see cref="IProducerConsumerQueue" />.
        /// </summary>
        /// <returns>The number of elements.</returns>
        long Count();

        /// <summary>
        /// Adds an object to the end of the <see cref="IProducerConsumerQueue" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="IProducerConsumerQueue" />. The value can be null for reference types.</param>
        void Enqueue(object item);

        /// <summary>
        /// Adds objects to the end of the <see cref="IProducerConsumerQueue" />.
        /// </summary>
        /// <param name="items">The objects to add to the <see cref="IProducerConsumerQueue" />. The value can be null for reference types.</param>
        void Enqueue(IEnumerable items);

        /// <summary>
        /// Removes and returns the object at the beginning of the <see cref="IProducerConsumerQueue" />.
        /// </summary>
        /// <returns>The object that is removed from the beginning of the <see cref="IProducerConsumerQueue" />.</returns>
        object Dequeue();

        /// <summary>
        /// Returns the object at the beginning of the <see cref="IProducerConsumerQueue" /> without removing it.
        /// </summary>
        /// <returns>The object at the beginning of the <see cref="IProducerConsumerQueue" />.</returns>
        object Peek();

        /// <summary>
        /// Determines whether an element is in the <see cref="IProducerConsumerQueue" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="IProducerConsumerQueue" />. The value can be null for reference types.</param>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="IProducerConsumerQueue" />; otherwise, false.</returns>
        bool Contains(object item);

        /// <summary>
        /// Removes all objects from the <see cref="IProducerConsumerQueue" />.
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Represents a first-in, first-out collection of objects interface.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements in the queue.</typeparam>
    public interface IProducerConsumerQueue<T> : IProducerConsumerQueue
    {
        /// <summary>
        /// Adds an object to the end of the <see cref="IProducerConsumerQueue{T}" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="IProducerConsumerQueue{T}" />. The value can be null for reference types.</param>
        void Enqueue(T item);

        /// <summary>
        /// Adds objects to the end of the <see cref="IProducerConsumerQueue{T}" />.
        /// </summary>
        /// <param name="items">The objects to add to the <see cref="IProducerConsumerQueue{T}" />. The value can be null for reference types.</param>
        void Enqueue(IEnumerable<T> items);

        /// <summary>
        /// Removes and returns the object at the beginning of the <see cref="IProducerConsumerQueue{T}" />.
        /// </summary>
        /// <returns>The object that is removed from the beginning of the <see cref="IProducerConsumerQueue{T}" />.</returns>
        new T Dequeue();

        /// <summary>
        /// Returns the object at the beginning of the <see cref="IProducerConsumerQueue{T}" /> without removing it.
        /// </summary>
        /// <returns>The object at the beginning of the <see cref="IProducerConsumerQueue{T}" />.</returns>
        new T Peek();

        /// <summary>
        /// Determines whether an element is in the <see cref="IProducerConsumerQueue{T}" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="IProducerConsumerQueue{T}" />. The value can be null for reference types.</param>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="IProducerConsumerQueue{T}" />; otherwise, false.</returns>
        bool Contains(T item);
    }
}
