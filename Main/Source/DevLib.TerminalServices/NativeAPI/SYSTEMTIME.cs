//-----------------------------------------------------------------------
// <copyright file="SYSTEMTIME.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices.NativeAPI
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEMTIME
    {
        public short Year;

        public short Month;

        public short DayOfWeek;

        public short Day;

        public short Hour;

        public short Minute;

        public short Second;

        public short Milliseconds;
    }
}
