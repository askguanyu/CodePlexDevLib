//-----------------------------------------------------------------------
// <copyright file="PSAPI_WORKING_SET_INFORMATION.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Diagnostics.NativeAPI
{
    using System.Runtime.InteropServices;

    [StructLayoutAttribute(LayoutKind.Sequential)]
    internal class PSAPI_WORKING_SET_INFORMATION
    {
        public int NumberOfEntries;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.Struct)]
        public PSAPI_WORKING_SET_BLOCK[] WorkingSetInfo;
    }
}
