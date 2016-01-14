//-----------------------------------------------------------------------
// <copyright file="DisposePattern.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DesignPatterns
{
    using System;

    /// <summary>
    /// Sample code for dispose pattern.
    /// </summary>
    public class DisposePattern : IDisposable
    {
        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisposePattern" /> class.
        /// </summary>
        public DisposePattern()
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="DisposePattern" /> class.
        /// </summary>
        ~DisposePattern()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="DisposePattern" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="DisposePattern" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="DisposePattern" /> class.
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
            }

            // free native resources
            ////if (nativeResource != IntPtr.Zero)
            ////{
            ////    Marshal.FreeHGlobal(nativeResource);
            ////    nativeResource = IntPtr.Zero;
            ////}
        }

        /// <summary>
        /// Checks whether this instance is disposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.DesignPatterns.DisposePattern");
            }
        }
    }
}
