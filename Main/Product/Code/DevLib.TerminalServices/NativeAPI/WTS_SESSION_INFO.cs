//-----------------------------------------------------------------------
// <copyright file="WTS_SESSION_INFO.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices.NativeAPI
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct WTS_SESSION_INFO
    {
        public int SessionID;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string WinStationName;

        public ConnectState ConnectState;
    }
}
