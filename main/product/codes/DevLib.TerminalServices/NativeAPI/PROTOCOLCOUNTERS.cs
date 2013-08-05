//-----------------------------------------------------------------------
// <copyright file="PROTOCOLCOUNTERS.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices.NativeAPI
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct PROTOCOLCOUNTERS
    {
        public int WdBytes;

        public int WdFrames;

        public int WaitForOutBuf;

        public int Frames;

        public int Bytes;

        public int CompressedBytes;

        public int CompressFlushes;

        public int Errors;

        public int Timeouts;

        public int AsyncFramingError;

        public int AsyncOverrunError;

        public int AsyncOverflowError;

        public int AsyncParityError;

        public int TdErrors;

        public short ProtocolType;

        public short Length;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public int[] Reserved;
    }
}
