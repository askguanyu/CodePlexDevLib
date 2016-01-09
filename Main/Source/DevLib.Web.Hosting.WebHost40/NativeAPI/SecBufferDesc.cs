//-----------------------------------------------------------------------
// <copyright file="SecBufferDesc.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Hosting.WebHost40.NativeAPI
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// SecBufferDesc struct.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SecBufferDesc
    {
        public uint ulVersion;

        public uint cBuffers;

        public IntPtr pBuffers;
    }
}
