//-----------------------------------------------------------------------
// <copyright file="WTSINFOEX.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices.NativeAPI
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct WTSINFOEX
    {
        public int Level;

        public int Unknown;

        public WTSINFOEX_LEVEL1 Data;
    }
}
