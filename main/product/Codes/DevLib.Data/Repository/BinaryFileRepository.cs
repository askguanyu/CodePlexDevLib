//-----------------------------------------------------------------------
// <copyright file="BinaryFileRepository.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Data.Repository
{
    using System.Collections.Generic;

    /// <summary>
    /// Binary file repository.
    /// </summary>
    /// <typeparam name="TPrimaryKey">The type of the entity primary key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class BinaryFileRepository<TPrimaryKey, TEntity> : FileBaseRepository<TPrimaryKey, TEntity>, IRepository<TPrimaryKey, TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileRepository{TPrimaryKey, TEntity}"/> class.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="getPrimaryKeyFunc">The get entity primary key function.</param>
        public BinaryFileRepository(string filename, GetPrimaryKeyFunc<TEntity, TPrimaryKey> getPrimaryKeyFunc)
            : base(filename, getPrimaryKeyFunc, file => RepositoryHelper.ReadBinary<List<TEntity>>(file), (file, obj) => RepositoryHelper.WriteBinary(file, obj))
        {
        }
    }
}
