//-----------------------------------------------------------------------
// <copyright file="PEB.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DaemonProcess.NativeAPI
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct PEB_32
    {
        public uint Reserved1;
        public uint Reserved2;
        public uint Reserved3;
        public uint Reserved4;

        public IntPtr ProcessParametersAddress;
    }
}
