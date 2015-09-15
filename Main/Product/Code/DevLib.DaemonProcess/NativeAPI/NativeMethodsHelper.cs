//-----------------------------------------------------------------------
// <copyright file="NativeMethodsHelper.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DaemonProcess.NativeAPI
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    /// <summary>
    /// Class NativeMethodsHelper.
    /// </summary>
    internal static class NativeMethodsHelper
    {
        /// <summary>
        /// Get process command line with native methods.
        /// </summary>
        /// <param name="processId">Process Id.</param>
        /// <returns>Process command line.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static string GetCommandLine(int processId)
        {
            string result = string.Empty;

            try
            {
                result = GetCommandLine(GetPEBAddress(processId));
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
            }

            return result;
        }

        /// <summary>
        /// Method GetPEBAddress.
        /// </summary>
        /// <param name="processId">Process Id.</param>
        /// <returns>PEB Address.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private static IntPtr GetPEBAddress(int processId)
        {
            IntPtr outPtr = IntPtr.Zero;

            Process process = null;

            try
            {
                process = Process.GetProcessById(processId);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                return outPtr;
            }

            IntPtr pbi = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PROCESS_BASIC_INFORMATION)));

            IntPtr outLong = Marshal.AllocHGlobal(sizeof(long));

            try
            {
                bool queryStatus = NativeMethods.NtQueryInformationProcess(process.Handle, 0, pbi, (uint)Marshal.SizeOf(typeof(PROCESS_BASIC_INFORMATION)), outLong) == 0;

                if (queryStatus)
                {
                    outPtr = ((PROCESS_BASIC_INFORMATION)Marshal.PtrToStructure(pbi, typeof(PROCESS_BASIC_INFORMATION))).PebBaseAddress;
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
            }
            finally
            {
                if (process != null)
                {
                    process.Dispose();
                    process = null;
                }

                if (outLong != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(outLong);
                    outLong = IntPtr.Zero;
                }

                if (pbi != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pbi);
                    pbi = IntPtr.Zero;
                }
            }

            return outPtr;
        }

        /// <summary>
        /// Method GetCommandLine.
        /// </summary>
        /// <param name="pebAddress">PEB Address.</param>
        /// <returns>Process command line.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private static string GetCommandLine(IntPtr pebAddress)
        {
            string result = string.Empty;

            if (pebAddress == IntPtr.Zero)
            {
                return result;
            }

            try
            {
                PEB_32 peb = (PEB_32)Marshal.PtrToStructure(pebAddress, typeof(PEB_32));

                IntPtr processParametersAddress = peb.ProcessParametersAddress;

                RTL_USER_PROCESS_PARAMETERS processParameters = (RTL_USER_PROCESS_PARAMETERS)Marshal.PtrToStructure(processParametersAddress, typeof(RTL_USER_PROCESS_PARAMETERS));

                result = Marshal.PtrToStringAuto(processParameters.CommandLineAddress);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
            }

            return result;
        }
    }
}
