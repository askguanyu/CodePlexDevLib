//-----------------------------------------------------------------------
// <copyright file="BLOCK.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Diagnostics.NativeAPI
{
    using System.Runtime.InteropServices;

    [StructLayoutAttribute(LayoutKind.Sequential)]
    internal struct BLOCK
    {
        public uint bitvector1;
        public uint Protection;
        public uint ShareCount;
        public uint Reserved;
        public uint VirtualPage;

        public uint Shared
        {
            get { return ((uint)(((this.bitvector1 & 256u) >> 8))); }
        }
    }
}
