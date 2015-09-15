//-----------------------------------------------------------------------
// <copyright file="WTS_CLIENT_DISPLAY.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices.NativeAPI
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct WTS_CLIENT_DISPLAY
    {
        public int HorizontalResolution;

        public int VerticalResolution;

        public int ColorDepth;
    }
}
