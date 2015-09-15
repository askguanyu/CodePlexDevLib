//-----------------------------------------------------------------------
// <copyright file="PROTOCOLSTATUS.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices.NativeAPI
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct PROTOCOLSTATUS
    {
        public PROTOCOLCOUNTERS Output;

        public PROTOCOLCOUNTERS Input;

        public CACHE_STATISTICS Statistics;

        public int AsyncSignal;

        public int AsyncSignalMask;
    }
}
