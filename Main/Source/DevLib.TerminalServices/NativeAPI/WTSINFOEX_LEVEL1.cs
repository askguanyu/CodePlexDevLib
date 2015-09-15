//-----------------------------------------------------------------------
// <copyright file="WTSINFOEX_LEVEL1.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices.NativeAPI
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct WTSINFOEX_LEVEL1
    {
        public int SessionId;

        public ConnectState ConnectState;

        public int SessionFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public string WinStationName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string UserName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
        public string DomainName;

        public int Unknown;

        public System.Runtime.InteropServices.ComTypes.FILETIME LogonTime;

        public System.Runtime.InteropServices.ComTypes.FILETIME ConnectTime;

        public System.Runtime.InteropServices.ComTypes.FILETIME DisconnectTime;

        public System.Runtime.InteropServices.ComTypes.FILETIME LastInputTime;

        public System.Runtime.InteropServices.ComTypes.FILETIME CurrentTime;

        public int IncomingBytes;

        public int OutgoingBytes;

        public int IncomingFrames;

        public int OutgoingFrames;

        public int IncomingCompressedBytes;

        public int OutgoingCompressedBytes;
    }
}
