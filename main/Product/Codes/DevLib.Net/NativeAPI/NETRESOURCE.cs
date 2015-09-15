//-----------------------------------------------------------------------
// <copyright file="NETRESOURCE.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.NativeAPI
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal class NETRESOURCE
    {
        public int dwScope = 0;
        public int dwType = 0;
        public int dwDisplayType = 0;
        public int dwUsage = 0;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpLocalName = "";
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpRemoteName = "";
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpComment = "";
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpProvider = "";
    }
}
