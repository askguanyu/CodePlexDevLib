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
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class BinaryFileRepository<TEntity> : FileBaseRepository<TEntity>, IRepository<TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileRepository{TEntity}" /> class.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public BinaryFileRepository(string filename)
            : base(filename, file => RepositoryHelper.ReadBinary<List<TEntity>>(file), (file, obj) => RepositoryHelper.WriteBinary(file, obj))
        {
        }
    }
}
