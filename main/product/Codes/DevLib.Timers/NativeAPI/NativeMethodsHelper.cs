//-----------------------------------------------------------------------
// <copyright file="NativeMethodsHelper.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Timers.NativeAPI
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Class NativeMethodsHelper.
    /// </summary>
    internal static class NativeMethodsHelper
    {
        /// <summary>
        /// Method GetLastInputTime.
        /// </summary>
        /// <returns>The time in milliseconds since last user input.</returns>
        public static uint GetLastInputTime()
        {
            uint idleTime = 0;

            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            uint envTicks = (uint)Environment.TickCount;

            if (NativeMethods.GetLastInputInfo(ref lastInputInfo))
            {
                uint lastInputTick = lastInputInfo.dwTime;

                idleTime = envTicks - lastInputTick;
            }

            return (idleTime > 0) ? idleTime : 0;
        }
    }
}
