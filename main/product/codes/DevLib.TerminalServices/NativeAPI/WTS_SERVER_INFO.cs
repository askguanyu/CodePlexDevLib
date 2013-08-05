//-----------------------------------------------------------------------
// <copyright file="WTS_SERVER_INFO.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices.NativeAPI
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct WTS_SERVER_INFO
    {
        [MarshalAs(UnmanagedType.LPTStr)]
        public string ServerName;
    }
}
