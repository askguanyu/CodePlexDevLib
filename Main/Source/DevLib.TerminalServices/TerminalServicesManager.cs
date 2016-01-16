//-----------------------------------------------------------------------
// <copyright file="TerminalServicesManager.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Permissions;
    using DevLib.TerminalServices.NativeAPI;

    /// <summary>
    /// Class TerminalServicesManager.
    /// </summary>
    public static class TerminalServicesManager
    {
        /// <summary>
        /// Gets the session in which the current process is running.
        /// </summary>
        public static TerminalServicesSession CurrentSession
        {
            [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
            get
            {
                return new TerminalServicesSession(GetLocalServer(), Process.GetCurrentProcess().SessionId);
            }
        }

        /// <summary>
        /// Gets the session to which the physical keyboard and mouse are connected.
        /// </summary>
        public static TerminalServicesSession ActiveConsoleSession
        {
            get
            {
                int? sessionId = NativeMethodsHelper.GetActiveConsoleSessionId();

                return sessionId == null ? null : new TerminalServicesSession(GetLocalServer(), sessionId.Value);
            }
        }

        /// <summary>
        /// Creates a connection to a remote terminal server.
        /// </summary>
        /// <param name="serverName">The name of the terminal server.</param>
        /// <returns>A <see cref="TerminalServer" /> instance representing the requested server.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static TerminalServer GetRemoteServer(string serverName)
        {
            return new TerminalServer(new RemoteServerHandle(serverName));
        }

        /// <summary>
        /// Creates a connection to the local terminal server.
        /// </summary>
        /// <returns>A <see cref="TerminalServer" /> instance representing the local server.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static TerminalServer GetLocalServer()
        {
            return new TerminalServer(new LocalServerHandle());
        }

        /// <summary>
        /// Enumerates all terminal servers in a given domain.
        /// </summary>
        /// <param name="domainName">The name of the domain.</param>
        /// <returns>A list of terminal servers in the domain.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static List<TerminalServer> GetServers(string domainName)
        {
            List<TerminalServer> servers = new List<TerminalServer>();

            foreach (WTS_SERVER_INFO serverInfo in NativeMethodsHelper.EnumerateServers(domainName))
            {
                servers.Add(new TerminalServer(new RemoteServerHandle(serverInfo.ServerName)));
            }

            return servers;
        }
    }
}
