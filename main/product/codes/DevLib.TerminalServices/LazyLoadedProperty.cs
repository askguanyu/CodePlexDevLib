//-----------------------------------------------------------------------
// <copyright file="LazyLoadedProperty.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    /// <summary>
    /// Delegate PropertyLoader.
    /// </summary>
    /// <typeparam name="T">Type of property.</typeparam>
    /// <returns>Property value.</returns>
    internal delegate T PropertyLoader<T>();

    /// <summary>
    /// Class LazyLoadedProperty.
    /// </summary>
    /// <typeparam name="T">Type of property.</typeparam>
    internal class LazyLoadedProperty<T>
    {
        /// <summary>
        /// Field _loader.
        /// </summary>
        private readonly PropertyLoader<T> _loader;

        /// <summary>
        /// Field _isLoaded.
        /// </summary>
        private bool _isLoaded;

        /// <summary>
        /// Field _value.
        /// </summary>
        private T _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyLoadedProperty{T}" /> class.
        /// </summary>
        /// <param name="loader">PropertyLoader{T} delegate.</param>
        public LazyLoadedProperty(PropertyLoader<T> loader)
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
                    this._value = this._loader();
                    this._isLoaded = true;
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