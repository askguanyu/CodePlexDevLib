//-----------------------------------------------------------------------
// <copyright file="FileBaseRepository.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Data.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// File base repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class FileBaseRepository<TEntity> : IRepository<TEntity>, IDisposable
    {
        /// <summary>
        /// Field _fileMutex.
        /// </summary>
        private readonly Mutex _fileMutex;

        /// <summary>
        /// Field _file.
        /// </summary>
        private readonly string _file;

        /// <summary>
        /// Field _readFileFunc.
        /// </summary>
        private readonly Converter<string, List<TEntity>> _readFileFunc;

        /// <summary>
        /// Field _writeFileAction.
        /// </summary>
        private readonly WriteFileAction<string, object> _writeFileAction;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBaseRepository{TEntity}" /> class.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="readFileFunc">The read file function.</param>
        /// <param name="writeFileAction">The write file action.</param>
        public FileBaseRepository(string filename, Converter<string, List<TEntity>> readFileFunc, WriteFileAction<string, object> writeFileAction)
        {
            this._file = Path.GetFullPath(filename);
            this._fileMutex = RepositoryHelper.CreateSharedMutex(RepositoryHelper.GetSharedFileMutexName(this._file));
            this._readFileFunc = readFileFunc;
            this._writeFileAction = writeFileAction;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="FileBaseRepository{TEntity}" /> class.
        /// </summary>
        ~FileBaseRepository()
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

            return this.FuncOnList<TEntity>(i => i[index], false);
        }

        /// <summary>
        /// Sets the entity at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The entity instance.</param>
        public void SetIndex(int index, TEntity item)
        {
            this.CheckDisposed();

            this.ActionOnList(i => i[index] = item, true);
        }

        /// <summary>
        /// Gets the number of entities contained in the repository.
        /// </summary>
        /// <returns>The number of entities contained in the repository.</returns>
        public long Count()
        {
            this.CheckDisposed();

            return this.FuncOnList<long>(i => i.Count < int.MaxValue ? i.Count : RepositoryHelper.LongCount(i), false);
        }

        /// <summary>
        /// Removes all elements from the repository.
        /// </summary>
        public void Clear()
        {
            this.CheckDisposed();

            this.ActionOnList(i => i.Clear(), true);
        }

        /// <summary>
        /// Destroys the repository.
        /// </summary>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Drop()
        {
            this.CheckDisposed();

            if (this._fileMutex == null)
            {
                return false;
            }

            try
            {
                this._fileMutex.WaitOne();
            }
            catch
            {
            }

            try
            {
                File.Delete(this._file);
                return true;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                return false;
            }
            finally
            {
                this._fileMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Adds an object to the end of the repository.
        /// </summary>
        /// <param name="item">The object to be added to the end of the repository. The value can be null for reference types.</param>
        public void Add(TEntity item)
        {
            this.CheckDisposed();

            this.ActionOnList(i => i.Add(item), true);
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

            this.ActionOnList(i => i.AddRange(collection), true);
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

            return this.FuncOnList<bool>(i =>
            {
                TPrimaryKey primaryKey = getPrimaryKey(entity);

                int index = i.FindIndex(item => getPrimaryKey(item).Equals(primaryKey));

                if (index < 0)
                {
                    i.Add(entity);
                }
                else
                {
                    i[index] = entity;
                }

                return true;
            }, true);
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

            return this.FuncOnList<bool>(i =>
            {
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

                int keyIndex = 0;

                foreach (TEntity entity in collection)
                {
                    int index = i.FindIndex(item => getPrimaryKey(item).Equals(primaryKeys[keyIndex]));

                    if (index < 0)
                    {
                        i.Add(entity);
                    }
                    else
                    {
                        i[index] = entity;
                    }

                    keyIndex++;
                }

                return true;
            }, true);
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

            return this.FuncOnList<bool>(i =>
            {
                TPrimaryKey primaryKey = getPrimaryKey(entity);

                int index = i.FindIndex(item => getPrimaryKey(item).Equals(primaryKey));

                if (index >= 0)
                {
                    i[index] = entity;
                    return true;
                }

                return false;
            }, true);
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

            return this.FuncOnList<bool>(i =>
            {
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

                int keyIndex = 0;

                foreach (TEntity entity in collection)
                {
                    int index = i.FindIndex(item => getPrimaryKey(item).Equals(primaryKeys[keyIndex]));

                    if (index >= 0)
                    {
                        i[index] = entity;
                    }

                    keyIndex++;
                }

                return true;
            }, true);
        }

        /// <summary>
        /// Inserts an element into the repository at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        public void Insert(int index, TEntity item)
        {
            this.CheckDisposed();

            this.ActionOnList(i => i.Insert(index, item), true);
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

            this.ActionOnList(i => i.InsertRange(index, collection), true);
        }

        /// <summary>
        /// Returns a read-only <see cref="T:System.Collections.Generic.IList{T}" /> wrapper for the current collection.
        /// </summary>
        /// <returns><see cref="T:System.Collections.ObjectModel.ReadOnlyCollection{T}" /> that acts as a read-only wrapper around the current repository.</returns>
        public ReadOnlyCollection<TEntity> AsReadOnly()
        {
            this.CheckDisposed();

            return this.FuncOnList<ReadOnlyCollection<TEntity>>(i => i.AsReadOnly(), false);
        }

        /// <summary>
        /// Gets all entities from the repository.
        /// </summary>
        /// <returns>List of entity instance.</returns>
        public List<TEntity> GetAll()
        {
            this.CheckDisposed();

            return this.FuncOnList<List<TEntity>>(i => i, false);
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

            return this.FuncOnList<List<TEntity>>(i => i.GetRange(index, count), false);
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

            return this.FuncOnList<List<TOutput>>(i => i.ConvertAll(converter), false);
        }

        /// <summary>
        /// Copies the entire repository to a compatible one-dimensional array, starting at the beginning of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the repository. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        public void CopyTo(TEntity[] array)
        {
            this.CheckDisposed();

            this.ActionOnList(i => i.CopyTo(array), false);
        }

        /// <summary>
        /// Copies the entire repository to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the repository. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(TEntity[] array, int arrayIndex)
        {
            this.CheckDisposed();

            this.ActionOnList(i => i.CopyTo(array, arrayIndex), false);
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

            this.ActionOnList(i => i.CopyTo(index, array, arrayIndex, count), false);
        }

        /// <summary>
        /// Copies the elements of the repository to a new array.
        /// </summary>
        /// <returns>An array containing copies of the elements of the repository.</returns>
        public TEntity[] ToArray()
        {
            this.CheckDisposed();

            return this.FuncOnList<TEntity[]>(i => i.ToArray(), false);
        }

        /// <summary>
        /// Searches the entire repository for an element using the default comparer and returns the zero-based index of the element.
        /// </summary>
        /// <param name="item">The object to locate. The value can be null for reference types.</param>
        /// <returns>The zero-based index of <paramref name="item" /> in the repository, if <paramref name="item" /> is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" /> or, if there is no larger element, the bitwise complement of the repository count.</returns>
        public int BinarySearch(TEntity item)
        {
            this.CheckDisposed();

            return this.FuncOnList<int>(i => i.BinarySearch(item), false);
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

            return this.FuncOnList<int>(i => i.BinarySearch(item, comparer), false);
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

            return this.FuncOnList<int>(i => i.BinarySearch(index, count, item, comparer), false);
        }

        /// <summary>
        /// Determines whether an element is in the repository.
        /// </summary>
        /// <param name="item">The object to locate in the repository. The value can be null for reference types.</param>
        /// <returns>true if <paramref name="item" /> is found in the repository; otherwise, false.</returns>
        public bool Contains(TEntity item)
        {
            this.CheckDisposed();

            return this.FuncOnList<bool>(i => i.Contains(item), false);
        }

        /// <summary>
        /// Determines whether the repository contains elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the elements to search for.</param>
        /// <returns>true if the repository contains one or more elements that match the conditions defined by the specified predicate; otherwise, false.</returns>
        public bool Exists(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            return this.FuncOnList<bool>(i => i.Exists(match), false);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the repository.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type TEntity.</returns>
        public TEntity Find(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            return this.FuncOnList<TEntity>(i => i.Find(match), false);
        }

        /// <summary>
        /// Retrieves all the elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the elements to search for.</param>
        /// <returns>A <see cref="T:System.Collections.Generic.List{T}" /> containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty <see cref="T:System.Collections.Generic.List{T}" />.</returns>
        public List<TEntity> FindAll(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            return this.FuncOnList<List<TEntity>>(i => i.FindAll(match), false);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the repository.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
        public int FindIndex(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            return this.FuncOnList<int>(i => i.FindIndex(match), false);
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

            return this.FuncOnList<int>(i => i.FindIndex(startIndex, match), false);
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

            return this.FuncOnList<int>(i => i.FindIndex(startIndex, count, match), false);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the repository.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type TEntity.</returns>
        public TEntity FindLast(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            return this.FuncOnList<TEntity>(i => i.FindLast(match), false);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the repository.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
        public int FindLastIndex(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            return this.FuncOnList<int>(i => i.FindLastIndex(match), false);
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

            return this.FuncOnList<int>(i => i.FindLastIndex(startIndex, match), false);
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

            return this.FuncOnList<int>(i => i.FindLastIndex(startIndex, count, match), false);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the repository.
        /// </summary>
        /// <param name="item">The object to locate in the repository. The value can be null for reference types.</param>
        /// <returns>The zero-based index of the first occurrence of <paramref name="item" /> within the repository, if found; otherwise, –1.</returns>
        public int IndexOf(TEntity item)
        {
            this.CheckDisposed();

            return this.FuncOnList<int>(i => i.IndexOf(item), false);
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

            return this.FuncOnList<int>(i => i.IndexOf(item, index), false);
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

            return this.FuncOnList<int>(i => i.IndexOf(item, index, count), false);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the last occurrence within the repository.
        /// </summary>
        /// <param name="item">The object to locate in the repository. The value can be null for reference types.</param>
        /// <returns>The zero-based index of the last occurrence of <paramref name="item" /> within the repository, if found; otherwise, –1.</returns>
        public int LastIndexOf(TEntity item)
        {
            this.CheckDisposed();

            return this.FuncOnList<int>(i => i.LastIndexOf(item), false);
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

            return this.FuncOnList<int>(i => i.LastIndexOf(item, index), false);
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

            return this.FuncOnList<int>(i => i.LastIndexOf(item, index, count), false);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the repository.
        /// </summary>
        /// <param name="item">The object to remove from the repository. The value can be null for reference types.</param>
        /// <returns>true if <paramref name="item" /> is successfully removed; otherwise, false. This method also returns false if <paramref name="item" /> was not found in the repository.</returns>
        public bool Remove(TEntity item)
        {
            this.CheckDisposed();

            return this.FuncOnList<bool>(i => i.Remove(item), true);
        }

        /// <summary>
        /// Removes the first element that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the elements to remove.</param>
        /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the repository.</returns>
        public bool Remove(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            return this.FuncOnList<bool>(i =>
            {
                int index = i.FindIndex(match);

                if (index < 0)
                {
                    return false;
                }
                else
                {
                    i.RemoveAt(index);

                    return true;
                }
            }, true);
        }

        /// <summary>
        /// Removes all the elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions of the elements to remove.</param>
        /// <returns>The number of elements removed from the repository.</returns>
        public int RemoveAll(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            return this.FuncOnList<int>(i => i.RemoveAll(match), true);
        }

        /// <summary>
        /// Removes the element at the specified index of the repository.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            this.CheckDisposed();

            this.ActionOnList(i => i.RemoveAt(index), true);
        }

        /// <summary>
        /// Removes a range of elements from the repository.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        public void RemoveRange(int index, int count)
        {
            this.CheckDisposed();

            this.ActionOnList(i => i.RemoveRange(index, count), true);
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

            return this.FuncOnList<bool>(i =>
            {
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

                int keyIndex = 0;

                foreach (TEntity entity in collection)
                {
                    int index = i.FindIndex(item => getPrimaryKey(item).Equals(primaryKeys[keyIndex]));

                    if (index >= 0)
                    {
                        i.RemoveAt(index);
                    }

                    keyIndex++;
                }

                return true;
            }, true);
        }

        /// <summary>
        /// Reverses the order of the elements in the repository.
        /// </summary>
        public void Reverse()
        {
            this.CheckDisposed();

            this.ActionOnList(i => i.Reverse(), true);
        }

        /// <summary>
        /// Reverses the order of the elements in the specified range.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range to reverse.</param>
        /// <param name="count">The number of elements in the range to reverse.</param>
        public void Reverse(int index, int count)
        {
            this.CheckDisposed();

            this.ActionOnList(i => i.Reverse(index, count), true);
        }

        /// <summary>
        /// Sorts the elements in the repository using the default comparer.
        /// </summary>
        public void Sort()
        {
            this.CheckDisposed();

            this.ActionOnList(i => i.Sort(), true);
        }

        /// <summary>
        /// Sorts the elements in the repository using the specified comparer.
        /// </summary>
        /// <param name="comparison">The comparison.</param>
        public void Sort(Comparison<TEntity> comparison)
        {
            this.CheckDisposed();

            this.ActionOnList(i => i.Sort(comparison), true);
        }

        /// <summary>
        /// Sorts the elements in a range of elements in the repository using the specified comparer.
        /// </summary>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer{T}" /> implementation to use when comparing elements, or null to use the default comparer <see cref="P:System.Collections.Generic.Comparer{T}.Default" />.</param>
        public void Sort(IComparer<TEntity> comparer)
        {
            this.CheckDisposed();

            this.ActionOnList(i => i.Sort(comparer), true);
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

            this.ActionOnList(i => i.Sort(index, count, comparer), true);
        }

        /// <summary>
        /// Performs the specified action on each element of the repository.
        /// </summary>
        /// <param name="action">The <see cref="T:System.Action{T}" /> delegate to perform on each element of the repository.</param>
        public void ForEach(Action<TEntity> action)
        {
            this.CheckDisposed();

            this.ActionOnList(i => i.ForEach(action), false);
        }

        /// <summary>
        /// Determines whether every element in the repository matches the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="T:System.Predicate{T}" /> delegate that defines the conditions to check against the elements.</param>
        /// <returns>true if every element in the repository matches the conditions defined by the specified predicate; otherwise, false. If the list has no elements, the return value is true.</returns>
        public bool TrueForAll(Predicate<TEntity> match)
        {
            this.CheckDisposed();

            return this.FuncOnList<bool>(i => i.TrueForAll(match), false);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="FileBaseRepository{TEntity}" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="FileBaseRepository{TEntity}" /> class.
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

                if (this._fileMutex != null)
                {
                    this._fileMutex.Close();
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
                throw new ObjectDisposedException("DevLib.Data.Repository.FileBaseRepository");
            }
        }

        /// <summary>
        /// Calls action of list.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="persistence">true to write the repository to file; otherwise, false.</param>
        private void ActionOnList(Action<List<TEntity>> action, bool persistence)
        {
            if (this._fileMutex == null)
            {
                return;
            }

            try
            {
                this._fileMutex.WaitOne();
            }
            catch
            {
            }

            try
            {
                List<TEntity> repository = null;

                try
                {
                    repository = this._readFileFunc(this._file);
                }
                catch (FileNotFoundException)
                {
                    repository = new List<TEntity>();
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                    throw;
                }

                action(repository);

                if (persistence)
                {
                    this._writeFileAction(this._file, repository);
                }
            }
            finally
            {
                this._fileMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Calls function of list.
        /// </summary>
        /// <typeparam name="T">The type of return value.</typeparam>
        /// <param name="func">The function.</param>
        /// <param name="persistence">true to write the repository to file; otherwise, false.</param>
        /// <returns>Result of the function.</returns>
        private T FuncOnList<T>(Converter<List<TEntity>, T> func, bool persistence)
        {
            if (this._fileMutex == null)
            {
                return default(T);
            }

            try
            {
                this._fileMutex.WaitOne();
            }
            catch
            {
            }

            try
            {
                List<TEntity> repository = null;

                try
                {
                    repository = this._readFileFunc(this._file);
                }
                catch (FileNotFoundException)
                {
                    repository = new List<TEntity>();
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                    throw;
                }

                T result = func(repository);

                if (persistence)
                {
                    this._writeFileAction(this._file, repository);
                }

                return result;
            }
            finally
            {
                this._fileMutex.ReleaseMutex();
            }
        }
    }
}
