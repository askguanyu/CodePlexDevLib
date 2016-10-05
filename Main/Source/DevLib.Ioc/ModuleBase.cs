//-----------------------------------------------------------------------
// <copyright file="ModuleBase.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Ioc
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Register module base class.
    /// </summary>
    public abstract class ModuleBase : IDisposable
    {
        /// <summary>
        /// Field ModuleContainer.
        /// </summary>
        private static readonly IocContainer ModuleContainer = new IocContainer(new Guid().ToString());

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleBase"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Reviewed.")]
        public ModuleBase()
        {
            ServiceLocator.SetLocatorProvider(() => ModuleContainer);

            this.RegisterModule(ModuleContainer);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ModuleBase" /> class.
        /// </summary>
        ~ModuleBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Registers modules.
        /// </summary>
        /// <param name="container">The default container.</param>
        public virtual void RegisterModule(IocContainer container)
        {
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ModuleBase" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="ModuleBase" /> class.
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

                if (ModuleContainer != null)
                {
                    ModuleContainer.Dispose();
                }
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
                throw new ObjectDisposedException("DevLib.Ioc.ModuleBase");
            }
        }
    }
}
