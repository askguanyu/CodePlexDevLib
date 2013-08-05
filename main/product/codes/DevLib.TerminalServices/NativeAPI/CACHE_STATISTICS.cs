//-----------------------------------------------------------------------
// <copyright file="CACHE_STATISTICS.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices.NativeAPI
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct CACHE_STATISTICS
    {
        public readonly short ProtocolType;

        public readonly short Length;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly int[] Reserved;
    }
}
