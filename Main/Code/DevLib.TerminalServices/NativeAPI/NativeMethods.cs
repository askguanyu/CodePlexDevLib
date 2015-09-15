//-----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices.NativeAPI
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Class NativeMethods.
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// Field CurrentSessionId.
        /// </summary>
        public const int CurrentSessionId = -1;

        /// <summary>
        /// Field LocalServerHandle.
        /// </summary>
        public static readonly IntPtr LocalServerHandle = IntPtr.Zero;

        [DllImport("Wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool WTSQuerySessionInformation(
            IntPtr hServer,
            int sessionId,
            WTS_INFO_CLASS wtsInfoClass,
            out IntPtr buffer,
            out int bytesReturned);

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WTSEnumerateSessions(
            IntPtr hServer,
            int reserved,
            int version,
            out IntPtr sessionInfo,
            out int count);

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void WTSFreeMemory(IntPtr memory);

        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "0", Justification = "Reviewed.")]
        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr WTSOpenServer(string serverName);

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void WTSCloseServer(IntPtr hServer);

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WTSLogoffSession(
            IntPtr hServer,
            int sessionId,
            bool wait);

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WTSDisconnectSession(
            IntPtr hServer,
            int sessionId,
            bool wait);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return", Justification = "Reviewed.")]
        [DllImport("winsta.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WinStationQueryInformation(
            IntPtr hServer,
            int sessionId,
            int information,
            ref WINSTATIONINFORMATIONW buffer,
            int bufferLength,
            ref int returnedLength);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return", Justification = "Reviewed.")]
        [DllImport("winsta.dll", CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "WinStationQueryInformationW")]
        public static extern int WinStationQueryInformationRemoteAddress(
            IntPtr hServer,
            int sessionId,
            WINSTATIONINFOCLASS information,
            ref WINSTATIONREMOTEADDRESS buffer,
            int bufferLength,
            out int returnedLength);

        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "2", Justification = "Reviewed.")]
        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "4", Justification = "Reviewed.")]
        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WTSSendMessage(
            IntPtr hServer,
            int sessionId,
            [MarshalAs(UnmanagedType.LPTStr)] string title,
            int titleLength,
            [MarshalAs(UnmanagedType.LPTStr)] string message,
            int messageLength,
            int style,
            int timeout,
            out RemoteMessageBoxResult result,
            bool wait);

        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "0", Justification = "Reviewed.")]
        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WTSEnumerateServers(
            [MarshalAs(UnmanagedType.LPTStr)] string pDomainName,
            int reserved,
            int version,
            out IntPtr ppServerInfo,
            out int pCount);

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WTSEnumerateProcesses(
            IntPtr hServer,
            int reserved,
            int version,
            out IntPtr ppProcessInfo,
            out int count);

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WTSShutdownSystem(
            IntPtr hServer,
            int shutdownFlag);

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WTSTerminateProcess(
            IntPtr hServer,
            int processId,
            int exitCode);

        [DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern ushort ntohs(ushort netValue);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int FileTimeToSystemTime(
            ref System.Runtime.InteropServices.ComTypes.FILETIME fileTime,
            ref SYSTEMTIME systemTime);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WTSGetActiveConsoleSessionId();

        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "0", Justification = "Reviewed.")]
        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WTSStartRemoteControlSession(
            string serverName,
            int targetSessionId,
            byte hotkeyVk,
            short hotkeyModifiers);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return", Justification = "Reviewed.")]
        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "3", Justification = "Reviewed.")]
        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "4", Justification = "Reviewed.")]
        [DllImport("winsta.dll", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int WinStationShadow(
            IntPtr hServer,
            string serverName,
            int targetSessionId,
            int hotkeyVk,
            int hotkeyModifier);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return", Justification = "Reviewed.")]
        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "2", Justification = "Reviewed.")]
        [DllImport("winsta.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WinStationShadowStop(
            IntPtr hServer,
            int targetSessionId,
            bool wait);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return", Justification = "Reviewed.")]
        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "4", Justification = "Reviewed.")]
        [DllImport("winsta.dll", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int WinStationConnectW(
            IntPtr hServer,
            int targetSessionId,
            int sourceSessionId,
            string password,
            bool wait);

        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "2", Justification = "Reviewed.")]
        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WTSConnectSession(
            int targetSessionId,
            int sourceSessionId,
            string password,
            bool wait);

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WTSStopRemoteControlSession(int targetSessionId);

        [DllImport("Wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool WTSQueryUserToken(int SessionId, ref IntPtr phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "1", Justification = "Reviewed.")]
        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "2", Justification = "Reviewed.")]
        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "8", Justification = "Reviewed.")]
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);
    }
}
