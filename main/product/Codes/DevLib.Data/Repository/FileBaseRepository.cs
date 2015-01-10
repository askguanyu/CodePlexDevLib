//-----------------------------------------------------------------------
// <copyright file="FileBaseRepository.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Data.Repository
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// File base repository.
    /// </summary>
    /// <typeparam name="TPrimaryKey">The type of the entity primary key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class FileBaseRepository<TPrimaryKey, TEntity> : IRepository<TPrimaryKey, TEntity>, IDisposable
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
        /// Field _getPrimaryKeyFunc.
        /// </summary>
        private readonly GetPrimaryKeyFunc<TEntity, TPrimaryKey> _getPrimaryKeyFunc;

        /// <summary>
        /// Field _readFileFunc.
        /// </summary>
        private readonly ReadFileFunc<string, List<TEntity>> _readFileFunc;

        /// <summary>
        /// Field _writeFileAction.
        /// </summary>
        private readonly WriteFileAction<string, object> _writeFileAction;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBaseRepository{TPrimaryKey, TEntity}"/> class.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="getPrimaryKeyFunc">The get entity primary key function.</param>
        /// <param name="readFileFunc">The read file function.</param>
        /// <param name="writeFileAction">The write file action.</param>
        public FileBaseRepository(string filename, GetPrimaryKeyFunc<TEntity, TPrimaryKey> getPrimaryKeyFunc, ReadFileFunc<string, List<TEntity>> readFileFunc, WriteFileAction<string, object> writeFileAction)
        {
            this._file = Path.GetFullPath(filename);
            this._fileMutex = RepositoryHelper.CreateSharedMutex(RepositoryHelper.GetSharedFileMutexName(this._file));
            this._getPrimaryKeyFunc = getPrimaryKeyFunc;
            this._readFileFunc = readFileFunc;
            this._writeFileAction = writeFileAction;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="FileBaseRepository{TPrimaryKey, TEntity}" /> class.
        /// </summary>
        ~FileBaseRepository()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="FileBaseRepository{TPrimaryKey, TEntity}" /> class.
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
                List<TEntity> repository = null;

                try
                {
                    repository = this._readFileFunc(this._file);
                }
                catch (FileNotFoundException)
                {
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }

                if (repository == null)
                {
                    repository = new List<TEntity>(1);
                    repository.Add(entity);

                    this._writeFileAction(this._file, repository);
                    return true;
                }

                if (repository.Count < 1)
                {
                    repository.Add(entity);

                    this._writeFileAction(this._file, repository);
                    return true;
                }

                TPrimaryKey primaryKey = this._getPrimaryKeyFunc(entity);

                int index = repository.FindIndex(i => this._getPrimaryKeyFunc(i).Equals(primaryKey));

                if (index < 0)
                {
                    repository.Add(entity);

                    this._writeFileAction(this._file, repository);
                    return true;
                }

                return false;
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
                List<TEntity> repository = null;

                try
                {
                    repository = this._readFileFunc(this._file);
                }
                catch (FileNotFoundException)
                {
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }

                if (repository == null)
                {
                    repository = new List<TEntity>();
                }

                foreach (TEntity entity in entities)
                {
                    TPrimaryKey primaryKey = this._getPrimaryKeyFunc(entity);

                    int index = repository.FindIndex(i => this._getPrimaryKeyFunc(i).Equals(primaryKey));

                    if (index < 0)
                    {
                        repository.Add(entity);
                    }
                }

                this._writeFileAction(this._file, repository);
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
                List<TEntity> repository = null;

                try
                {
                    repository = this._readFileFunc(this._file);
                }
                catch (FileNotFoundException)
                {
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }

                if (repository == null)
                {
                    repository = new List<TEntity>(1);
                    repository.Add(entity);

                    this._writeFileAction(this._file, repository);
                    return true;
                }

                if (repository.Count < 1)
                {
                    repository.Add(entity);

                    this._writeFileAction(this._file, repository);
                    return true;
                }

                TPrimaryKey primaryKey = this._getPrimaryKeyFunc(entity);

                int index = repository.FindIndex(i => this._getPrimaryKeyFunc(i).Equals(primaryKey));

                if (index < 0)
                {
                    repository.Add(entity);
                }
                else
                {
                    repository[index] = entity;
                }

                this._writeFileAction(this._file, repository);
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
                List<TEntity> repository = null;

                try
                {
                    repository = this._readFileFunc(this._file);
                }
                catch (FileNotFoundException)
                {
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }

                if (repository == null)
                {
                    repository = new List<TEntity>();
                }

                foreach (TEntity entity in entities)
                {
                    TPrimaryKey primaryKey = this._getPrimaryKeyFunc(entity);

                    int index = repository.FindIndex(i => this._getPrimaryKeyFunc(i).Equals(primaryKey));

                    if (index < 0)
                    {
                        repository.Add(entity);
                    }
                    else
                    {
                        repository[index] = entity;
                    }
                }

                this._writeFileAction(this._file, repository);
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
        /// Gets entity from the repository by entity primary key.
        /// </summary>
        /// <param name="primaryKey">Entity primary key.</param>
        /// <returns>Entity instance.</returns>
        public TEntity Select(TPrimaryKey primaryKey)
        {
            this.CheckDisposed();

            if (this._fileMutex == null)
            {
                return default(TEntity);
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
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }

                if (repository == null || repository.Count < 1)
                {
                    return default(TEntity);
                }

                int index = repository.FindIndex(i => this._getPrimaryKeyFunc(i).Equals(primaryKey));

                if (index >= 0)
                {
                    return repository[index];
                }
                else
                {
                    return default(TEntity);
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                return default(TEntity);
            }
            finally
            {
                this._fileMutex.ReleaseMutex();
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

            if (this._fileMutex == null)
            {
                return null;
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
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }

                if (repository == null || repository.Count < 1)
                {
                    return null;
                }

                List<TEntity> result = new List<TEntity>();

                foreach (TPrimaryKey primaryKey in primaryKeys)
                {
                    try
                    {
                        int index = repository.FindIndex(i => this._getPrimaryKeyFunc(i).Equals(primaryKey));

                        if (index >= 0)
                        {
                            result.Add(repository[index]);
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
            catch (Exception e)
            {
                InternalLogger.Log(e);

                return null;
            }
            finally
            {
                this._fileMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Gets all entities of the type from the repository.
        /// </summary>
        /// <returns>List of entity instance.</returns>
        public IEnumerable<TEntity> SelectAll()
        {
            this.CheckDisposed();

            if (this._fileMutex == null)
            {
                return null;
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
                return this._readFileFunc(this._file) ?? new List<TEntity>(0);
            }
            catch (FileNotFoundException)
            {
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
            }
            finally
            {
                this._fileMutex.ReleaseMutex();
            }

            return null;
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
                List<TEntity> repository = null;

                try
                {
                    repository = this._readFileFunc(this._file);
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);

                    return false;
                }

                if (repository == null || repository.Count < 1)
                {
                    return false;
                }

                TPrimaryKey primaryKey = this._getPrimaryKeyFunc(entity);

                int index = repository.FindIndex(i => this._getPrimaryKeyFunc(i).Equals(primaryKey));

                if (index >= 0)
                {
                    repository[index] = entity;

                    this._writeFileAction(this._file, repository);
                    return true;
                }

                return false;
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
                List<TEntity> repository = null;

                try
                {
                    repository = this._readFileFunc(this._file);
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);

                    return false;
                }

                if (repository == null || repository.Count < 1)
                {
                    return false;
                }

                foreach (TEntity entity in entities)
                {
                    TPrimaryKey primaryKey = this._getPrimaryKeyFunc(entity);

                    int index = repository.FindIndex(i => this._getPrimaryKeyFunc(i).Equals(primaryKey));

                    if (index >= 0)
                    {
                        repository[index] = entity;
                    }
                }

                this._writeFileAction(this._file, repository);
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
        /// Deletes entity in the repository by entity primary key.
        /// </summary>
        /// <param name="primaryKey">Entity primary key.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool DeleteByPrimaryKey(TPrimaryKey primaryKey)
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
                List<TEntity> repository = null;

                try
                {
                    repository = this._readFileFunc(this._file);
                }
                catch (FileNotFoundException)
                {
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }

                if (repository == null || repository.Count < 1)
                {
                    return true;
                }

                int index = repository.FindIndex(i => this._getPrimaryKeyFunc(i).Equals(primaryKey));

                if (index >= 0)
                {
                    repository.RemoveAt(index);

                    this._writeFileAction(this._file, repository);
                }

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
                List<TEntity> repository = null;

                try
                {
                    repository = this._readFileFunc(this._file);
                }
                catch (FileNotFoundException)
                {
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }

                if (repository == null || repository.Count < 1)
                {
                    return true;
                }

                foreach (TPrimaryKey primaryKey in primaryKeys)
                {
                    int index = repository.FindIndex(i => this._getPrimaryKeyFunc(i).Equals(primaryKey));

                    if (index >= 0)
                    {
                        repository.RemoveAt(index);
                    }
                }

                this._writeFileAction(this._file, repository);
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
        /// Deletes entity in the repository.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Delete(TEntity entity)
        {
            this.CheckDisposed();

            if (entity == null)
            {
                return false;
            }

            return this.DeleteByPrimaryKey(this._getPrimaryKeyFunc(entity));
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
                List<TEntity> repository = null;

                try
                {
                    repository = this._readFileFunc(this._file);
                }
                catch (FileNotFoundException)
                {
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }

                if (repository == null || repository.Count < 1)
                {
                    return true;
                }

                foreach (TEntity entity in entities)
                {
                    TPrimaryKey primaryKey = this._getPrimaryKeyFunc(entity);

                    int index = repository.FindIndex(i => this._getPrimaryKeyFunc(i).Equals(primaryKey));

                    if (index >= 0)
                    {
                        repository.RemoveAt(index);
                    }
                }

                this._writeFileAction(this._file, repository);
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
        /// Deletes all entities in the repository.
        /// </summary>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool DeleteAll()
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
                this._writeFileAction(this._file, new List<TEntity>(0));
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
        /// Determines whether the specified entity exists by entity primary key.
        /// </summary>
        /// <param name="primaryKey">Entity primary key.</param>
        /// <returns>true if the specified entity exists; otherwise, false.</returns>
        public bool ExistsByPrimaryKey(TPrimaryKey primaryKey)
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
                List<TEntity> repository = null;

                try
                {
                    repository = this._readFileFunc(this._file);
                }
                catch (FileNotFoundException)
                {
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }

                if (repository == null || repository.Count < 1)
                {
                    return false;
                }

                return repository.Exists(i => this._getPrimaryKeyFunc(i).Equals(primaryKey));
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
        /// Determines whether the specified entity exists.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <returns>true if the specified entity exists; otherwise, false.</returns>
        public bool Exists(TEntity entity)
        {
            this.CheckDisposed();

            if (entity == null)
            {
                return false;
            }

            return this.ExistsByPrimaryKey(this._getPrimaryKeyFunc(entity));
        }

        /// <summary>
        /// Gets the number of entities contained in the repository.
        /// </summary>
        /// <returns>The number of entities contained in the repository.</returns>
        public long Count()
        {
            this.CheckDisposed();

            if (this._fileMutex == null)
            {
                return -1;
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
                return RepositoryHelper.LongCount(this._readFileFunc(this._file));
            }
            catch (FileNotFoundException)
            {
                return 0;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                return -1;
            }
            finally
            {
                this._fileMutex.ReleaseMutex();
            }
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
        /// Releases all resources used by the current instance of the <see cref="FileBaseRepository{TPrimaryKey, TEntity}" /> class.
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
    }
}
