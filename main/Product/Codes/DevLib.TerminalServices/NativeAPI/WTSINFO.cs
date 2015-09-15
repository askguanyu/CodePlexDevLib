﻿//-----------------------------------------------------------------------
// <copyright file="WTSINFO.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices.NativeAPI
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct WTSINFO
    {
        public ConnectState ConnectState;

        public int SessionId;

        public int IncomingBytes;

        public int OutgoingBytes;

        public int IncomingFrames;

        public int OutgoingFrames;

        public int IncomingCompressedBytes;

        public int OutgoingCompressedBytes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string WinStationName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
        public string Domain;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string UserName;

        [MarshalAs(UnmanagedType.I8)]
        public long ConnectTime;

        [MarshalAs(UnmanagedType.I8)]
        public long DisconnectTime;

        [MarshalAs(UnmanagedType.I8)]
        public long LastInputTime;

        [MarshalAs(UnmanagedType.I8)]
        public long LogonTime;

        [MarshalAs(UnmanagedType.I8)]
        public long CurrentTime;
    }
}
