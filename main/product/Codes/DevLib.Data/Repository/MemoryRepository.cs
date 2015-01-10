//-----------------------------------------------------------------------
// <copyright file="MemoryRepository.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Data.Repository
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Memory repository.
    /// </summary>
    /// <typeparam name="TPrimaryKey">The type of the entity primary key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class MemoryRepository<TPrimaryKey, TEntity> : IRepository<TPrimaryKey, TEntity>, IDisposable
    {
        /// <summary>
        /// Field _getPrimaryKeyFunc.
        /// </summary>
        private readonly GetPrimaryKeyFunc<TEntity, TPrimaryKey> _getPrimaryKeyFunc;

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
        private Dictionary<TPrimaryKey, TEntity> _repository;

        /// <summary>
        /// Field _nullPrimaryKeyEntity.
        /// </summary>
        private NullablePrimaryKeyEntity<TEntity> _nullPrimaryKeyEntity;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryRepository{TPrimaryKey, TEntity}" /> class.
        /// </summary>
        /// <param name="getPrimaryKeyFunc">The get entity primary key function.</param>
        public MemoryRepository(GetPrimaryKeyFunc<TEntity, TPrimaryKey> getPrimaryKeyFunc)
        {
            this._repository = new Dictionary<TPrimaryKey, TEntity>();
            this._nullPrimaryKeyEntity = new NullablePrimaryKeyEntity<TEntity>();
            this._getPrimaryKeyFunc = getPrimaryKeyFunc;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="MemoryRepository{TPrimaryKey, TEntity}" /> class.
        /// </summary>
        ~MemoryRepository()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="MemoryRepository{TPrimaryKey, TEntity}" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Inserts entity to the repository.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Insert(TEntity entity)
        {
            this.CheckDisposed();

            if (entity == null)
            {
                return false;
            }

            try
            {
                TPrimaryKey primaryKey = this._getPrimaryKeyFunc(entity);

                lock (this._syncRoot)
                {
                    try
                    {
                        if (!this._repository.ContainsKey(primaryKey))
                        {
                            this._repository.Add(primaryKey, RepositoryHelper.CloneDeep(entity));
                            return true;
                        }
                    }
                    catch (ArgumentNullException)
                    {
                        if (!this._nullPrimaryKeyEntity.HasValue)
                        {
                            this._nullPrimaryKeyEntity.HasValue = true;
                            this._nullPrimaryKeyEntity.Value = RepositoryHelper.CloneDeep(entity);

                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                    }

                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Inserts entities to the repository.
        /// </summary>
        /// <param name="entities">Entity instances.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Insert(IEnumerable<TEntity> entities)
        {
            this.CheckDisposed();

            if (RepositoryHelper.IsEnumerableNullOrEmpty(entities))
            {
                return false;
            }

            List<TPrimaryKey> primaryKeys = new List<TPrimaryKey>();

            try
            {
                foreach (TEntity entity in entities)
                {
                    primaryKeys.Add(this._getPrimaryKeyFunc(entity));
                }
            }
            catch
            {
                return false;
            }

            lock (this._syncRoot)
            {
                int index = 0;

                foreach (TEntity entity in entities)
                {
                    try
                    {
                        if (!this._repository.ContainsKey(primaryKeys[index]))
                        {
                            this._repository.Add(primaryKeys[index], RepositoryHelper.CloneDeep(entity));
                        }
                    }
                    catch (ArgumentNullException)
                    {
                        if (!this._nullPrimaryKeyEntity.HasValue)
                        {
                            this._nullPrimaryKeyEntity.HasValue = true;
                            this._nullPrimaryKeyEntity.Value = RepositoryHelper.CloneDeep(entity);
                        }
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                        return false;
                    }

                    index++;
                }

                return true;
            }
        }

        /// <summary>
        /// Inserts entity to the repository if the primary key does not already exist, or update entity in the repository if the primary key already exists.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool InsertOrUpdate(TEntity entity)
        {
            this.CheckDisposed();

            if (entity == null)
            {
                return false;
            }

            try
            {
                TPrimaryKey primaryKey = this._getPrimaryKeyFunc(entity);

                lock (this._syncRoot)
                {
                    try
                    {
                        this._repository[primaryKey] = RepositoryHelper.CloneDeep(entity);
                        return true;
                    }
                    catch (ArgumentNullException)
                    {
                        this._nullPrimaryKeyEntity.HasValue = true;
                        this._nullPrimaryKeyEntity.Value = RepositoryHelper.CloneDeep(entity);

                        return true;
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Inserts entities to the repository if the primary key does not already exist, or update entities in the repository if the primary key already exists.
        /// </summary>
        /// <param name="entities">Entity instances.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool InsertOrUpdate(IEnumerable<TEntity> entities)
        {
            this.CheckDisposed();

            if (RepositoryHelper.IsEnumerableNullOrEmpty(entities))
            {
                return false;
            }

            List<TPrimaryKey> primaryKeys = new List<TPrimaryKey>();

            try
            {
                foreach (TEntity entity in entities)
                {
                    primaryKeys.Add(this._getPrimaryKeyFunc(entity));
                }
            }
            catch
            {
                return false;
            }

            lock (this._syncRoot)
            {
                int index = 0;

                foreach (TEntity entity in entities)
                {
                    try
                    {
                        this._repository[primaryKeys[index]] = RepositoryHelper.CloneDeep(entity);
                    }
                    catch (ArgumentNullException)
                    {
                        this._nullPrimaryKeyEntity.HasValue = true;
                        this._nullPrimaryKeyEntity.Value = RepositoryHelper.CloneDeep(entity);
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                        return false;
                    }

                    index++;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets entity from the repository by entity primary key.
        /// </summary>
        /// <param name="primaryKey">Entity primary key.</param>
        /// <returns>Entity instance.</returns>
        public TEntity Select(TPrimaryKey primaryKey)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                try
                {
                    if (this._repository.ContainsKey(primaryKey))
                    {
                        return RepositoryHelper.CloneDeep(this._repository[primaryKey]);
                    }
                }
                catch (ArgumentNullException)
                {
                    if (this._nullPrimaryKeyEntity.HasValue)
                    {
                        return RepositoryHelper.CloneDeep(this._nullPrimaryKeyEntity.Value);
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }

                return default(TEntity);
            }
        }

        /// <summary>
        /// Gets entities from the repository by entity primary key.
        /// </summary>
        /// <param name="primaryKeys">Entity primary keys.</param>
        /// <returns>List of entity instance.</returns>
        public IEnumerable<TEntity> Select(IEnumerable<TPrimaryKey> primaryKeys)
        {
            this.CheckDisposed();

            if (RepositoryHelper.IsEnumerableNullOrEmpty(primaryKeys))
            {
                return null;
            }

            lock (this._syncRoot)
            {
                List<TEntity> result = new List<TEntity>();

                foreach (TPrimaryKey primaryKey in primaryKeys)
                {
                    try
                    {
                        if (this._repository.ContainsKey(primaryKey))
                        {
                            result.Add(RepositoryHelper.CloneDeep(this._repository[primaryKey]));
                        }
                        else
                        {
                            result.Add(default(TEntity));
                        }
                    }
                    catch (ArgumentNullException)
                    {
                        if (this._nullPrimaryKeyEntity.HasValue)
                        {
                            result.Add(RepositoryHelper.CloneDeep(this._nullPrimaryKeyEntity.Value));
                        }
                        else
                        {
                            result.Add(default(TEntity));
                        }
                    }
                    catch
                    {
                        result.Add(default(TEntity));
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets all entities of the type from the repository.
        /// </summary>
        /// <returns>List of entity instance.</returns>
        public IEnumerable<TEntity> SelectAll()
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                List<TEntity> result = new List<TEntity>();

                if (this._nullPrimaryKeyEntity.HasValue)
                {
                    result.Add(RepositoryHelper.CloneDeep(this._nullPrimaryKeyEntity.Value));
                }

                foreach (var item in this._repository)
                {
                    try
                    {
                        result.Add(RepositoryHelper.CloneDeep(item.Value));
                    }
                    catch
                    {
                        result.Add(default(TEntity));
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Updates entity in the repository.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Update(TEntity entity)
        {
            this.CheckDisposed();

            if (entity == null)
            {
                return false;
            }

            try
            {
                TPrimaryKey primaryKey = this._getPrimaryKeyFunc(entity);

                lock (this._syncRoot)
                {
                    try
                    {
                        if (this._repository.ContainsKey(primaryKey))
                        {
                            this._repository[primaryKey] = RepositoryHelper.CloneDeep(entity);
                            return true;
                        }
                    }
                    catch (ArgumentNullException)
                    {
                        if (this._nullPrimaryKeyEntity.HasValue)
                        {
                            this._nullPrimaryKeyEntity.Value = RepositoryHelper.CloneDeep(entity);
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                    }

                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Updates entities in the repository.
        /// </summary>
        /// <param name="entities">Entity instances.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Update(IEnumerable<TEntity> entities)
        {
            this.CheckDisposed();

            if (RepositoryHelper.IsEnumerableNullOrEmpty(entities))
            {
                return false;
            }

            List<TPrimaryKey> primaryKeys = new List<TPrimaryKey>();

            try
            {
                foreach (TEntity entity in entities)
                {
                    primaryKeys.Add(this._getPrimaryKeyFunc(entity));
                }
            }
            catch
            {
                return false;
            }

            lock (this._syncRoot)
            {
                int index = 0;

                foreach (TEntity entity in entities)
                {
                    try
                    {
                        if (this._repository.ContainsKey(primaryKeys[index]))
                        {
                            this._repository[primaryKeys[index]] = RepositoryHelper.CloneDeep(entity);
                        }
                    }
                    catch (ArgumentNullException)
                    {
                        if (this._nullPrimaryKeyEntity.HasValue)
                        {
                            this._nullPrimaryKeyEntity.Value = RepositoryHelper.CloneDeep(entity);
                        }
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                        return false;
                    }

                    index++;
                }

                return true;
            }
        }

        /// <summary>
        /// Deletes entity in the repository by entity primary key.
        /// </summary>
        /// <param name="primaryKey">Entity primary key.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool DeleteByPrimaryKey(TPrimaryKey primaryKey)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                try
                {
                    if (this._repository.ContainsKey(primaryKey))
                    {
                        this._repository.Remove(primaryKey);
                        return true;
                    }
                }
                catch (ArgumentNullException)
                {
                    if (this._nullPrimaryKeyEntity.HasValue)
                    {
                        this._nullPrimaryKeyEntity.Clear();
                        return true;
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }

                return false;
            }
        }

        /// <summary>
        /// Deletes entities in the repository by entity primary key.
        /// </summary>
        /// <param name="primaryKeys">Entity primary keys.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool DeleteByPrimaryKey(IEnumerable<TPrimaryKey> primaryKeys)
        {
            this.CheckDisposed();

            if (RepositoryHelper.IsEnumerableNullOrEmpty(primaryKeys))
            {
                return false;
            }

            lock (this._syncRoot)
            {
                foreach (TPrimaryKey primaryKey in primaryKeys)
                {
                    try
                    {
                        if (this._repository.ContainsKey(primaryKey))
                        {
                            this._repository.Remove(primaryKey);
                        }
                    }
                    catch (ArgumentNullException)
                    {
                        if (this._nullPrimaryKeyEntity.HasValue)
                        {
                            this._nullPrimaryKeyEntity.Clear();
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Deletes entity in the repository.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Delete(TEntity entity)
        {
            this.CheckDisposed();

            try
            {
                TPrimaryKey primaryKey = this._getPrimaryKeyFunc(entity);

                lock (this._syncRoot)
                {
                    try
                    {
                        if (this._repository.ContainsKey(primaryKey))
                        {
                            this._repository.Remove(primaryKey);
                            return true;
                        }
                    }
                    catch (ArgumentNullException)
                    {
                        if (this._nullPrimaryKeyEntity.HasValue)
                        {
                            this._nullPrimaryKeyEntity.Clear();
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                    }

                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes entities in the repository.
        /// </summary>
        /// <param name="entities">Entity instances.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Delete(IEnumerable<TEntity> entities)
        {
            this.CheckDisposed();

            if (RepositoryHelper.IsEnumerableNullOrEmpty(entities))
            {
                return false;
            }

            List<TPrimaryKey> primaryKeys = new List<TPrimaryKey>();

            try
            {
                foreach (TEntity entity in entities)
                {
                    primaryKeys.Add(this._getPrimaryKeyFunc(entity));
                }
            }
            catch
            {
                return false;
            }

            lock (this._syncRoot)
            {
                foreach (TPrimaryKey primaryKey in primaryKeys)
                {
                    try
                    {
                        if (this._repository.ContainsKey(primaryKey))
                        {
                            this._repository.Remove(primaryKey);
                        }
                    }
                    catch (ArgumentNullException)
                    {
                        if (this._nullPrimaryKeyEntity.HasValue)
                        {
                            this._nullPrimaryKeyEntity.Clear();
                        }
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Deletes all entities in the repository.
        /// </summary>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool DeleteAll()
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                try
                {
                    this._repository.Clear();
                    this._nullPrimaryKeyEntity.Clear();

                    return true;
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                    return false;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified entity exists by entity primary key.
        /// </summary>
        /// <param name="primaryKey">Entity primary key.</param>
        /// <returns>true if the specified entity exists; otherwise, false.</returns>
        public bool ExistsByPrimaryKey(TPrimaryKey primaryKey)
        {
            this.CheckDisposed();

            lock (this._syncRoot)
            {
                try
                {
                    if (this._repository.ContainsKey(primaryKey))
                    {
                        return true;
                    }
                }
                catch (ArgumentNullException)
                {
                    if (this._nullPrimaryKeyEntity.HasValue)
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }

                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified entity exists.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <returns>true if the specified entity exists; otherwise, false.</returns>
        public bool Exists(TEntity entity)
        {
            this.CheckDisposed();

            try
            {
                TPrimaryKey primaryKey = this._getPrimaryKeyFunc(entity);

                lock (this._syncRoot)
                {
                    try
                    {
                        if (this._repository.ContainsKey(primaryKey))
                        {
                            return true;
                        }
                    }
                    catch (ArgumentNullException)
                    {
                        if (this._nullPrimaryKeyEntity.HasValue)
                        {
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                    }

                    return false;
                }
            }
            catch
            {
                return false;
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
                long count = RepositoryHelper.LongCount(this._repository);

                if (this._nullPrimaryKeyEntity.HasValue)
                {
                    return checked(count + 1);
                }
                else
                {
                    return count;
                }
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
                try
                {
                    this._repository.Clear();
                    this._nullPrimaryKeyEntity.Clear();

                    this._repository = new Dictionary<TPrimaryKey, TEntity>();
                    this._nullPrimaryKeyEntity = new NullablePrimaryKeyEntity<TEntity>();

                    return true;
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                    return false;
                }
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="MemoryRepository{TPrimaryKey, TEntity}" /> class.
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

                    this._nullPrimaryKeyEntity.Clear();
                    this._nullPrimaryKeyEntity = null;
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
