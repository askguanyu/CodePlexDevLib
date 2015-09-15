//-----------------------------------------------------------------------
// <copyright file="STARTUPINFO.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices.NativeAPI
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct STARTUPINFO
    {
        public int cb;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpReserved;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpDesktop;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpTitle;

        public int dwX;

        public int dwY;

        public int dwXSize;

        public int dwYSize;

        public int dwXCountChars;

        public int dwYCountChars;

        public int dwFillAttribute;

        public int dwFlags;

        public short wShowWindow;

        public short cbReserved2;

        public IntPtr lpReserved2;

        public IntPtr hStdInput;

        public IntPtr hStdOutput;

        public IntPtr hStdError;
    }
}
