//-----------------------------------------------------------------------
// <copyright file="PROCESSOR_INFO_UNION.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Diagnostics.NativeAPI
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    internal struct PROCESSOR_INFO_UNION
    {
        [FieldOffset(0)]
        internal uint dwOemId;

        [FieldOffset(0)]
        internal ushort wProcessorArchitecture;

        [FieldOffset(2)]
        internal ushort wReserved;
    }
}
