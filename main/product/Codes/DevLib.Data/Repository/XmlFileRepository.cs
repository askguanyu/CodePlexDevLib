//-----------------------------------------------------------------------
// <copyright file="XmlFileRepository.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Data.Repository
{
    using System.Collections.Generic;

    /// <summary>
    /// Xml file repository.
    /// </summary>
    /// <typeparam name="TPrimaryKey">The type of the entity primary key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class XmlFileRepository<TPrimaryKey, TEntity> : FileBaseRepository<TPrimaryKey, TEntity>, IRepository<TPrimaryKey, TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlFileRepository{TPrimaryKey, TEntity}"/> class.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="getPrimaryKeyFunc">The get entity primary key function.</param>
        public XmlFileRepository(string filename, GetPrimaryKeyFunc<TEntity, TPrimaryKey> getPrimaryKeyFunc)
            : base(filename, getPrimaryKeyFunc, file => RepositoryHelper.ReadXml<List<TEntity>>(file), (file, obj) => RepositoryHelper.WriteXml(file, obj))
        {
        }
    }
}
