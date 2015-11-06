//-----------------------------------------------------------------------
// <copyright file="IocRegistration.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Ioc
{
    /// <summary>
    /// Represents the registration of a resolution.
    /// </summary>
    public class IocRegistration
    {
        /// <summary>
        /// Gets or sets a shared instance.
        /// </summary>
        public object Instance
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether a shared instance has been created.
        /// </summary>
        public bool HasInstance
        {
            get
            {
                return this.Instance != null;
            }
        }

        /// <summary>
        /// Gets or sets delegate method to create a new instance.
        /// </summary>
        public BuilderFunc Builder
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether a delegate method of creation a new instance exists.
        /// </summary>
        public bool HasBuilder
        {
            get
            {
                return this.Builder != null;
            }
        }
    }
}
