//-----------------------------------------------------------------------
// <copyright file="ObjectPoolRegistration.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    using System;

    /// <summary>
    /// Represents a pooled object registration.
    /// </summary>
    /// <typeparam name="T">The type of element in the object pool.</typeparam>
    public class ObjectPoolRegistration<T> : IDisposable
    {
        /// <summary>
        /// Field _releaseAction.
        /// </summary>
        private readonly Action<T> _releaseAction;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPoolRegistration{T}" /> class.
        /// </summary>
        /// <param name="value">The object to be represented.</param>
        /// <param name="releaseAction">The action called before return the object to the pool.</param>
        internal ObjectPoolRegistration(T value, Action<T> releaseAction)
        {
            this._releaseAction = releaseAction;
            this.Value = value;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ObjectPoolRegistration{T}"/> class from being created.
        /// </summary>
        private ObjectPoolRegistration()
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ObjectPoolRegistration{T}" /> class.
        /// </summary>
        ~ObjectPoolRegistration()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the object to be returned to the pool when the registration is disposed.
        /// </summary>
        public T Value
        {
            get;
            private set;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="ObjectPoolRegistration{T}"/> to T.
        /// </summary>
        /// <param name="source">The object pool registration.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator T(ObjectPoolRegistration<T> source)
        {
            return source.Value;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ObjectPoolRegistration{T}" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ObjectPoolRegistration{T}" /> class.
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

                if (this._releaseAction != null)
                {
                    this._releaseAction(this.Value);
                }
            }

            // free native resources
            ////if (nativeResource != IntPtr.Zero)
            ////{
            ////    Marshal.FreeHGlobal(nativeResource);
            ////    nativeResource = IntPtr.Zero;
            ////}
        }
    }
}
