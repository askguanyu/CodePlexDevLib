//-----------------------------------------------------------------------
// <copyright file="IocRegistration.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Ioc
{
    using System;

    /// <summary>
    /// Registration context.
    /// </summary>
    public class IocRegistration : IDisposable
    {
        /// <summary>
        /// Field _releaseAction.
        /// </summary>
        private readonly Action<IocRegistration> _releaseAction;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="IocRegistration" /> class.
        /// </summary>
        /// <param name="releaseAction">The release action.</param>
        /// <param name="name">The name of the registration.</param>
        public IocRegistration(Action<IocRegistration> releaseAction, string name)
        {
            this._releaseAction = releaseAction;
            this.Name = name;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="IocRegistration" /> class.
        /// </summary>
        ~IocRegistration()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the name of this registration.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="IocRegistration" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="IocRegistration" /> class.
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

                this._releaseAction(this);
            }

            // free native resources
            ////if (nativeResource != IntPtr.Zero)
            ////{
            ////    Marshal.FreeHGlobal(nativeResource);
            ////    nativeResource = IntPtr.Zero;
            ////}
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.Ioc.IocRegistration");
            }
        }
    }
}
