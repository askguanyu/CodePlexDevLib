//-----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DaemonProcess.NativeAPI
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Class NativeMethods.
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, IntPtr processInformation, uint processInformationLength, IntPtr returnLength);

        [DllImport("NTDLL.DLL", SetLastError = true)]
        public static extern int NtQueryInformationProcess(IntPtr hProcess, PROCESSINFOCLASS pic, ref PROCESS_BASIC_INFORMATION pbi, int cb, out int pSize);
    }
}
