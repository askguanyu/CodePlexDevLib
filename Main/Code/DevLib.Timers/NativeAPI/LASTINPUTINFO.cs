//-----------------------------------------------------------------------
// <copyright file="LASTINPUTINFO.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Timers.NativeAPI
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct LASTINPUTINFO
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint cbSize;

        [MarshalAs(UnmanagedType.U4)]
        public uint dwTime;
    }
}
