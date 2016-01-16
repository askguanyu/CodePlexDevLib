//-----------------------------------------------------------------------
// <copyright file="TerminalServer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    using System;
    using System.Collections.Generic;
    using DevLib.TerminalServices.NativeAPI;

    /// <summary>
    /// Connection to a terminal server.
    /// </summary>
    /// <remarks>
    /// <see cref="Open" /> must be called before any operations can be performed on a remote terminal server.
    /// </remarks>
    public class TerminalServer : IDisposable
    {
        /// <summary>
        /// Field _terminalServerHandle.
        /// </summary>
        private readonly ITerminalServerHandle _terminalServerHandle;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalServer" /> class.
        /// </summary>
        /// <param name="handle">ITerminalServerHandle instance.</param>
        internal TerminalServer(ITerminalServerHandle handle)
        {
            this._terminalServerHandle = handle;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="TerminalServer" /> class.
        /// </summary>
        ~TerminalServer()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the name of the terminal server.
        /// </summary>
        /// <remarks>
        /// It is not necessary to have a connection to the server open before retrieving this value.
        /// </remarks>
        public string ServerName
        {
            get
            {
                return this._terminalServerHandle.ServerName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this server is the local terminal server.
        /// </summary>
        public bool IsLocal
        {
            get
            {
                return this._terminalServerHandle.IsLocal;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the server is currently open.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return this._terminalServerHandle.IsOpen;
            }
        }

        /// <summary>
        /// Gets underlying connection to the terminal server.
        /// </summary>
        internal ITerminalServerHandle Handle
        {
            get
            {
                return this._terminalServerHandle;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="TerminalServer" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets list of the sessions on the terminal server.
        /// </summary>
        /// <returns>A list of sessions.</returns>
        public List<TerminalServicesSession> GetSessions()
        {
            this.CheckDisposed();

            List<TerminalServicesSession> results = new List<TerminalServicesSession>();

            List<WTS_SESSION_INFO> sessionInfos = NativeMethodsHelper.GetSessionInfos(this.Handle);

            foreach (WTS_SESSION_INFO sessionInfo in sessionInfos)
            {
                results.Add(new TerminalServicesSession(this, sessionInfo));
            }

            return results;
        }

        /// <summary>
        /// Retrieves information about a particular session on the server.
        /// </summary>
        /// <param name="sessionId">The Id of the session.</param>
        /// <returns>Information about the requested session.</returns>
        public TerminalServicesSession GetSession(int sessionId)
        {
            this.CheckDisposed();

            return new TerminalServicesSession(this, sessionId);
        }

        /// <summary>
        /// Opens a connection to the server.
        /// </summary>
        /// <remarks>
        /// Call this before attempting operations that access information or perform operations on a remote server. You can call this method for the local terminal server, but it is not necessary.
        /// </remarks>
        public void Open()
        {
            this.CheckDisposed();

            this._terminalServerHandle.Open();
        }

        /// <summary>
        /// Closes the connection to the server.
        /// </summary>
        public void Close()
        {
            this._terminalServerHandle.Close();
        }

        /// <summary>
        /// Retrieves a list of processes running on the terminal server.
        /// </summary>
        /// <returns>A list of processes.</returns>
        public List<TerminalServicesProcess> GetProcesses()
        {
            this.CheckDisposed();

            List<TerminalServicesProcess> processes = new List<TerminalServicesProcess>();

            NativeMethodsHelper.ForEachProcessInfo(
                this.Handle,
                delegate(WTS_PROCESS_INFO processInfo) { processes.Add(new TerminalServicesProcess(this, processInfo)); });

            return processes;
        }

        /// <summary>
        /// Retrieves information about a particular process running on the server.
        /// </summary>
        /// <param name="processId">The Id of the process.</param>
        /// <returns>Information about the requested process.</returns>
        public TerminalServicesProcess GetProcess(int processId)
        {
            this.CheckDisposed();

            foreach (TerminalServicesProcess process in this.GetProcesses())
            {
                if (process.ProcessId == processId)
                {
                    return process;
                }
            }

            throw new InvalidOperationException("Process ID " + processId + " not found");
        }

        /// <summary>
        /// Shuts down the terminal server.
        /// </summary>
        /// <param name="type">Type of shutdown requested.</param>
        public void Shutdown(ShutdownType type)
        {
            this.CheckDisposed();

            NativeMethodsHelper.ShutdownSystem(this.Handle, (int)type);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="TerminalServer" /> class.
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

            this._terminalServerHandle.Dispose();
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.TerminalServer.TerminalServer");
            }
        }
    }
}
