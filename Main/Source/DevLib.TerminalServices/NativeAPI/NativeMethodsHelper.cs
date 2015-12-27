//-----------------------------------------------------------------------
// <copyright file="NativeMethodsHelper.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices.NativeAPI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    /// <summary>
    /// Class NativeMethodsHelper.
    /// </summary>
    internal static class NativeMethodsHelper
    {
        private delegate T ProcessSessionCallback<T>(IntPtr buffer, int returnedBytes);

        /// <summary>
        /// Gets the state of the connect.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns>Connect state.</returns>
        public static ConnectState GetConnectState(ITerminalServerHandle server, int sessionId)
        {
            return QuerySessionInformation(
                server,
                sessionId,
                WTS_INFO_CLASS.WTSConnectState,
                (buffer, returned) => (ConnectState)Marshal.ReadInt32(buffer));
        }

        /// <summary>
        /// Queries the session information for string.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="infoClass">The information class.</param>
        /// <returns>Session information.</returns>
        public static string QuerySessionInformationForString(ITerminalServerHandle server, int sessionId, WTS_INFO_CLASS infoClass)
        {
            return QuerySessionInformation(
                server,
                sessionId,
                infoClass,
                (buffer, returned) => buffer == IntPtr.Zero ? null : Marshal.PtrToStringAuto(buffer));
        }

        /// <summary>
        /// Queries the session information for structure.
        /// </summary>
        /// <typeparam name="T">Type of structure</typeparam>
        /// <param name="server">The server.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="infoClass">The information class.</param>
        /// <returns>Structure instance.</returns>
        public static T QuerySessionInformationForStruct<T>(ITerminalServerHandle server, int sessionId, WTS_INFO_CLASS infoClass) where T : struct
        {
            return QuerySessionInformation(
                server,
                sessionId,
                infoClass,
                (buffer, returned) => (T)Marshal.PtrToStructure(buffer, typeof(T)));
        }

        /// <summary>
        /// Gets the win station information.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns>WINSTATIONINFORMATIONW instance.</returns>
        public static WINSTATIONINFORMATIONW GetWinStationInformation(ITerminalServerHandle server, int sessionId)
        {
            var retLen = 0;

            var wsInfo = new WINSTATIONINFORMATIONW();

            if (NativeMethods.WinStationQueryInformation(
                server.Handle,
                sessionId,
                (int)WINSTATIONINFOCLASS.WinStationInformation,
                ref wsInfo,
                Marshal.SizeOf(typeof(WINSTATIONINFORMATIONW)),
                ref retLen) != 0)
            {
                return wsInfo;
            }

            throw new Win32Exception();
        }

        /// <summary>
        /// Converts FileTime to DateTime.
        /// </summary>
        /// <param name="fileTime">The file time.</param>
        /// <returns>DateTime instance.</returns>
        public static DateTime? FileTimeToDateTime(System.Runtime.InteropServices.ComTypes.FILETIME fileTime)
        {
            var sysTime = new SYSTEMTIME();

            if (NativeMethods.FileTimeToSystemTime(ref fileTime, ref sysTime) == 0)
            {
                return null;
            }

            if (sysTime.Year < 1900)
            {
                return null;
            }

            return new DateTime(
                sysTime.Year,
                sysTime.Month,
                sysTime.Day,
                sysTime.Hour,
                sysTime.Minute,
                sysTime.Second,
                sysTime.Milliseconds,
                DateTimeKind.Utc).ToLocalTime();
        }

        /// <summary>
        /// Gets the session information.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <returns>List of WTS_SESSION_INFO.</returns>
        public static List<WTS_SESSION_INFO> GetSessionInfos(ITerminalServerHandle server)
        {
            IntPtr ppSessionInfo;

            int count;

            if (NativeMethods.WTSEnumerateSessions(server.Handle, 0, 1, out ppSessionInfo, out count) == 0)
            {
                throw new Win32Exception();
            }

            try
            {
                return PtrToStructureList<WTS_SESSION_INFO>(ppSessionInfo, count);
            }
            finally
            {
                if (ppSessionInfo != IntPtr.Zero)
                {
                    NativeMethods.WTSFreeMemory(ppSessionInfo);
                    ppSessionInfo = IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// Logoffs the session.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="wait">Whether to wait logoff.</param>
        public static void LogoffSession(ITerminalServerHandle server, int sessionId, bool wait)
        {
            if (NativeMethods.WTSLogoffSession(server.Handle, sessionId, wait) == 0)
            {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Disconnects the session.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="wait">Whether to wait disconnect.</param>
        public static void DisconnectSession(ITerminalServerHandle server, int sessionId, bool wait)
        {
            if (NativeMethods.WTSDisconnectSession(server.Handle, sessionId, wait) == 0)
            {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        /// <param name="style">The style.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="wait">Whether to wait send.</param>
        /// <returns>RemoteMessageBoxResult instance.</returns>
        public static RemoteMessageBoxResult SendMessage(ITerminalServerHandle server, int sessionId, string title, string message, int style, int timeout, bool wait)
        {
            RemoteMessageBoxResult result;

            title = string.IsNullOrEmpty(title) ? " " : title;

            message = message ?? string.Empty;

            if (NativeMethods.WTSSendMessage(
                server.Handle,
                sessionId,
                title,
                title.Length * Marshal.SystemDefaultCharSize,
                message,
                message.Length * Marshal.SystemDefaultCharSize,
                style,
                timeout,
                out result,
                wait) == 0)
            {
                throw new Win32Exception();
            }

            return result;
        }

        /// <summary>
        /// Enumerates the servers.
        /// </summary>
        /// <param name="domainName">Name of the domain.</param>
        /// <returns>List of WTS_SERVER_INFO.</returns>
        public static List<WTS_SERVER_INFO> EnumerateServers(string domainName)
        {
            IntPtr ppServerInfo;

            int count;

            if (NativeMethods.WTSEnumerateServers(domainName, 0, 1, out ppServerInfo, out count) == 0)
            {
                throw new Win32Exception();
            }

            try
            {
                return PtrToStructureList<WTS_SERVER_INFO>(ppServerInfo, count);
            }
            finally
            {
                if (ppServerInfo != IntPtr.Zero)
                {
                    NativeMethods.WTSFreeMemory(ppServerInfo);
                    ppServerInfo = IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// Executes action on each process information.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="callback">The callback.</param>
        public static void ForEachProcessInfo(ITerminalServerHandle server, Action<WTS_PROCESS_INFO> callback)
        {
            IntPtr ppProcessInfo;

            int count;

            if (NativeMethods.WTSEnumerateProcesses(server.Handle, 0, 1, out ppProcessInfo, out count) == 0)
            {
                throw new Win32Exception();
            }

            try
            {
                var processInfos = PtrToStructureList<WTS_PROCESS_INFO>(ppProcessInfo, count);

                foreach (WTS_PROCESS_INFO processInfo in processInfos)
                {
                    if (processInfo.ProcessId != 0)
                    {
                        callback(processInfo);
                    }
                }
            }
            finally
            {
                if (ppProcessInfo != IntPtr.Zero)
                {
                    NativeMethods.WTSFreeMemory(ppProcessInfo);
                    ppProcessInfo = IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// Terminates the process.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="processId">The process identifier.</param>
        /// <param name="exitCode">The exit code.</param>
        public static void TerminateProcess(ITerminalServerHandle server, int processId, int exitCode)
        {
            if (NativeMethods.WTSTerminateProcess(server.Handle, processId, exitCode) == 0)
            {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Queries the session information for int.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="infoClass">The information class.</param>
        /// <returns>Query result.</returns>
        public static int QuerySessionInformationForInt(ITerminalServerHandle server, int sessionId, WTS_INFO_CLASS infoClass)
        {
            return QuerySessionInformation(server, sessionId, infoClass, (buffer, returned) => Marshal.ReadInt32(buffer));
        }

        /// <summary>
        /// Shutdowns the system.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="flags">The flags.</param>
        public static void ShutdownSystem(ITerminalServerHandle server, int flags)
        {
            if (NativeMethods.WTSShutdownSystem(server.Handle, flags) == 0)
            {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Converts FileTime to DateTime.
        /// </summary>
        /// <param name="fileTime">The file time.</param>
        /// <returns>DateTime instance.</returns>
        public static DateTime? FileTimeToDateTime(long fileTime)
        {
            if (fileTime == 0)
            {
                return null;
            }

            return DateTime.FromFileTime(fileTime);
        }

        /// <summary>
        /// Queries the session information for short.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="infoClass">The information class.</param>
        /// <returns>Query result.</returns>
        public static short QuerySessionInformationForShort(ITerminalServerHandle server, int sessionId, WTS_INFO_CLASS infoClass)
        {
            return QuerySessionInformation(server, sessionId, infoClass, (buffer, returned) => Marshal.ReadInt16(buffer));
        }

        /// <summary>
        /// Extracts the ip address.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="rawAddress">The raw address.</param>
        /// <returns>IPAddress instance.</returns>
        public static IPAddress ExtractIPAddress(AddressFamily family, byte[] rawAddress)
        {
            switch (family)
            {
                case AddressFamily.InterNetwork:
                    var v4Address = new byte[4];
                    Array.Copy(rawAddress, 2, v4Address, 0, 4);
                    return new IPAddress(v4Address);
                case AddressFamily.InterNetworkV6:
                    var v6Address = new byte[16];
                    Array.Copy(rawAddress, 2, v6Address, 0, 16);
                    return new IPAddress(v6Address);
            }

            return null;
        }

        /// <summary>
        /// Queries the session information for end point.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns>EndPoint instance.</returns>
        public static EndPoint QuerySessionInformationForEndPoint(ITerminalServerHandle server, int sessionId)
        {
            int retLen;

            var remoteAddress = new WINSTATIONREMOTEADDRESS();

            if (NativeMethods.WinStationQueryInformationRemoteAddress(
                server.Handle,
                sessionId,
                WINSTATIONINFOCLASS.WinStationRemoteAddress,
                ref remoteAddress,
                Marshal.SizeOf(typeof(WINSTATIONREMOTEADDRESS)),
                out retLen) != 0)
            {
                var ipAddress = ExtractIPAddress(remoteAddress.Family, remoteAddress.Address);

                int port = NativeMethods.ntohs((ushort)remoteAddress.Port);

                return ipAddress == null ? null : new IPEndPoint(ipAddress, port);
            }

            throw new Win32Exception();
        }

        /// <summary>
        /// Legacies the start remote control.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="hotkey">The hotkey.</param>
        /// <param name="hotkeyModifiers">The hotkey modifiers.</param>
        public static void LegacyStartRemoteControl(ITerminalServerHandle server, int sessionId, ConsoleKey hotkey, RemoteControlHotkeyModifiers hotkeyModifiers)
        {
            if (NativeMethods.WinStationShadow(
                server.Handle,
                server.ServerName,
                sessionId,
                (int)hotkey,
                (int)hotkeyModifiers) == 0)
            {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Starts the remote control.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="hotkey">The hotkey.</param>
        /// <param name="hotkeyModifiers">The hotkey modifiers.</param>
        public static void StartRemoteControl(ITerminalServerHandle server, int sessionId, ConsoleKey hotkey, RemoteControlHotkeyModifiers hotkeyModifiers)
        {
            if (NativeMethods.WTSStartRemoteControlSession(
                server.ServerName,
                sessionId,
                (byte)hotkey,
                (short)hotkeyModifiers) == 0)
            {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Legacies the stop remote control.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="wait">Whether to wait stop.</param>
        public static void LegacyStopRemoteControl(ITerminalServerHandle server, int sessionId, bool wait)
        {
            if (NativeMethods.WinStationShadowStop(server.Handle, sessionId, wait) == 0)
            {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Stops the remote control.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        public static void StopRemoteControl(int sessionId)
        {
            if (NativeMethods.WTSStopRemoteControlSession(sessionId) == 0)
            {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Legacies the connect.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="sourceSessionId">The source session identifier.</param>
        /// <param name="targetSessionId">The target session identifier.</param>
        /// <param name="password">The password.</param>
        /// <param name="wait">Whether to wait connect.</param>
        public static void LegacyConnect(ITerminalServerHandle server, int sourceSessionId, int targetSessionId, string password, bool wait)
        {
            if (NativeMethods.WinStationConnectW(server.Handle, targetSessionId, sourceSessionId, password, wait) == 0)
            {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Connects the specified source session identifier.
        /// </summary>
        /// <param name="sourceSessionId">The source session identifier.</param>
        /// <param name="targetSessionId">The target session identifier.</param>
        /// <param name="password">The password.</param>
        /// <param name="wait">Whether to wait connect.</param>
        public static void Connect(int sourceSessionId, int targetSessionId, string password, bool wait)
        {
            if (NativeMethods.WTSConnectSession(targetSessionId, sourceSessionId, password, wait) == 0)
            {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Gets the active console session identifier.
        /// </summary>
        /// <returns>Session identifier.</returns>
        public static int? GetActiveConsoleSessionId()
        {
            var sessionId = NativeMethods.WTSGetActiveConsoleSessionId();

            return sessionId == -1 ? (int?)null : sessionId;
        }

        /// <summary>
        /// Creates the process.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>Whether succeeded.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public static bool CreateProcess(int sessionId, string filename, string arguments)
        {
            IntPtr userTokenHandle = IntPtr.Zero;

            try
            {
                string commandLine = string.Format("{0} {1}", filename, arguments ?? string.Empty);

                NativeMethods.WTSQueryUserToken(sessionId, ref userTokenHandle);

                PROCESS_INFORMATION processInfo = new PROCESS_INFORMATION();

                STARTUPINFO startupInfo = new STARTUPINFO();

                startupInfo.cb = Marshal.SizeOf(startupInfo);

                bool result = NativeMethods.CreateProcessAsUser(
                    userTokenHandle,
                    filename,
                    commandLine,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    0,
                    IntPtr.Zero,
                    null,
                    ref startupInfo,
                    out processInfo);

                return result;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (userTokenHandle != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(userTokenHandle);
                    userTokenHandle = IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// Queries the session information.
        /// </summary>
        /// <typeparam name="T">Type of Session information.</typeparam>
        /// <param name="server">The server.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="infoClass">The information class.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>Session information.</returns>
        private static T QuerySessionInformation<T>(ITerminalServerHandle server, int sessionId, WTS_INFO_CLASS infoClass, ProcessSessionCallback<T> callback)
        {
            int returned;

            IntPtr buffer = IntPtr.Zero;

            try
            {
                if (NativeMethods.WTSQuerySessionInformation(server.Handle, sessionId, infoClass, out buffer, out returned))
                {
                    return callback(buffer, returned);
                }
                else
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                {
                    NativeMethods.WTSFreeMemory(buffer);
                    buffer = IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// Converts pointer to structure list.
        /// </summary>
        /// <typeparam name="T">Type of structure.</typeparam>
        /// <param name="ppList">The pointer of list.</param>
        /// <param name="count">The count.</param>
        /// <returns>List of structure.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        private static List<T> PtrToStructureList<T>(IntPtr ppList, int count) where T : struct
        {
            var result = new List<T>();

            var pointer = ppList.ToInt64();

            var sizeOf = Marshal.SizeOf(typeof(T));

            for (var index = 0; index < count; index++)
            {
                var item = (T)Marshal.PtrToStructure(new IntPtr(pointer), typeof(T));

                result.Add(item);

                pointer += sizeOf;
            }

            return result;
        }
    }
}
