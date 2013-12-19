//-----------------------------------------------------------------------
// <copyright file="DaemonProcessHelper.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DaemonProcess
{
    using System;
    using System.Collections.Generic;
    using System.Management;
    using System.Text;
    using DevLib.DaemonProcess.NativeAPI;

    /// <summary>
    /// Class DaemonProcessHelper.
    /// </summary>
    internal static class DaemonProcessHelper
    {
        /// <summary>
        /// Method GetCommandLineByProcessId.
        /// </summary>
        /// <param name="processId">Process id.</param>
        /// <returns>Command line string.</returns>
        public static string GetCommandLineByProcessId(int processId)
        {
            string result = string.Empty;

            ManagementObjectSearcher managementObjectSearcher = null;

            try
            {
                managementObjectSearcher = new ManagementObjectSearcher(string.Format("SELECT CommandLine FROM Win32_Process WHERE ProcessId = {0}", processId));

                foreach (ManagementObject managementObject in managementObjectSearcher.Get())
                {
                    try
                    {
                        result = managementObject["CommandLine"].ToString();

                        break;
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);
                    }
                }
            }
            catch
            {
                result = NativeMethodsHelper.GetCommandLine(processId);
            }
            finally
            {
                if (managementObjectSearcher != null)
                {
                    managementObjectSearcher.Dispose();
                    managementObjectSearcher = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Method GetCommandLineArguments.
        /// </summary>
        /// <param name="commandLine">Command line string.</param>
        /// <returns>Command line arguments</returns>
        public static List<string> GetCommandLineArguments(string commandLine)
        {
            List<string> result = new List<string>();

            if (string.IsNullOrEmpty(commandLine))
            {
                return result;
            }

            string args = null;

            commandLine = commandLine.Trim();

            if (commandLine.StartsWith("\""))
            {
                int argsIndex = commandLine.IndexOf("\"", 1);
                args = commandLine.Substring(argsIndex + 1, commandLine.Length - argsIndex - 1);
            }
            else
            {
                int argsIndex = commandLine.IndexOf(" ", 0);
                args = commandLine.Substring(argsIndex + 1, commandLine.Length - argsIndex - 1);
            }

            if (string.IsNullOrEmpty(args))
            {
                return result;
            }

            StringBuilder stringBuilder = new StringBuilder();

            uint splitSwitch = 0;

            foreach (char item in args.Trim())
            {
                if (splitSwitch % 2 == 1)
                {
                    if (!item.Equals('\"'))
                    {
                        stringBuilder.Append(item);
                        continue;
                    }
                    else
                    {
                        splitSwitch += 1;
                        result.Add(stringBuilder.ToString());
                        stringBuilder.Remove(0, stringBuilder.Length);
                        continue;
                    }
                }
                else
                {
                    if (item.Equals('\"'))
                    {
                        splitSwitch += 1;
                        continue;
                    }

                    if (!item.Equals(' '))
                    {
                        stringBuilder.Append(item);
                        continue;
                    }
                    else
                    {
                        if (stringBuilder.Length > 0)
                        {
                            result.Add(stringBuilder.ToString());
                            stringBuilder.Remove(0, stringBuilder.Length);
                            continue;
                        }
                    }
                }
            }

            if (stringBuilder.Length > 0)
            {
                result.Add(stringBuilder.ToString());
            }

            return result;
        }

        /// <summary>
        /// Method IsCommandLineArgumentsEquals.
        /// </summary>
        /// <param name="argsA">The first CommandLineArguments.</param>
        /// <param name="argsB">The second CommandLineArguments.</param>
        /// <param name="stringComparison">One of the System.StringComparison values.</param>
        /// <returns>true if two command line arguments are same;otherwise, false.</returns>
        public static bool CommandLineArgumentsEquals(IList<string> argsA, IList<string> argsB, StringComparison stringComparison)
        {
            if (object.ReferenceEquals(argsA, argsB))
            {
                return true;
            }

            if (object.ReferenceEquals(argsA, null) || object.ReferenceEquals(argsB, null))
            {
                return false;
            }

            if (argsA.Count != argsB.Count)
            {
                return false;
            }

            for (int i = 0; i < argsA.Count; i++)
            {
                if (!argsA[i].Trim('\"').Equals(argsB[i].Trim('\"'), stringComparison))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
