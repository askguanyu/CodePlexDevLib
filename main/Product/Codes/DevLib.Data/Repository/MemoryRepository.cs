//-----------------------------------------------------------------------
// <copyright file="MemoryRepository.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Data.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Memory repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class MemoryRepository<TEntity> : IRepository<TEntity>, IDisposable
    {
        /// <summary>
        /// Field _syncRoot.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Field _repository.
        /// </summary>
        private List<TEntity> _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryRepository{TEntity}"/> class.
        /// </summary>
        public MemoryRepository()
        {
            this._repository = new List<TEntity>();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="MemoryRepository{TEntity}" /> class.
        /// </summary>
        ~MemoryRepository()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public TEntity this[int index]
        {
            get
            {
                return this.GetIndex(index);
            }

            set
            {
                this.SetIndex(index, value);
            }
        }

        /// <summary>
        /// Gets the entity at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Entity instance.</returns>
        public TEntity GetIndex(int index)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return default(TEntity);
                }

                return this._repository[index];
            }
        }

        /// <summary>
        /// Gets the entity at the specified index, and removes the element at the specified index of the repository.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get and remove.</param>
        /// <returns>Entity instance.</returns>
        public TEntity GetIndexAndRemove(int index)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return default(TEntity);
                }

                TEntity result = this._repository[index];
                this._repository.RemoveAt(index);
                return RepositoryHelper.CloneDeep(result);
            }
        }

        /// <summary>
        /// Gets the entity at the specified last index.
        /// </summary>
        /// <param name="index">The last index.</param>
        /// <returns>Entity instance.</returns>
        public TEntity GetLastIndex(int index)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return default(TEntity);
                }

                return RepositoryHelper.CloneDeep(this._repository[this._repository.Count - index - 1]);
            }
        }

        /// <summary>
        /// Gets the entity at the specified last index, and removes the element at the specified last index of the repository.
        /// </summary>
        /// <param name="index">The zero-based last index of the element to get and remove.</param>
        /// <returns>Entity instance.</returns>
        public TEntity GetLastIndexAndRemove(int index)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return default(TEntity);
                }

                int startIndex = this._repository.Count - index - 1;
                TEntity result = this._repository[startIndex];
                this._repository.RemoveAt(startIndex);
                return RepositoryHelper.CloneDeep(result);
            }
        }

        /// <summary>
        /// Sets the entity at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The entity instance.</param>
        public void SetIndex(int index, TEntity item)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                this._repository[index] = RepositoryHelper.CloneDeep(item);
            }
        }

        /// <summary>
        /// Sets the entity at the specified last index.
        /// </summary>
        /// <param name="index">The last index.</param>
        /// <param name="item">The entity instance.</param>
        public void SetLastIndex(int index, TEntity item)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                this._repository[this._repository.Count - index - 1] = RepositoryHelper.CloneDeep(item);
            }
        }

        /// <summary>
        /// Gets the number of entities contained in the repository.
        /// </summary>
        /// <returns>The number of entities contained in the repository.</returns>
        public long Count()
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return -1;
                }

                return this._repository.Count < int.MaxValue ? this._repository.Count : RepositoryHelper.LongCount(this._repository);
            }
        }

        /// <summary>
        /// Removes all elements from the repository.
        /// </summary>
        public void Clear()
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                this._repository.Clear();
            }
        }

        /// <summary>
        /// Destroys the repository.
        /// </summary>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Drop()
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return false;
                }

                try
                {
                    this._repository.Clear();
                    this._repository = new List<TEntity>();

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Adds an object to the end of the repository.
        /// </summary>
        /// <param name="item">The object to be added to the end of the repository. The value can be null for reference types.</param>
        public void Add(TEntity item)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                this._repository.Add(RepositoryHelper.CloneDeep(item));
            }
        }

        /// <summary>
        /// Adds the elements of the specified collection to the end of the repository.
        /// </summary>
        /// <param name="collection">The collection whose elements should be added to the end of the repository. The collection itself cannot be null, but it can contain elements that are null, if type TEntity is a reference type.</param>
        public void AddRange(IEnumerable<TEntity> collection)
        {
            this.CheckDisposed();

            if (RepositoryHelper.IsEnumerableNullOrEmpty(collection))
            {
                return;
            }

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                this._repository.AddRange(RepositoryHelper.CloneDeep(collection));
            }
        }

        /// <summary>
        /// Inserts entity to the repository if the primary key does not already exist, or update entity in the repository if the primary key already exists.
        /// </summary>
        /// <typeparam name="TPrimaryKey">The type of the primary key.</typeparam>
        /// <param name="entity">Entity instance.</param>
        /// <param name="getPrimaryKey">The get primary key function.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool AddOrUpdate<TPrimaryKey>(TEntity entity, Converter<TEntity, TPrimaryKey> getPrimaryKey)
        {
            this.CheckDisposed();

            if (getPrimaryKey == null)
            {
                return false;
            }

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return false;
                }

                try
                {
                    TPrimaryKey primaryKey = getPrimaryKey(entity);

                    int index = this._repository.FindIndex(item => getPrimaryKey(item).Equals(primaryKey));

                    if (index < 0)
                    {
                        this._repository.Add(RepositoryHelper.CloneDeep(entity));
                    }
                    else
                    {
                        this._repository[index] = RepositoryHelper.CloneDeep(entity);
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Inserts entities to the repository if the primary key does not already exist, or update entities in the repository if the primary key already exists.
        /// </summary>
        /// <typeparam name="TPrimaryKey">The type of the primary key.</typeparam>
        /// <param name="collection">The entities collection.</param>
        /// <param name="getPrimaryKey">The get primary key function.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool AddOrUpdate<TPrimaryKey>(IEnumerable<TEntity> collection, Converter<TEntity, TPrimaryKey> getPrimaryKey)
        {
            this.CheckDisposed();

            if (RepositoryHelper.IsEnumerableNullOrEmpty(collection))
            {
                return false;
            }

            if (getPrimaryKey == null)
            {
                return false;
            }

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return false;
                }

                List<TPrimaryKey> primaryKeys = new List<TPrimaryKey>();

                try
                {
                    foreach (TEntity entity in collection)
                    {
                        primaryKeys.Add(getPrimaryKey(entity));
                    }
                }
                catch
                {
                    return false;
                }

                try
                {
                    int keyIndex = 0;

                    foreach (TEntity entity in collection)
                    {
                        int index = this._repository.FindIndex(item => getPrimaryKey(item).Equals(primaryKeys[keyIndex]));

                        if (index < 0)
                        {
                            this._repository.Add(RepositoryHelper.CloneDeep(entity));
                        }
                        else
                        {
                            this._repository[index] = RepositoryHelper.CloneDeep(entity);
                        }

                        keyIndex++;
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Updates entity in the repository.
        /// </summary>
        /// <typeparam name="TPrimaryKey">The type of the primary key.</typeparam>
        /// <param name="entity">Entity instance.</param>
        /// <param name="getPrimaryKey">The get primary key function.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Update<TPrimaryKey>(TEntity entity, Converter<TEntity, TPrimaryKey> getPrimaryKey)
        {
            this.CheckDisposed();

            if (getPrimaryKey == null)
            {
                return false;
            }

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return false;
                }

                try
                {
                    TPrimaryKey primaryKey = getPrimaryKey(entity);

                    int index = this._repository.FindIndex(item => getPrimaryKey(item).Equals(primaryKey));

                    if (index >= 0)
                    {
                        this._repository[index] = RepositoryHelper.CloneDeep(entity);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Updates entities in the repository.
        /// </summary>
        /// <typeparam name="TPrimaryKey">The type of the primary key.</typeparam>
        /// <param name="collection">The entities collection.</param>
        /// <param name="getPrimaryKey">The get primary key function.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Update<TPrimaryKey>(IEnumerable<TEntity> collection, Converter<TEntity, TPrimaryKey> getPrimaryKey)
        {
            this.CheckDisposed();

            if (RepositoryHelper.IsEnumerableNullOrEmpty(collection))
            {
                return false;
            }

            if (getPrimaryKey == null)
            {
                return false;
            }

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return false;
                }

                List<TPrimaryKey> primaryKeys = new List<TPrimaryKey>();

                try
                {
                    foreach (TEntity entity in collection)
                    {
                        primaryKeys.Add(getPrimaryKey(entity));
                    }
                }
                catch
                {
                    return false;
                }

                try
                {
                    int keyIndex = 0;

                    foreach (TEntity entity in collection)
                    {
                        int index = this._repository.FindIndex(item => getPrimaryKey(item).Equals(primaryKeys[keyIndex]));

                        if (index >= 0)
                        {
                            this._repository[index] = RepositoryHelper.CloneDeep(entity);
                        }

                        keyIndex++;
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Inserts an element into the repository at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        public void Insert(int index, TEntity item)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                this._repository.Insert(index, RepositoryHelper.CloneDeep(item));
            }
        }

        /// <summary>
        /// Inserts the elements of a collection into the repository at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="collection">The collection whose elements should be inserted into the repository. The collection itself cannot be null, but it can contain elements that are null, if type TEntity is a reference type.</param>
        public void InsertRange(int index, IEnumerable<TEntity> collection)
        {
            this.CheckDisposed();

            if (RepositoryHelper.IsEnumerableNullOrEmpty(collection))
            {
                return;
            }

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                this._repository.InsertRange(index, RepositoryHelper.CloneDeep(collection));
            }
        }

        /// <summary>
        /// Returns a read-only <see cref="T:System.Collections.Generic.IList{T}" /> wrapper for the current collection.
        /// </summary>
        /// <returns><see cref="T:System.Collections.ObjectModel.ReadOnlyCollection{T}" /> that acts as a read-only wrapper around the current repository.</returns>
        public ReadOnlyCollection<TEntity> AsReadOnly()
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return null;
                }

                return RepositoryHelper.CloneDeep(this._repository).AsReadOnly();
            }
        }

        /// <summary>
        /// Gets all entities from the repository.
        /// </summary>
        /// <returns>List of entity instance.</returns>
        public List<TEntity> GetAll()
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return null;
                }

                return RepositoryHelper.CloneDeep(this._repository);
            }
        }

        /// <summary>
        /// Creates a copy of a range of elements in the source repository.
        /// </summary>
        /// <param name="index">The zero-based index at which the range starts.</param>
        /// <param name="count">The number of elements in the range.</param>
        /// <returns>A copy of a range of elements in the source repository.</returns>
        public List<TEntity> GetRange(int index, int count)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return null;
                }

                return RepositoryHelper.CloneDeep(this._repository.GetRange(index, count));
            }
        }

        /// <summary>
        /// Creates a copy of a range of elements in the source repository, and removes them from the repository.
        /// </summary>
        /// <param name="index">The zero-based index at which the range starts.</param>
        /// <param name="count">The number of elements in the range.</param>
        /// <returns>A copy of a range of elements in the source repository.</returns>
        public List<TEntity> GetRangeAndRemove(int index, int count)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return null;
                }

                List<TEntity> result = this._repository.GetRange(index, count);
                this._repository.RemoveRange(index, count);
                return RepositoryHelper.CloneDeep(result);
            }
        }

        /// <summary>
        /// Creates a copy of a range of elements in the source repository of last index and count.
        /// </summary>
        /// <param name="index">The zero-based last index at which the range starts.</param>
        /// <param name="count">The number of elements in the range.</param>
        /// <returns>A copy of a range of elements in the source repository.</returns>
        public List<TEntity> GetLastRange(int index, int count)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return null;
                }

                return RepositoryHelper.CloneDeep(this._repository.GetRange(this._repository.Count - index - count, count));
            }
        }

        /// <summary>
        /// Creates a copy of a range of elements in the source repository of last index and count, and removes them from the repository.
        /// </summary>
        /// <param name="index">The zero-based last index at which the range starts.</param>
        /// <param name="count">The number of elements in the range.</param>
        /// <returns>A copy of a range of elements in the source repository.</returns>
        public List<TEntity> GetLastRangeAndRemove(int index, int count)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return null;
                }

                int startIndex = this._repository.Count - index - count;
                List<TEntity> result = this._repository.GetRange(startIndex, count);
                this._repository.RemoveRange(startIndex, count);
                return RepositoryHelper.CloneDeep(result);
            }
        }

        /// <summary>
        /// Converts the elements in the current repository to another type, and returns a list containing the converted elements.
        /// </summary>
        /// <typeparam name="TOutput">The type of the elements of the target array.</typeparam>
        /// <param name="converter">A <see cref="T:System.Converter{T, TOutput}" /> delegate that converts each element from one type to another type.</param>
        /// <returns>A <see cref="T:System.Collections.Generic.List{TOutput}" /> of the target type containing the converted elements from the current repository.</returns>
        public List<TOutput> ConvertAll<TOutput>(Converter<TEntity, TOutput> converter)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return null;
                }

                return RepositoryHelper.CloneDeep(this._repository).ConvertAll(converter);
            }
        }

        /// <summary>
        /// Copies the entire repository to a compatible one-dimensional array, starting at the beginning of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the repository. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        public void CopyTo(TEntity[] array)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                RepositoryHelper.CloneDeep(this._repository).CopyTo(array);
            }
        }

        /// <summary>
        /// Copies the entire repository to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the repository. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(TEntity[] array, int arrayIndex)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                RepositoryHelper.CloneDeep(this._repository).CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        /// Copies a range of elements from the repository to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="index">The zero-based index in the source repository at which copying begins.</param>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the repository. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <param name="count">The number of elements to copy.</param>
        public void CopyTo(int index, TEntity[] array, int arrayIndex, int count)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                RepositoryHelper.CloneDeep(this._repository).CopyTo(index, array, arrayIndex, count);
            }
        }

        /// <summary>
        /// Copies the elements of the repository to a new array.
        /// </summary>
        /// <returns>An array containing copies of the elements of the repository.</returns>
        public TEntity[] ToArray()
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return null;
                }

                return RepositoryHelper.CloneDeep(this._repository).ToArray();
            }
        }

        /// <summary>
        /// Searches the entire repository for an element using the default comparer and returns the zero-based index of the element.
        /// </summary>
        /// <param name="item">The object to locate. The value can be null for reference types.</param>
        /// <returns>The zero-based index of <paramref name="item" /> in the repository, if <paramref name="item" /> is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" /> or, if there is no larger element, the bitwise complement of the repository count.</returns>
        public int BinarySearch(TEntity item)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return -1;
                }

                return this._repository.BinarySearch(item);
            }
        }

        /// <summary>
        /// Searches the entire repository for an element using the specified comparer and returns the zero-based index of the element.
        /// </summary>
        /// <param name="item">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer{T}" /> implementation to use when comparing elements.-or-null to use the default comparer <see cref="P:System.Collections.Generic.Comparer{T}.Default" />.</param>
        /// <returns>The zero-based index of <paramref name="item" /> in the repository, if <paramref name="item" /> is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" /> or, if there is no larger element, the bitwise complement of the repository count.</returns>
        public int BinarySearch(TEntity item, IComparer<TEntity> comparer)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return -1;
                }

                return this._repository.BinarySearch(item, comparer);
            }
        }

        /// <summary>
        /// Searches a range of elements in the repository for an element using the specified comparer and returns the zero-based index of the element.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range to search.</param>
        /// <param name="count">The length of the range to search.</param>
        /// <param name="item">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer{T}" /> implementation to use when comparing elements, or null to use the default comparer <see cref="P:System.Collections.Generic.Comparer{T}.Default" />.</param>
        /// <returns>The zero-based index of <paramref name="item" /> in the repository, if <paramref name="item" /> is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" /> or, if there is no larger element, the bitwise complement of the repository count.</returns>
        public int BinarySearch(int index, int count, TEntity item, IComparer<TEntity> comparer)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return -1;
                }

                return this._repository.BinarySearch(index, count, item, comparer);
            }
        }

        /// <summary>
        /// Determines whether an element is in the repository.
        /// </summary>
        /// <param name="item">The object to locate in the repository. The value can be null for reference types.</param>
        /// <returns>true if <paramref name="item" /> is found in the repository; otherwise, false.</returns>
        public bool Contains(TEntity item)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return false;
                }

                return this._repository.Contains(item);
            }
        }

        /// <summary>
        /// Determines whether the repository contains elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the elements to search for.</param>
        /// <returns>true if the repository contains one or more elements that match the conditions defined by the specified predicate; otherwise, false.</returns>
        public bool Exists(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return false;
                }

                return this._repository.Exists(match);
            }
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the repository.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type TEntity.</returns>
        public TEntity Find(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return default(TEntity);
                }

                return RepositoryHelper.CloneDeep(this._repository.Find(match));
            }
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the repository, and removes it from the repository.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type TEntity.</returns>
        public TEntity FindAndRemove(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return default(TEntity);
                }

                int index = this._repository.FindIndex(match);

                if (index >= 0)
                {
                    TEntity result = this._repository[index];
                    this._repository.RemoveAt(index);
                    return RepositoryHelper.CloneDeep(result);
                }
                else
                {
                    return default(TEntity);
                }
            }
        }

        /// <summary>
        /// Retrieves all the elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the elements to search for.</param>
        /// <returns>A <see cref="T:System.Collections.Generic.List{T}" /> containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty <see cref="T:System.Collections.Generic.List{T}" />.</returns>
        public List<TEntity> FindAll(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return null;
                }

                return RepositoryHelper.CloneDeep(this._repository.FindAll(match));
            }
        }

        /// <summary>
        /// Retrieves all the elements that match the conditions defined by the specified predicate, and removes them all from the repository.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the elements to search for.</param>
        /// <returns>A <see cref="T:System.Collections.Generic.List{T}" /> containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty <see cref="T:System.Collections.Generic.List{T}" />.</returns>
        public List<TEntity> FindAllAndRemove(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return null;
                }

                List<TEntity> result = this._repository.FindAll(match);
                this._repository.RemoveAll(match);
                return RepositoryHelper.CloneDeep(result);
            }
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the repository.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
        public int FindIndex(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return -1;
                }

                return this._repository.FindIndex(match);
            }
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the repository that extends from the specified index to the last element.
        /// </summary>
        /// <param name="startIndex">The zero-based starting index of the search.</param>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
        public int FindIndex(int startIndex, Predicate<TEntity> match)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return -1;
                }

                return this._repository.FindIndex(startIndex, match);
            }
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the repository that starts at the specified index and contains the specified number of elements.
        /// </summary>
        /// <param name="startIndex">The zero-based starting index of the search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="match">The <see cref="T:System.Predicate`{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
        public int FindIndex(int startIndex, int count, Predicate<TEntity> match)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return -1;
                }

                return this._repository.FindIndex(startIndex, count, match);
            }
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the repository.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type TEntity.</returns>
        public TEntity FindLast(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return default(TEntity);
                }

                return RepositoryHelper.CloneDeep(this._repository.FindLast(match));
            }
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the repository, and removes it from the repository.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type TEntity.</returns>
        public TEntity FindLastAndRemove(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return default(TEntity);
                }

                int index = this._repository.FindLastIndex(match);

                if (index >= 0)
                {
                    TEntity result = this._repository[index];
                    this._repository.RemoveAt(index);
                    return RepositoryHelper.CloneDeep(result);
                }
                else
                {
                    return default(TEntity);
                }
            }
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the repository.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
        public int FindLastIndex(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return -1;
                }

                return this._repository.FindLastIndex(match);
            }
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the repository that extends from the first element to the specified index.
        /// </summary>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
        public int FindLastIndex(int startIndex, Predicate<TEntity> match)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return -1;
                }

                return this._repository.FindLastIndex(startIndex, match);
            }
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the repository that contains the specified number of elements and ends at the specified index.
        /// </summary>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
        public int FindLastIndex(int startIndex, int count, Predicate<TEntity> match)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return -1;
                }

                return this._repository.FindLastIndex(startIndex, count, match);
            }
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the repository.
        /// </summary>
        /// <param name="item">The object to locate in the repository. The value can be null for reference types.</param>
        /// <returns>The zero-based index of the first occurrence of <paramref name="item" /> within the repository, if found; otherwise, –1.</returns>
        public int IndexOf(TEntity item)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return -1;
                }

                return this._repository.IndexOf(item);
            }
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the repository that extends from the specified index to the last element.
        /// </summary>
        /// <param name="item">The object to locate in the repository. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
        /// <returns>The zero-based index of the first occurrence of <paramref name="item" /> within the range of elements in the repository that extends from <paramref name="index" /> to the last element, if found; otherwise, –1.</returns>
        public int IndexOf(TEntity item, int index)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return -1;
                }

                return this._repository.IndexOf(item, index);
            }
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the repository that starts at the specified index and contains the specified number of elements.
        /// </summary>
        /// <param name="item">The object to locate in the repository. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <returns>The zero-based index of the first occurrence of <paramref name="item" /> within the range of elements in the repository that starts at <paramref name="index" /> and contains <paramref name="count" /> number of elements, if found; otherwise, –1.</returns>
        public int IndexOf(TEntity item, int index, int count)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return -1;
                }

                return this._repository.IndexOf(item, index, count);
            }
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the last occurrence within the repository.
        /// </summary>
        /// <param name="item">The object to locate in the repository. The value can be null for reference types.</param>
        /// <returns>The zero-based index of the last occurrence of <paramref name="item" /> within the repository, if found; otherwise, –1.</returns>
        public int LastIndexOf(TEntity item)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return -1;
                }

                return this._repository.LastIndexOf(item);
            }
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the repository that extends from the first element to the specified index.
        /// </summary>
        /// <param name="item">The object to locate in the repository. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <returns>The zero-based index of the last occurrence of <paramref name="item" /> within the range of elements in the repository that extends from the first element to <paramref name="index" />, if found; otherwise, –1.</returns>
        public int LastIndexOf(TEntity item, int index)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return -1;
                }

                return this._repository.LastIndexOf(item, index);
            }
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the repository that contains the specified number of elements and ends at the specified index.
        /// </summary>
        /// <param name="item">The object to locate in the repository. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <returns>The zero-based index of the last occurrence of <paramref name="item" /> within the range of elements in the repository that contains <paramref name="count" /> number of elements and ends at <paramref name="index" />, if found; otherwise, –1.</returns>
        public int LastIndexOf(TEntity item, int index, int count)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return -1;
                }

                return this._repository.LastIndexOf(item, index, count);
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the repository.
        /// </summary>
        /// <param name="item">The object to remove from the repository. The value can be null for reference types.</param>
        /// <returns>true if <paramref name="item" /> is successfully removed; otherwise, false. This method also returns false if <paramref name="item" /> was not found in the repository.</returns>
        public bool Remove(TEntity item)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return false;
                }

                return this._repository.Remove(item);
            }
        }

        /// <summary>
        /// Removes the first element that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the elements to remove.</param>
        /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the repository.</returns>
        public bool Remove(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return false;
                }

                int index = this._repository.FindIndex(match);

                if (index < 0)
                {
                    return false;
                }
                else
                {
                    this._repository.RemoveAt(index);

                    return true;
                }
            }
        }

        /// <summary>
        /// Removes the last occurrence of a specific object from the repository.
        /// </summary>
        /// <param name="item">The object to remove from the repository. The value can be null for reference types.</param>
        /// <returns>true if <paramref name="item" /> is successfully removed; otherwise, false. This method also returns false if <paramref name="item" /> was not found in the repository.</returns>
        public bool RemoveLast(TEntity item)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return false;
                }

                int index = this._repository.LastIndexOf(item);

                if (index >= 0)
                {
                    this._repository.RemoveAt(index);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Removes the last occurrence element that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the elements to remove.</param>
        /// <returns>true if item successfully removed; otherwise, false. This method also returns false if item was not found in the repository.</returns>
        public bool RemoveLast(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return false;
                }

                int index = this._repository.FindLastIndex(match);

                if (index >= 0)
                {
                    this._repository.RemoveAt(index);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Removes all the elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the elements to remove.</param>
        /// <returns>The number of elements removed from the repository.</returns>
        public int RemoveAll(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return -1;
                }

                return this._repository.RemoveAll(match);
            }
        }

        /// <summary>
        /// Removes the element at the specified index of the repository.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                this._repository.RemoveAt(index);
            }
        }

        /// <summary>
        /// Removes the element at the specified last index of the repository.
        /// </summary>
        /// <param name="index">The zero-based last index of the element to remove.</param>
        public void RemoveLastAt(int index)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                this._repository.RemoveAt(this._repository.Count - index - 1);
            }
        }

        /// <summary>
        /// Removes a range of elements from the repository.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        public void RemoveRange(int index, int count)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                this._repository.RemoveRange(index, count);
            }
        }

        /// <summary>
        /// Removes a range of elements from the repository.
        /// </summary>
        /// <param name="index">The zero-based last starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        public void RemoveLastRange(int index, int count)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                this._repository.RemoveRange(this._repository.Count - index - count, count);
            }
        }

        /// <summary>
        /// Removes a range of elements from the repository.
        /// </summary>
        /// <typeparam name="TPrimaryKey">The type of the primary key.</typeparam>
        /// <param name="collection">The collection whose elements should be removed from the repository.</param>
        /// <param name="getPrimaryKey">The get primary key function.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool RemoveRange<TPrimaryKey>(IEnumerable<TEntity> collection, Converter<TEntity, TPrimaryKey> getPrimaryKey)
        {
            this.CheckDisposed();

            if (RepositoryHelper.IsEnumerableNullOrEmpty(collection))
            {
                return false;
            }

            if (getPrimaryKey == null)
            {
                return false;
            }

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return false;
                }

                List<TPrimaryKey> primaryKeys = new List<TPrimaryKey>();

                try
                {
                    foreach (TEntity entity in collection)
                    {
                        primaryKeys.Add(getPrimaryKey(entity));
                    }
                }
                catch
                {
                    return false;
                }

                try
                {
                    int keyIndex = 0;

                    foreach (TEntity entity in collection)
                    {
                        int index = this._repository.FindIndex(item => getPrimaryKey(item).Equals(primaryKeys[keyIndex]));

                        if (index >= 0)
                        {
                            this._repository.RemoveAt(index);
                        }

                        keyIndex++;
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Reverses the order of the elements in the repository.
        /// </summary>
        public void Reverse()
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                this._repository.Reverse();
            }
        }

        /// <summary>
        /// Reverses the order of the elements in the specified range.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range to reverse.</param>
        /// <param name="count">The number of elements in the range to reverse.</param>
        public void Reverse(int index, int count)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                this._repository.Reverse(index, count);
            }
        }

        /// <summary>
        /// Sorts the elements in the repository using the default comparer.
        /// </summary>
        public void Sort()
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                this._repository.Sort();
            }
        }

        /// <summary>
        /// Sorts the elements in the repository using the specified comparer.
        /// </summary>
        /// <param name="comparison">The comparison.</param>
        public void Sort(Comparison<TEntity> comparison)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                this._repository.Sort(comparison);
            }
        }

        /// <summary>
        /// Sorts the elements in a range of elements in the repository using the specified comparer.
        /// </summary>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer{T}" /> implementation to use when comparing elements, or null to use the default comparer <see cref="P:System.Collections.Generic.Comparer{T}.Default" />.</param>
        public void Sort(IComparer<TEntity> comparer)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                this._repository.Sort(comparer);
            }
        }

        /// <summary>
        /// Sorts the elements in the repository using the specified <see cref="T:System.Comparison{T}" />.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <param name="comparer">The comparer.</param>
        public void Sort(int index, int count, IComparer<TEntity> comparer)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                this._repository.Sort(index, count, comparer);
            }
        }

        /// <summary>
        /// Performs the specified action on each element of the repository.
        /// </summary>
        /// <param name="action">The <see cref="T:System.Action{T}" /> delegate to perform on each element of the repository.</param>
        public void ForEach(Action<TEntity> action)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                this._repository.ForEach(action);
            }
        }

        /// <summary>
        /// Determines whether every element in the repository matches the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions to check against the elements.</param>
        /// <returns>true if every element in the repository matches the conditions defined by the specified predicate; otherwise, false. If the list has no elements, the return value is true.</returns>
        public bool TrueForAll(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return false;
                }

                return this._repository.TrueForAll(match);
            }
        }

        /// <summary>
        /// Calls action on the repository.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="submitChanges">true to submit changes of the repository; otherwise, false.</param>
        public void ActionOnRepository(Action<List<TEntity>> action, bool submitChanges)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return;
                }

                action(this._repository);
            }
        }

        /// <summary>
        /// Calls function on the repository.
        /// </summary>
        /// <typeparam name="TResult">The type of return value.</typeparam>
        /// <param name="func">The function.</param>
        /// <param name="submitChanges">true to submit changes of the repository; otherwise, false.</param>
        /// <returns>Result of the function.</returns>
        public TResult FuncOnRepository<TResult>(Converter<List<TEntity>, TResult> func, bool submitChanges)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                if (this._disposed)
                {
                    return default(TResult);
                }

                return func(this._repository);
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="MemoryRepository{TEntity}" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="MemoryRepository{TEntity}" /> class.
        /// protected virtual for non-sealed class; private for sealed class.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;

            if (disposing)
            {
                // dispose managed resources
                ////if (managedResource != null)
                ////{
                ////    managedResource.Dispose();
                ////    managedResource = null;
                ////}

                lock (this._syncRoot)
                {
                    this._repository.Clear();
                    this._repository = null;
                }
            }

            // free native resources
            ////if (nativeResource != IntPtr.Zero)
            ////{
            ////    Marshal.FreeHGlobal(nativeResource);
            ////    nativeResource = IntPtr.Zero;
            ////}
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.Data.Repository.MemoryRepository");
            }
        }
    }
}
