//-----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Hosting.WebHost40
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Class NativeMethods.
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport("ADVAPI32.DLL", SetLastError = true)]
        public static extern bool ImpersonateSelf(int level);

        [DllImport("ADVAPI32.DLL", SetLastError = true)]
        public static extern int RevertToSelf();

        [DllImport("KERNEL32.DLL", SetLastError = true)]
        public static extern IntPtr GetCurrentThread();

        [DllImport("ADVAPI32.DLL", SetLastError = true)]
        public static extern int OpenThreadToken(IntPtr thread, int access, bool openAsSelf, ref IntPtr hToken);
    }
}
