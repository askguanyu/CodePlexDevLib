//-----------------------------------------------------------------------
// <copyright file="RunAndBlock.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Threading
{
    using System;
    using System.Threading;

    /// <summary>
    /// Runs the code and blocks the current thread while the code remains running.
    /// </summary>
    /// <example>E.g.
    /// <code>
    /// using (var runAndBlock = new RunAndBlock())
    /// {
    ///     // your code
    /// }
    /// </code>
    /// </example>
    public class RunAndBlock : IDisposable
    {
        /// <summary>
        /// Field _signal.
        /// </summary>
        private readonly ManualResetEvent _signal = new ManualResetEvent(false);

        /// <summary>
        /// Field _timeout.
        /// </summary>
        private readonly int _timeout;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="RunAndBlock"/> class.
        /// </summary>
        public RunAndBlock()
        {
            this._timeout = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunAndBlock"/> class.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or System.Threading.Timeout.Infinite (-1) to wait indefinitely.</param>
        public RunAndBlock(int millisecondsTimeout)
        {
            this._timeout = millisecondsTimeout;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="RunAndBlock" /> class.
        /// </summary>
        ~RunAndBlock()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Exits the code block.
        /// </summary>
        public virtual void Exit()
        {
            this._signal.Set();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="RunAndBlock" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="RunAndBlock" /> class.
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
                
                this._signal.WaitOne(this._timeout);
                this._signal.Close();
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
                throw new ObjectDisposedException("DevLib.Threading.RunAndBlock");
            }
        }
    }
}
