//-----------------------------------------------------------------------
// <copyright file="PSAPI_WORKING_SET_BLOCK.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Diagnostics.NativeAPI
{
    using System.Runtime.InteropServices;

    [StructLayoutAttribute(LayoutKind.Explicit)]
    internal struct PSAPI_WORKING_SET_BLOCK
    {
        [FieldOffsetAttribute(0)]
        public uint Flags;

        [FieldOffsetAttribute(0)]
        public BLOCK Block1;
    }
}
