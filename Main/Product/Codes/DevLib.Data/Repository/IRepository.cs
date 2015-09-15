//-----------------------------------------------------------------------
// <copyright file="IRepository.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Data.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Generic repository interface for reading and writing domain entities to a storage.
    /// </summary>
    /// <typeparam name="TEntity">Type of the entity.</typeparam>
    public interface IRepository<TEntity>
    {
        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        TEntity this[int index]
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the entity at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Entity instance.</returns>
        TEntity GetIndex(int index);

        /// <summary>
        /// Gets the entity at the specified index, and removes the element at the specified index of the repository.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get and remove.</param>
        /// <returns>Entity instance.</returns>
        TEntity GetIndexAndRemove(int index);

        /// <summary>
        /// Gets the entity at the specified last index.
        /// </summary>
        /// <param name="index">The last index.</param>
        /// <returns>Entity instance.</returns>
        TEntity GetLastIndex(int index);

        /// <summary>
        /// Gets the entity at the specified last index, and removes the element at the specified last index of the repository.
        /// </summary>
        /// <param name="index">The zero-based last index of the element to get and remove.</param>
        /// <returns>Entity instance.</returns>
        TEntity GetLastIndexAndRemove(int index);

        /// <summary>
        /// Sets the entity at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The entity instance.</param>
        void SetIndex(int index, TEntity item);

        /// <summary>
        /// Sets the entity at the specified last index.
        /// </summary>
        /// <param name="index">The last index.</param>
        /// <param name="item">The entity instance.</param>
        void SetLastIndex(int index, TEntity item);

        /// <summary>
        /// Gets the number of entities contained in the repository.
        /// </summary>
        /// <returns>The number of entities contained in the repository.</returns>
        long Count();

        /// <summary>
        /// Removes all elements from the repository.
        /// </summary>
        void Clear();

        /// <summary>
        /// Destroys the repository.
        /// </summary>
        /// <returns>true if succeeded; otherwise, false.</returns>
        bool Drop();

        /// <summary>
        /// Adds an object to the end of the repository.
        /// </summary>
        /// <param name="item">The object to be added to the end of the repository. The value can be null for reference types.</param>
        void Add(TEntity item);

        /// <summary>
        /// Adds the elements of the specified collection to the end of the repository.
        /// </summary>
        /// <param name="collection">The collection whose elements should be added to the end of the repository. The collection itself cannot be null, but it can contain elements that are null, if type TEntity is a reference type.</param>
        void AddRange(IEnumerable<TEntity> collection);

        /// <summary>
        /// Inserts entity to the repository if the primary key does not already exist, or update entity in the repository if the primary key already exists.
        /// </summary>
        /// <typeparam name="TPrimaryKey">The type of the primary key.</typeparam>
        /// <param name="entity">Entity instance.</param>
        /// <param name="getPrimaryKey">The get primary key function.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        bool AddOrUpdate<TPrimaryKey>(TEntity entity, Converter<TEntity, TPrimaryKey> getPrimaryKey);

        /// <summary>
        /// Inserts entities to the repository if the primary key does not already exist, or update entities in the repository if the primary key already exists.
        /// </summary>
        /// <typeparam name="TPrimaryKey">The type of the primary key.</typeparam>
        /// <param name="collection">The entities collection.</param>
        /// <param name="getPrimaryKey">The get primary key function.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        bool AddOrUpdate<TPrimaryKey>(IEnumerable<TEntity> collection, Converter<TEntity, TPrimaryKey> getPrimaryKey);

        /// <summary>
        /// Updates entity in the repository.
        /// </summary>
        /// <typeparam name="TPrimaryKey">The type of the primary key.</typeparam>
        /// <param name="entity">Entity instance.</param>
        /// <param name="getPrimaryKey">The get primary key function.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        bool Update<TPrimaryKey>(TEntity entity, Converter<TEntity, TPrimaryKey> getPrimaryKey);

        /// <summary>
        /// Updates entities in the repository.
        /// </summary>
        /// <typeparam name="TPrimaryKey">The type of the primary key.</typeparam>
        /// <param name="collection">The entities collection.</param>
        /// <param name="getPrimaryKey">The get primary key function.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        bool Update<TPrimaryKey>(IEnumerable<TEntity> collection, Converter<TEntity, TPrimaryKey> getPrimaryKey);

        /// <summary>
        /// Inserts an element into the repository at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        void Insert(int index, TEntity item);

        /// <summary>
        /// Inserts the elements of a collection into the repository at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="collection">The collection whose elements should be inserted into the repository. The collection itself cannot be null, but it can contain elements that are null, if type TEntity is a reference type.</param>
        void InsertRange(int index, IEnumerable<TEntity> collection);

        /// <summary>
        /// Returns a read-only <see cref="T:System.Collections.Generic.IList{T}" /> wrapper for the current collection.
        /// </summary>
        /// <returns><see cref="T:System.Collections.ObjectModel.ReadOnlyCollection{T}" /> that acts as a read-only wrapper around the current repository.</returns>
        ReadOnlyCollection<TEntity> AsReadOnly();

        /// <summary>
        /// Gets all entities from the repository.
        /// </summary>
        /// <returns>List of entity instance.</returns>
        List<TEntity> GetAll();

        /// <summary>
        /// Creates a copy of a range of elements in the source repository.
        /// </summary>
        /// <param name="index">The zero-based index at which the range starts.</param>
        /// <param name="count">The number of elements in the range.</param>
        /// <returns>A copy of a range of elements in the source repository.</returns>
        List<TEntity> GetRange(int index, int count);

        /// <summary>
        /// Creates a copy of a range of elements in the source repository, and removes them from the repository.
        /// </summary>
        /// <param name="index">The zero-based index at which the range starts.</param>
        /// <param name="count">The number of elements in the range.</param>
        /// <returns>A copy of a range of elements in the source repository.</returns>
        List<TEntity> GetRangeAndRemove(int index, int count);

        /// <summary>
        /// Creates a copy of a range of elements in the source repository of last index and count.
        /// </summary>
        /// <param name="index">The zero-based last index at which the range starts.</param>
        /// <param name="count">The number of elements in the range.</param>
        /// <returns>A copy of a range of elements in the source repository.</returns>
        List<TEntity> GetLastRange(int index, int count);

        /// <summary>
        /// Creates a copy of a range of elements in the source repository of last index and count, and removes them from the repository.
        /// </summary>
        /// <param name="index">The zero-based last index at which the range starts.</param>
        /// <param name="count">The number of elements in the range.</param>
        /// <returns>A copy of a range of elements in the source repository.</returns>
        List<TEntity> GetLastRangeAndRemove(int index, int count);

        /// <summary>
        /// Converts the elements in the current repository to another type, and returns a list containing the converted elements.
        /// </summary>
        /// <typeparam name="TOutput">The type of the elements of the target array.</typeparam>
        /// <param name="converter">A <see cref="T:System.Converter{T, TOutput}" /> delegate that converts each element from one type to another type.</param>
        /// <returns>A <see cref="T:System.Collections.Generic.List{TOutput}" /> of the target type containing the converted elements from the current repository.</returns>
        List<TOutput> ConvertAll<TOutput>(Converter<TEntity, TOutput> converter);

        /// <summary>
        /// Copies the entire repository to a compatible one-dimensional array, starting at the beginning of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the repository. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        void CopyTo(TEntity[] array);

        /// <summary>
        /// Copies the entire repository to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the repository. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        void CopyTo(TEntity[] array, int arrayIndex);

        /// <summary>
        /// Copies a range of elements from the repository to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="index">The zero-based index in the source repository at which copying begins.</param>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the repository. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <param name="count">The number of elements to copy.</param>
        void CopyTo(int index, TEntity[] array, int arrayIndex, int count);

        /// <summary>
        /// Copies the elements of the repository to a new array.
        /// </summary>
        /// <returns>An array containing copies of the elements of the repository.</returns>
        TEntity[] ToArray();

        /// <summary>
        /// Searches the entire repository for an element using the default comparer and returns the zero-based index of the element.
        /// </summary>
        /// <param name="item">The object to locate. The value can be null for reference types.</param>
        /// <returns>The zero-based index of <paramref name="item" /> in the repository, if <paramref name="item" /> is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" /> or, if there is no larger element, the bitwise complement of the repository count.</returns>
        int BinarySearch(TEntity item);

        /// <summary>
        /// Searches the entire repository for an element using the specified comparer and returns the zero-based index of the element.
        /// </summary>
        /// <param name="item">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer{T}" /> implementation to use when comparing elements.-or-null to use the default comparer <see cref="P:System.Collections.Generic.Comparer{T}.Default" />.</param>
        /// <returns>The zero-based index of <paramref name="item" /> in the repository, if <paramref name="item" /> is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" /> or, if there is no larger element, the bitwise complement of the repository count.</returns>
        int BinarySearch(TEntity item, IComparer<TEntity> comparer);

        /// <summary>
        /// Searches a range of elements in the repository for an element using the specified comparer and returns the zero-based index of the element.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range to search.</param>
        /// <param name="count">The length of the range to search.</param>
        /// <param name="item">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer{T}" /> implementation to use when comparing elements, or null to use the default comparer <see cref="P:System.Collections.Generic.Comparer{T}.Default" />.</param>
        /// <returns>The zero-based index of <paramref name="item" /> in the repository, if <paramref name="item" /> is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" /> or, if there is no larger element, the bitwise complement of the repository count.</returns>
        int BinarySearch(int index, int count, TEntity item, IComparer<TEntity> comparer);

        /// <summary>
        /// Determines whether an element is in the repository.
        /// </summary>
        /// <param name="item">The object to locate in the repository. The value can be null for reference types.</param>
        /// <returns>true if <paramref name="item" /> is found in the repository; otherwise, false.</returns>
        bool Contains(TEntity item);

        /// <summary>
        /// Determines whether the repository contains elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the elements to search for.</param>
        /// <returns>true if the repository contains one or more elements that match the conditions defined by the specified predicate; otherwise, false.</returns>
        bool Exists(Predicate<TEntity> match);

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the repository.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type TEntity.</returns>
        TEntity Find(Predicate<TEntity> match);

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the repository, and removes it from the repository.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type TEntity.</returns>
        TEntity FindAndRemove(Predicate<TEntity> match);

        /// <summary>
        /// Retrieves all the elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the elements to search for.</param>
        /// <returns>A <see cref="T:System.Collections.Generic.List{T}" /> containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty <see cref="T:System.Collections.Generic.List{T}" />.</returns>
        List<TEntity> FindAll(Predicate<TEntity> match);

        /// <summary>
        /// Retrieves all the elements that match the conditions defined by the specified predicate, and removes them all from the repository.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the elements to search for.</param>
        /// <returns>A <see cref="T:System.Collections.Generic.List{T}" /> containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty <see cref="T:System.Collections.Generic.List{T}" />.</returns>
        List<TEntity> FindAllAndRemove(Predicate<TEntity> match);

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the repository.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
        int FindIndex(Predicate<TEntity> match);

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the repository that extends from the specified index to the last element.
        /// </summary>
        /// <param name="startIndex">The zero-based starting index of the search.</param>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
        int FindIndex(int startIndex, Predicate<TEntity> match);

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the repository that starts at the specified index and contains the specified number of elements.
        /// </summary>
        /// <param name="startIndex">The zero-based starting index of the search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="match">The <see cref="T:System.Predicate`{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
        int FindIndex(int startIndex, int count, Predicate<TEntity> match);

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the repository.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type TEntity.</returns>
        TEntity FindLast(Predicate<TEntity> match);

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the repository, and removes it from the repository.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type TEntity.</returns>
        TEntity FindLastAndRemove(Predicate<TEntity> match);

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the repository.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
        int FindLastIndex(Predicate<TEntity> match);

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the repository that extends from the first element to the specified index.
        /// </summary>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
        int FindLastIndex(int startIndex, Predicate<TEntity> match);

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the repository that contains the specified number of elements and ends at the specified index.
        /// </summary>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
        int FindLastIndex(int startIndex, int count, Predicate<TEntity> match);

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the repository.
        /// </summary>
        /// <param name="item">The object to locate in the repository. The value can be null for reference types.</param>
        /// <returns>The zero-based index of the first occurrence of <paramref name="item" /> within the repository, if found; otherwise, –1.</returns>
        int IndexOf(TEntity item);

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the repository that extends from the specified index to the last element.
        /// </summary>
        /// <param name="item">The object to locate in the repository. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
        /// <returns>The zero-based index of the first occurrence of <paramref name="item" /> within the range of elements in the repository that extends from <paramref name="index" /> to the last element, if found; otherwise, –1.</returns>
        int IndexOf(TEntity item, int index);

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the repository that starts at the specified index and contains the specified number of elements.
        /// </summary>
        /// <param name="item">The object to locate in the repository. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <returns>The zero-based index of the first occurrence of <paramref name="item" /> within the range of elements in the repository that starts at <paramref name="index" /> and contains <paramref name="count" /> number of elements, if found; otherwise, –1.</returns>
        int IndexOf(TEntity item, int index, int count);

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the last occurrence within the repository.
        /// </summary>
        /// <param name="item">The object to locate in the repository. The value can be null for reference types.</param>
        /// <returns>The zero-based index of the last occurrence of <paramref name="item" /> within the repository, if found; otherwise, –1.</returns>
        int LastIndexOf(TEntity item);

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the repository that extends from the first element to the specified index.
        /// </summary>
        /// <param name="item">The object to locate in the repository. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <returns>The zero-based index of the last occurrence of <paramref name="item" /> within the range of elements in the repository that extends from the first element to <paramref name="index" />, if found; otherwise, –1.</returns>
        int LastIndexOf(TEntity item, int index);

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the repository that contains the specified number of elements and ends at the specified index.
        /// </summary>
        /// <param name="item">The object to locate in the repository. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <returns>The zero-based index of the last occurrence of <paramref name="item" /> within the range of elements in the repository that contains <paramref name="count" /> number of elements and ends at <paramref name="index" />, if found; otherwise, –1.</returns>
        int LastIndexOf(TEntity item, int index, int count);

        /// <summary>
        /// Removes the first occurrence of a specific object from the repository.
        /// </summary>
        /// <param name="item">The object to remove from the repository. The value can be null for reference types.</param>
        /// <returns>true if <paramref name="item" /> is successfully removed; otherwise, false. This method also returns false if <paramref name="item" /> was not found in the repository.</returns>
        bool Remove(TEntity item);

        /// <summary>
        /// Removes the first element that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the elements to remove.</param>
        /// <returns>true if item successfully removed; otherwise, false. This method also returns false if item was not found in the repository.</returns>
        bool Remove(Predicate<TEntity> match);

        /// <summary>
        /// Removes the last occurrence of a specific object from the repository.
        /// </summary>
        /// <param name="item">The object to remove from the repository. The value can be null for reference types.</param>
        /// <returns>true if <paramref name="item" /> is successfully removed; otherwise, false. This method also returns false if <paramref name="item" /> was not found in the repository.</returns>
        bool RemoveLast(TEntity item);

        /// <summary>
        /// Removes the last occurrence element that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the elements to remove.</param>
        /// <returns>true if item successfully removed; otherwise, false. This method also returns false if item was not found in the repository.</returns>
        bool RemoveLast(Predicate<TEntity> match);

        /// <summary>
        /// Removes all the elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the elements to remove.</param>
        /// <returns>The number of elements removed from the repository.</returns>
        int RemoveAll(Predicate<TEntity> match);

        /// <summary>
        /// Removes the element at the specified index of the repository.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        void RemoveAt(int index);

        /// <summary>
        /// Removes the element at the specified last index of the repository.
        /// </summary>
        /// <param name="index">The zero-based last index of the element to remove.</param>
        void RemoveLastAt(int index);

        /// <summary>
        /// Removes a range of elements from the repository.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        void RemoveRange(int index, int count);

        /// <summary>
        /// Removes a range of elements from the repository.
        /// </summary>
        /// <param name="index">The zero-based last starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        void RemoveLastRange(int index, int count);

        /// <summary>
        /// Removes a range of elements from the repository.
        /// </summary>
        /// <typeparam name="TPrimaryKey">The type of the primary key.</typeparam>
        /// <param name="collection">The collection whose elements should be removed from the repository.</param>
        /// <param name="getPrimaryKey">The get primary key function.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        bool RemoveRange<TPrimaryKey>(IEnumerable<TEntity> collection, Converter<TEntity, TPrimaryKey> getPrimaryKey);

        /// <summary>
        /// Reverses the order of the elements in the repository.
        /// </summary>
        void Reverse();

        /// <summary>
        /// Reverses the order of the elements in the specified range.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range to reverse.</param>
        /// <param name="count">The number of elements in the range to reverse.</param>
        void Reverse(int index, int count);

        /// <summary>
        /// Sorts the elements in the repository using the default comparer.
        /// </summary>
        void Sort();

        /// <summary>
        /// Sorts the elements in the repository using the specified comparer.
        /// </summary>
        /// <param name="comparison">The comparison.</param>
        void Sort(Comparison<TEntity> comparison);

        /// <summary>
        /// Sorts the elements in a range of elements in the repository using the specified comparer.
        /// </summary>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer{T}" /> implementation to use when comparing elements, or null to use the default comparer <see cref="P:System.Collections.Generic.Comparer{T}.Default" />.</param>
        void Sort(IComparer<TEntity> comparer);

        /// <summary>
        /// Sorts the elements in the repository using the specified <see cref="T:System.Comparison{T}" />.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <param name="comparer">The comparer.</param>
        void Sort(int index, int count, IComparer<TEntity> comparer);

        /// <summary>
        /// Performs the specified action on each element of the repository.
        /// </summary>
        /// <param name="action">The <see cref="T:System.Action{T}" /> delegate to perform on each element of the repository.</param>
        void ForEach(Action<TEntity> action);

        /// <summary>
        /// Determines whether every element in the repository matches the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions to check against the elements.</param>
        /// <returns>true if every element in the repository matches the conditions defined by the specified predicate; otherwise, false. If the list has no elements, the return value is true.</returns>
        bool TrueForAll(Predicate<TEntity> match);

        /// <summary>
        /// Calls action on the repository.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="submitChanges">true to submit changes of the repository; otherwise, false.</param>
        void ActionOnRepository(Action<List<TEntity>> action, bool submitChanges);

        /// <summary>
        /// Calls function on the repository.
        /// </summary>
        /// <typeparam name="TResult">The type of return value.</typeparam>
        /// <param name="func">The function.</param>
        /// <param name="submitChanges">true to submit changes of the repository; otherwise, false.</param>
        /// <returns>Result of the function.</returns>
        TResult FuncOnRepository<TResult>(Converter<List<TEntity>, TResult> func, bool submitChanges);
    }
}
