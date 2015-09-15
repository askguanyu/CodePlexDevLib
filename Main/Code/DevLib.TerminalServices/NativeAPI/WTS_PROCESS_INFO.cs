//-----------------------------------------------------------------------
// <copyright file="WTS_PROCESS_INFO.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices.NativeAPI
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct WTS_PROCESS_INFO
    {
        public int SessionId;

        public int ProcessId;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string ProcessName;

        public IntPtr UserSid;
    }
}
