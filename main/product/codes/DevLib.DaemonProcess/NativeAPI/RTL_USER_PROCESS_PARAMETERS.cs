//-----------------------------------------------------------------------
// <copyright file="RTL_USER_PROCESS_PARAMETERS.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DaemonProcess.NativeAPI
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTL_USER_PROCESS_PARAMETERS
    {
        public uint Reserved1;
        public uint Reserved2;
        public uint Reserved3;
        public uint Reserved4;
        public uint Reserved5;
        public uint Reserved6;
        public uint Reserved7;
        public uint Reserved8;
        public uint Reserved9;
        public uint Reserved10;
        public uint Reserved11;
        public uint Reserved12;
        public uint Reserved13;
        public uint Reserved14;
        public uint Reserved15;
        public uint Reserved16;
        public uint Reserved17;

        public IntPtr CommandLineAddress;
    }
}
