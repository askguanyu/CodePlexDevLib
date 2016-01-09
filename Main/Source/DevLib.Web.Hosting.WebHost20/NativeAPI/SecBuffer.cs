//-----------------------------------------------------------------------
// <copyright file="SecBuffer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Hosting.WebHost20.NativeAPI
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// SecBuffer struct.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SecBuffer
    {
        public uint cbBuffer;

        public uint BufferType;

        public IntPtr pvBuffer;
    }
}
