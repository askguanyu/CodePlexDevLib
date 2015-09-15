//-----------------------------------------------------------------------
// <copyright file="TerminalServicesProcess.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    using System;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.Security.Principal;
    using DevLib.TerminalServices.NativeAPI;

    /// <summary>
    /// A process running on a terminal server.
    /// </summary>
    public class TerminalServicesProcess
    {
        /// <summary>
        /// Field _processId.
        /// </summary>
        private readonly int _processId;

        /// <summary>
        /// Field _processName.
        /// </summary>
        private readonly string _processName;

        /// <summary>
        /// Field _securityIdentifier.
        /// </summary>
        private readonly SecurityIdentifier _securityIdentifier;

        /// <summary>
        /// Field _server.
        /// </summary>
        private readonly TerminalServer _server;

        /// <summary>
        /// Field _sessionId.
        /// </summary>
        private readonly int _sessionId;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalServicesProcess" /> class.
        /// </summary>
        /// <param name="server">TerminalServer instance.</param>
        /// <param name="processInfo">WTS_PROCESS_INFO instance.</param>
        internal TerminalServicesProcess(TerminalServer server, WTS_PROCESS_INFO processInfo)
        {
            this._server = server;
            this._sessionId = processInfo.SessionId;
            this._processId = processInfo.ProcessId;
            this._processName = processInfo.ProcessName;

            if (processInfo.UserSid != IntPtr.Zero)
            {
                this._securityIdentifier = new SecurityIdentifier(processInfo.UserSid);
            }
        }

        /// <summary>
        /// Gets the terminal server on which this process is running.
        /// </summary>
        public TerminalServer Server
        {
            get
            {
                return this._server;
            }
        }

        /// <summary>
        /// Gets the ID of the terminal session on the server in which the process is running.
        /// </summary>
        public int SessionId
        {
            get
            {
                return this._sessionId;
            }
        }

        /// <summary>
        /// Gets the ID of the process on the server.
        /// </summary>
        public int ProcessId
        {
            get
            {
                return this._processId;
            }
        }

        /// <summary>
        /// Gets the name of the process.
        /// </summary>
        public string ProcessName
        {
            get
            {
                return this._processName;
            }
        }

        /// <summary>
        /// Gets the security identifier under which the process is running.
        /// </summary>
        public SecurityIdentifier SecurityIdentifier
        {
            get
            {
                return this._securityIdentifier;
            }
        }

        /// <summary>
        /// Gets a <see cref="System.Diagnostics.Process" /> object that represents the process.
        /// </summary>
        public Process UnderlyingProcess
        {
            [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
            get
            {
                return this._server.ServerName == null ? Process.GetProcessById(this._processId) : Process.GetProcessById(this._processId, this._server.ServerName);
            }
        }

        /// <summary>
        /// Creates a new process associated with session Id.
        /// </summary>
        /// <param name="sessionId">Terminal session Id.</param>
        /// <param name="filename">The name of the module to be executed.</param>
        /// <param name="arguments">Arguments for the process.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public static bool CreateProcess(int sessionId, string filename, string arguments = null)
        {
            return NativeMethodsHelper.CreateProcess(sessionId, filename, arguments);
        }

        /// <summary>
        /// Terminates the process.
        /// </summary>
        /// <param name="exitCode">The exit code for the process.</param>
        public void Kill(int exitCode = -1)
        {
            NativeMethodsHelper.TerminateProcess(this._server.Handle, this._processId, exitCode);
        }
    }
}
