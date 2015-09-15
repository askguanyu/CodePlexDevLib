﻿//-----------------------------------------------------------------------
// <copyright file="MemoryInterop.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Diagnostics.NativeAPI
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    internal static class MemoryInterop
    {
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        internal static PROCESS_MEMORY_COUNTERS_EX GetCounters(IntPtr hProcess)
        {
            PROCESS_MEMORY_COUNTERS_EX counters = new PROCESS_MEMORY_COUNTERS_EX();
            counters.cb = Marshal.SizeOf(counters);

            if (NativeMethods.GetProcessMemoryInfo(hProcess, out counters, counters.cb) == 0)
            {
                int error = NativeMethods.GetLastError();
                throw new Win32Exception("GetProcessMemoryInfo failed with last error " + error.ToString() +
                    ": Marshal.SizeOf(counters) = " + Marshal.SizeOf(counters).ToString() +
                    ", counters.cb = " + counters.cb.ToString());
            }

            return counters;
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        internal static long GetPrivateWorkingSet(Process process)
        {
            SYSTEM_INFO sysinfo = new SYSTEM_INFO();
            NativeMethods.GetSystemInfo(ref sysinfo);

            int wsInfoLength = (int)(Marshal.SizeOf(new PSAPI_WORKING_SET_INFORMATION()) + Marshal.SizeOf(new PSAPI_WORKING_SET_BLOCK()) * (process.WorkingSet64 / (sysinfo.dwPageSize)));
            IntPtr workingSetPointer = Marshal.AllocHGlobal(wsInfoLength);

            if (NativeMethods.QueryWorkingSet(process.Handle, workingSetPointer, wsInfoLength) == 0)
            {
                throw new Win32Exception();
            }

            PSAPI_WORKING_SET_INFORMATION workingSet = GenerateWorkingSetArray(workingSetPointer);
            Marshal.FreeHGlobal(workingSetPointer);

            return CalculatePrivatePages(workingSet) * sysinfo.dwPageSize;
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        private static PSAPI_WORKING_SET_INFORMATION GenerateWorkingSetArray(IntPtr workingSetPointer)
        {
            int entries = Marshal.ReadInt32(workingSetPointer);

            PSAPI_WORKING_SET_INFORMATION workingSet = new PSAPI_WORKING_SET_INFORMATION();
            workingSet.NumberOfEntries = entries;
            workingSet.WorkingSetInfo = new PSAPI_WORKING_SET_BLOCK[entries];

            for (int i = 0; i < entries; i++)
            {
                workingSet.WorkingSetInfo[i].Flags = (uint)Marshal.ReadInt32(workingSetPointer, 4 + i * 4);
            }

            return workingSet;
        }

        private static int CalculatePrivatePages(PSAPI_WORKING_SET_INFORMATION workingSet)
        {
            int totalPages = workingSet.NumberOfEntries;
            int privatePages = 0;

            for (int i = 0; i < totalPages; i++)
            {
                if (workingSet.WorkingSetInfo[i].Block1.Shared == 0)
                {
                    privatePages++;
                }
            }

            return privatePages;
        }
    }
}
