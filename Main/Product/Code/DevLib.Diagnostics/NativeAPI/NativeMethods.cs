//-----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Diagnostics.NativeAPI
{
    using System;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        internal const int GR_GDIOBJECTS = 0;
        internal const int GR_USEROBJECTS = 1;

        [DllImport("User32.dll")]
        internal static extern int GetGuiResources(IntPtr hProcess, int flags);

        [DllImport("kernel32.dll")]
        internal static extern void GetSystemInfo([MarshalAs(UnmanagedType.Struct)] ref SYSTEM_INFO lpSystemInfo);

        // Interop call the get workingset information.
        [DllImport("psapi.dll", SetLastError = true)]
        internal static extern int QueryWorkingSet(IntPtr hProcess, IntPtr info, int size);

        // Interop call the get performance memory counters
        [DllImport("psapi.dll", SetLastError = true)]
        internal static extern int GetProcessMemoryInfo(IntPtr hProcess, out PROCESS_MEMORY_COUNTERS_EX counters, int size);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int GetLastError();
    }
}
