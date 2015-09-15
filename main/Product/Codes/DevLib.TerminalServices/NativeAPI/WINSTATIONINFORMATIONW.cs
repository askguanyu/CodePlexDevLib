//-----------------------------------------------------------------------
// <copyright file="WINSTATIONINFORMATIONW.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices.NativeAPI
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct WINSTATIONINFORMATIONW
    {
        public ConnectState ConnectState;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string WinStationName;

        public int SessionId;

        public int Unknown;

        public System.Runtime.InteropServices.ComTypes.FILETIME ConnectTime;

        public System.Runtime.InteropServices.ComTypes.FILETIME DisconnectTime;

        public System.Runtime.InteropServices.ComTypes.FILETIME LastInputTime;

        public System.Runtime.InteropServices.ComTypes.FILETIME LoginTime;

        public PROTOCOLSTATUS ProtocolStatus;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
        public string Domain;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)]
        public string UserName;

        public System.Runtime.InteropServices.ComTypes.FILETIME CurrentTime;
    }
}
