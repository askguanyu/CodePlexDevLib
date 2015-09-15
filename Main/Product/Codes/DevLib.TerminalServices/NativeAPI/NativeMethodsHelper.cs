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
        public delegate void ListProcessInfosCallback(WTS_PROCESS_INFO processInfo);

        private delegate T ProcessSessionCallback<T>(IntPtr buffer, int returnedBytes);

        public static ConnectState GetConnectState(ITerminalServerHandle server, int sessionId)
        {
            return QuerySessionInformation(
                server,
                sessionId,
                WTS_INFO_CLASS.WTSConnectState,
                (buffer, returned) => (ConnectState)Marshal.ReadInt32(buffer));
        }

        public static string QuerySessionInformationForString(ITerminalServerHandle server, int sessionId, WTS_INFO_CLASS infoClass)
        {
            return QuerySessionInformation(
                server,
                sessionId,
                infoClass,
                (buffer, returned) => buffer == IntPtr.Zero ? null : Marshal.PtrToStringAuto(buffer));
        }

        public static T QuerySessionInformationForStruct<T>(ITerminalServerHandle server, int sessionId, WTS_INFO_CLASS infoClass) where T : struct
        {
            return QuerySessionInformation(
                server,
                sessionId,
                infoClass,
                (buffer, returned) => (T)Marshal.PtrToStructure(buffer, typeof(T)));
        }

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

        public static void LogoffSession(ITerminalServerHandle server, int sessionId, bool wait)
        {
            if (NativeMethods.WTSLogoffSession(server.Handle, sessionId, wait) == 0)
            {
                throw new Win32Exception();
            }
        }

        public static void DisconnectSession(ITerminalServerHandle server, int sessionId, bool wait)
        {
            if (NativeMethods.WTSDisconnectSession(server.Handle, sessionId, wait) == 0)
            {
                throw new Win32Exception();
            }
        }

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

        public static void ForEachProcessInfo(ITerminalServerHandle server, ListProcessInfosCallback callback)
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

        public static void TerminateProcess(ITerminalServerHandle server, int processId, int exitCode)
        {
            if (NativeMethods.WTSTerminateProcess(server.Handle, processId, exitCode) == 0)
            {
                throw new Win32Exception();
            }
        }

        public static int QuerySessionInformationForInt(ITerminalServerHandle server, int sessionId, WTS_INFO_CLASS infoClass)
        {
            return QuerySessionInformation(server, sessionId, infoClass, (buffer, returned) => Marshal.ReadInt32(buffer));
        }

        public static void ShutdownSystem(ITerminalServerHandle server, int flags)
        {
            if (NativeMethods.WTSShutdownSystem(server.Handle, flags) == 0)
            {
                throw new Win32Exception();
            }
        }

        public static DateTime? FileTimeToDateTime(long fileTime)
        {
            if (fileTime == 0)
            {
                return null;
            }

            return DateTime.FromFileTime(fileTime);
        }

        public static short QuerySessionInformationForShort(ITerminalServerHandle server, int sessionId, WTS_INFO_CLASS infoClass)
        {
            return QuerySessionInformation(server, sessionId, infoClass, (buffer, returned) => Marshal.ReadInt16(buffer));
        }

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

        public static void LegacyStopRemoteControl(ITerminalServerHandle server, int sessionId, bool wait)
        {
            if (NativeMethods.WinStationShadowStop(server.Handle, sessionId, wait) == 0)
            {
                throw new Win32Exception();
            }
        }

        public static void StopRemoteControl(int sessionId)
        {
            if (NativeMethods.WTSStopRemoteControlSession(sessionId) == 0)
            {
                throw new Win32Exception();
            }
        }

        public static void LegacyConnect(ITerminalServerHandle server, int sourceSessionId, int targetSessionId, string password, bool wait)
        {
            if (NativeMethods.WinStationConnectW(server.Handle, targetSessionId, sourceSessionId, password, wait) == 0)
            {
                throw new Win32Exception();
            }
        }

        public static void Connect(int sourceSessionId, int targetSessionId, string password, bool wait)
        {
            if (NativeMethods.WTSConnectSession(targetSessionId, sourceSessionId, password, wait) == 0)
            {
                throw new Win32Exception();
            }
        }

        public static int? GetActiveConsoleSessionId()
        {
            var sessionId = NativeMethods.WTSGetActiveConsoleSessionId();

            return sessionId == -1 ? (int?)null : sessionId;
        }

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
