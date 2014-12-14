//-----------------------------------------------------------------------
// <copyright file="GroupLazyLoadedProperty.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    using System;

    /// <summary>
    /// Delegate GroupPropertyLoader.
    /// </summary>
    internal delegate void GroupPropertyLoader();

    /// <summary>
    /// Class GroupLazyLoadedProperty.
    /// </summary>
    /// <typeparam name="T">Type of property.</typeparam>
    internal class GroupLazyLoadedProperty<T>
    {
        /// <summary>
        /// Field _loader.
        /// </summary>
        private readonly GroupPropertyLoader _loader;

        /// <summary>
        /// Field _isLoaded.
        /// </summary>
        private bool _isLoaded;

        /// <summary>
        /// Field _value.
        /// </summary>
        private T _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupLazyLoadedProperty{T}" /> class.
        /// </summary>
        /// <param name="loader">GroupPropertyLoader delegate.</param>
        public GroupLazyLoadedProperty(GroupPropertyLoader loader)
        {
            this._loader = loader;
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public T Value
        {
            get
            {
                if (!this._isLoaded)
                {
                    this._loader();

                    if (!this._isLoaded)
                    {
                        throw new InvalidOperationException("Property value should have been set by now");
                    }
                }

                return this._value;
            }

            set
            {
                this._value = value;
                this._isLoaded = true;
            }
        }
    }
}
