//-----------------------------------------------------------------------
// <copyright file="IRepository.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Data.Repository
{
    using System.Collections.Generic;

    /// <summary>
    /// Generic repository interface for reading and writing domain entities to a storage.
    /// </summary>
    /// <typeparam name="TPrimaryKey">Type of the entity primary key.</typeparam>
    /// <typeparam name="TEntity">Type of the entity.</typeparam>
    public interface IRepository<TPrimaryKey, TEntity>
    {
        /// <summary>
        /// Inserts entity to the repository.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        bool Insert(TEntity entity);

        /// <summary>
        /// Inserts entities to the repository.
        /// </summary>
        /// <param name="entities">Entity instances.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        bool Insert(IEnumerable<TEntity> entities);

        /// <summary>
        /// Inserts entity to the repository if the primary key does not already exist, or update entity in the repository if the primary key already exists.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        bool InsertOrUpdate(TEntity entity);

        /// <summary>
        /// Inserts entities to the repository if the primary key does not already exist, or update entities in the repository if the primary key already exists.
        /// </summary>
        /// <param name="entities">Entity instances.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        bool InsertOrUpdate(IEnumerable<TEntity> entities);

        /// <summary>
        /// Gets entity from the repository by entity primary key.
        /// </summary>
        /// <param name="primaryKey">Entity primary key.</param>
        /// <returns>Entity instance.</returns>
        TEntity Select(TPrimaryKey primaryKey);

        /// <summary>
        /// Gets entities from the repository by entity primary key.
        /// </summary>
        /// <param name="primaryKeys">Entity primary keys.</param>
        /// <returns>List of entity instance.</returns>
        IEnumerable<TEntity> Select(IEnumerable<TPrimaryKey> primaryKeys);

        /// <summary>
        /// Gets all entities of the type from the repository.
        /// </summary>
        /// <returns>List of entity instance.</returns>
        IEnumerable<TEntity> SelectAll();

        /// <summary>
        /// Updates entity in the repository.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        bool Update(TEntity entity);

        /// <summary>
        /// Updates entities in the repository.
        /// </summary>
        /// <param name="entities">Entity instances.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        bool Update(IEnumerable<TEntity> entities);

        /// <summary>
        /// Deletes entity in the repository by entity primary key.
        /// </summary>
        /// <param name="primaryKey">Entity primary key.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        bool DeleteByPrimaryKey(TPrimaryKey primaryKey);

        /// <summary>
        /// Deletes entities in the repository by entity primary key.
        /// </summary>
        /// <param name="primaryKeys">Entity primary keys.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        bool DeleteByPrimaryKey(IEnumerable<TPrimaryKey> primaryKeys);

        /// <summary>
        /// Deletes entity in the repository.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        bool Delete(TEntity entity);

        /// <summary>
        /// Deletes entities in the repository.
        /// </summary>
        /// <param name="entities">Entity instances.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        bool Delete(IEnumerable<TEntity> entities);

        /// <summary>
        /// Deletes all entities in the repository.
        /// </summary>
        /// <returns>true if succeeded; otherwise, false.</returns>
        bool DeleteAll();

        /// <summary>
        /// Determines whether the specified entity exists by entity primary key.
        /// </summary>
        /// <param name="primaryKey">Entity primary key.</param>
        /// <returns>true if the specified entity exists; otherwise, false.</returns>
        bool ExistsByPrimaryKey(TPrimaryKey primaryKey);

        /// <summary>
        /// Determines whether the specified entity exists.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <returns>true if the specified entity exists; otherwise, false.</returns>
        bool Exists(TEntity entity);

        /// <summary>
        /// Gets the number of entities contained in the repository.
        /// </summary>
        /// <returns>The number of entities contained in the repository.</returns>
        long Count();

        /// <summary>
        /// Destroys the repository.
        /// </summary>
        /// <returns>true if succeeded; otherwise, false.</returns>
        bool Drop();
    }
}
