//-----------------------------------------------------------------------
// <copyright file="PROCESS_MEMORY_COUNTERS.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Diagnostics.NativeAPI
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct PROCESS_MEMORY_COUNTERS
    {
        public int cb;
        public int PageFaultCount;
        public int PeakWorkingSetSize;
        public int WorkingSetSize;
        public int QuotaPeakPagedPoolUsage;
        public int QuotaPagedPoolUsage;
        public int QuotaPeakNonPagedPoolUsage;
        public int QuotaNonPagedPoolUsage;
        public int PagefileUsage;
        public int PeakPagefileUsage;
    }
}
