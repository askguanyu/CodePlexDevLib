//-----------------------------------------------------------------------
// <copyright file="SecHandle.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Hosting.WebHost20.NativeAPI
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// SecHandle struct.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SecHandle
    {
        public IntPtr dwLower;

        public IntPtr dwUpper;
    }
}
