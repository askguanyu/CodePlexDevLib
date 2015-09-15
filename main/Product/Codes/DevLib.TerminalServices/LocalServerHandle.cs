//-----------------------------------------------------------------------
// <copyright file="LocalServerHandle.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    using System;
    using DevLib.TerminalServices.NativeAPI;

    /// <summary>
    /// Connection to the local terminal server.
    /// </summary>
    internal class LocalServerHandle : ITerminalServerHandle
    {
        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalServerHandle" /> class.
        /// </summary>
        public LocalServerHandle()
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="LocalServerHandle" /> class.
        /// </summary>
        ~LocalServerHandle()
        {
            this.Dispose(false);
        }

        /// <summary>
        ///  Gets the underlying terminal server handle provided by Windows in a call to WTSOpenServer.
        /// </summary>
        public IntPtr Handle
        {
            get
            {
                return NativeMethods.LocalServerHandle;
            }
        }

        /// <summary>
        /// Gets the name of the terminal server for this connection.
        /// </summary>
        public string ServerName
        {
            get
            {
                return Environment.MachineName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the connection to the server is currently open.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the handle is for the local server.
        /// </summary>
        public bool IsLocal
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="LocalServerHandle" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Opens the terminal server handle.
        /// </summary>
        public void Open()
        {
        }

        /// <summary>
        /// Closes the terminal server handle.
        /// </summary>
        public void Close()
        {
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="LocalServerHandle" /> class.
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
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.TerminalServices.LocalServerHandle");
            }
        }
    }
}
