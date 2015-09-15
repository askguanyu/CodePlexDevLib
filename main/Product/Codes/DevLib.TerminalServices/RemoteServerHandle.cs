//-----------------------------------------------------------------------
// <copyright file="RemoteServerHandle.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    using System;
    using System.ComponentModel;
    using DevLib.TerminalServices.NativeAPI;

    /// <summary>
    /// Connection to a remote terminal server.
    /// </summary>
    internal class RemoteServerHandle : ITerminalServerHandle
    {
        /// <summary>
        /// Field _serverName.
        /// </summary>
        private readonly string _serverName;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Field _serverHandle.
        /// </summary>
        private IntPtr _serverHandle;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteServerHandle" /> class.
        /// </summary>
        /// <param name="serverName">Server name.</param>
        public RemoteServerHandle(string serverName)
        {
            if (serverName == null)
            {
                throw new ArgumentNullException("serverName");
            }

            this._serverName = serverName;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="RemoteServerHandle" /> class.
        /// </summary>
        ~RemoteServerHandle()
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
                if (!this.IsOpen)
                {
                    throw new InvalidOperationException();
                }

                return this._serverHandle;
            }
        }

        /// <summary>
        /// Gets the name of the terminal server for this connection.
        /// </summary>
        public string ServerName
        {
            get
            {
                return this._serverName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the connection to the server is currently open.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return this._serverHandle != IntPtr.Zero;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the handle is for the local server.
        /// </summary>
        public bool IsLocal
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="RemoteServerHandle" /> class.
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
            this.CheckDisposed();

            if (this._serverHandle != IntPtr.Zero)
            {
                return;
            }

            this._serverHandle = NativeMethods.WTSOpenServer(this._serverName);

            if (this._serverHandle == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Closes the terminal server handle.
        /// </summary>
        public void Close()
        {
            if (this._serverHandle == IntPtr.Zero)
            {
                return;
            }

            NativeMethods.WTSCloseServer(this._serverHandle);

            this._serverHandle = IntPtr.Zero;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="RemoteServerHandle" /> class.
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

            this.Close();
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.TerminalServices.RemoteServerHandle");
            }
        }
    }
}
