//-----------------------------------------------------------------------
// <copyright file="NullablePrimaryKeyEntity.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Data.Repository
{
    /// <summary>
    /// Nullable primary key entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    internal class NullablePrimaryKeyEntity<TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullablePrimaryKeyEntity{TEntity}"/> class.
        /// </summary>
        public NullablePrimaryKeyEntity()
        {
            this.HasValue = false;
            this.Value = default(TEntity);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has value.
        /// </summary>
        public bool HasValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public TEntity Value
        {
            get;
            set;
        }

        /// <summary>
        /// Clears this instance value.
        /// </summary>
        public void Clear()
        {
            this.HasValue = false;
            this.Value = default(TEntity);
        }
    }
}
