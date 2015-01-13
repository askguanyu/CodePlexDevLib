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
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class XmlFileRepository<TEntity> : FileBaseRepository<TEntity>, IRepository<TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlFileRepository{TEntity}" /> class.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public XmlFileRepository(string filename)
            : base(filename, file => RepositoryHelper.ReadXml<List<TEntity>>(file), (file, obj) => RepositoryHelper.WriteXml(file, obj))
        {
        }
    }
}
