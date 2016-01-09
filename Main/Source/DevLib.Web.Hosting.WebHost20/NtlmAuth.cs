//-----------------------------------------------------------------------
// <copyright file="NtlmAuth.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Hosting.WebHost20
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;
    using DevLib.Web.Hosting.WebHost20.NativeAPI;

    /// <summary>
    /// NTLM Authenticate Class.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal sealed class NtlmAuth : IDisposable
    {
        private SecBufferDesc _inputBufferDesc;
        private SecBuffer _inputBuffer;
        private SecBufferDesc _outputBufferDesc;
        private SecBuffer _outputBuffer;
        private SecHandle _credentialsHandle;
        private bool _credentialsHandleAcquired;
        private SecHandle _securityContext;
        private bool _securityContextAcquired;
        private uint _securityContextAttributes;
        private long _timestamp;
        private bool _completed;
        private string _blob;
        private SecurityIdentifier _sid;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="NtlmAuth"/> class.
        /// </summary>
        public NtlmAuth()
        {
            if (NativeMethods.AcquireCredentialsHandle((string)null, "NTLM", 1U, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref this._credentialsHandle, ref this._timestamp) != 0)
            {
                throw new InvalidOperationException();
            }

            this._credentialsHandleAcquired = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="NtlmAuth" /> class.
        /// </summary>
        ~NtlmAuth()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets Blob string.
        /// </summary>
        public string Blob
        {
            get
            {
                return this._blob;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="NtlmAuth"/> is completed.
        /// </summary>
        public bool Completed
        {
            get
            {
                return this._completed;
            }
        }

        /// <summary>
        /// Gets the SID.
        /// </summary>
        public SecurityIdentifier SID
        {
            get
            {
                return this._sid;
            }
        }

        /// <summary>
        /// Authenticates the specified Blob string.
        /// </summary>
        /// <param name="blobString">The Blob string.</param>
        /// <returns>true if authenticate succeeded; otherwise, false.</returns>
        public unsafe bool Authenticate(string blobString)
        {
            this.CheckDisposed();

            this._blob = null;
            byte[] numArray = Convert.FromBase64String(blobString);
            byte[] inArray = new byte[16384];

            fixed (SecHandle* secHandlePtr = &this._securityContext)
            fixed (SecBuffer* secBufferPtr1 = &this._inputBuffer)
            fixed (SecBuffer* secBufferPtr2 = &this._outputBuffer)
            fixed (byte* numPtr1 = &numArray[0])
            fixed (byte* numPtr2 = &inArray[0])
            {
                IntPtr phContext = IntPtr.Zero;

                if (this._securityContextAcquired)
                {
                    phContext = (IntPtr)((void*)secHandlePtr);
                }

                this._inputBufferDesc.ulVersion = 0U;
                this._inputBufferDesc.cBuffers = 1U;
                this._inputBufferDesc.pBuffers = (IntPtr)((void*)secBufferPtr1);
                this._inputBuffer.cbBuffer = (uint)numArray.Length;
                this._inputBuffer.BufferType = 2U;
                this._inputBuffer.pvBuffer = (IntPtr)((void*)numPtr1);
                this._outputBufferDesc.ulVersion = 0U;
                this._outputBufferDesc.cBuffers = 1U;
                this._outputBufferDesc.pBuffers = (IntPtr)((void*)secBufferPtr2);
                this._outputBuffer.cbBuffer = (uint)inArray.Length;
                this._outputBuffer.BufferType = 2U;
                this._outputBuffer.pvBuffer = (IntPtr)((void*)numPtr2);

                switch (NativeMethods.AcceptSecurityContext(ref this._credentialsHandle, phContext, ref this._inputBufferDesc, 20U, 0U, ref this._securityContext, ref this._outputBufferDesc, ref this._securityContextAttributes, ref this._timestamp))
                {
                    case 590610:
                        this._securityContextAcquired = true;
                        this._blob = Convert.ToBase64String(inArray, 0, (int)this._outputBuffer.cbBuffer);

                        break;
                    case 0:
                        IntPtr phToken = IntPtr.Zero;

                        if (NativeMethods.QuerySecurityContextToken(ref this._securityContext, ref phToken) != 0)
                        {
                            return false;
                        }

                        try
                        {
                            using (WindowsIdentity windowsIdentity = new WindowsIdentity(phToken))
                            {
                                this._sid = windowsIdentity.User;
                            }
                        }
                        finally
                        {
                            NativeMethods.CloseHandle(phToken);
                        }

                        this._completed = true;

                        break;
                    default:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="NtlmAuth" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="NtlmAuth" /> class.
        /// protected virtual for non-sealed class; private for sealed class.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
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

            if (this._securityContextAcquired)
            {
                NativeMethods.DeleteSecurityContext(ref this._securityContext);
            }

            if (this._credentialsHandleAcquired)
            {
                NativeMethods.FreeCredentialsHandle(ref this._credentialsHandle);
            }
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.Web.Hosting.WebHost20.NtlmAuth");
            }
        }
    }
}
