//-----------------------------------------------------------------------
// <copyright file="DaemonProcessManager.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DaemonProcess
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Management;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;
    using System.Threading;

    /// <summary>
    /// Class DaemonProcessManager.
    /// </summary>
    public static class DaemonProcessManager
    {
        /// <summary>
        /// Start to protect current process.
        /// </summary>
        /// <param name="daemonProcessGuid">Daemon process Guid.</param>
        /// <param name="processMode">Current process mode.</param>
        /// <param name="delaySeconds">Restart delay time in seconds.</param>
        /// <param name="args">Arguments for restart current process.</param>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public static void StartSelfProtect(Guid daemonProcessGuid, ProcessMode processMode, int delaySeconds = 0, params string[] args)
        {
            string entryPoint = Assembly.GetEntryAssembly().Location;

            if (processMode == ProcessMode.Service)
            {
                ManagementObjectSearcher managementObjectSearcher = null;

                try
                {
                    managementObjectSearcher = new ManagementObjectSearcher(string.Format("SELECT Name FROM Win32_Service WHERE PathName LIKE \"%{0}%\" ", entryPoint));

                    foreach (ManagementObject managementObject in managementObjectSearcher.Get())
                    {
                        try
                        {
                            entryPoint = managementObject["Name"].ToString();

                            break;
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    if (managementObjectSearcher != null)
                    {
                        managementObjectSearcher.Dispose();
                        managementObjectSearcher = null;
                    }
                }
            }

            StartProtect(daemonProcessGuid, Process.GetCurrentProcess().Id, processMode, entryPoint, delaySeconds, args);
        }

        /// <summary>
        /// Start to protect process.
        /// </summary>
        /// <param name="daemonProcessGuid">Daemon process Guid.</param>
        /// <param name="protectedProcessId">Protected process Id.</param>
        /// <param name="processMode">Protected process mode.</param>
        /// <param name="entryPoint">Protected process entry point. Executable file or windows service name.</param>
        /// <param name="delaySeconds">Restart delay time in seconds.</param>
        /// <param name="args">Arguments for restart protected process.</param>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public static void StartProtect(Guid daemonProcessGuid, int protectedProcessId, ProcessMode processMode, string entryPoint, int delaySeconds = 0, params string[] args)
        {
            string daemonProcessFullPath = Assembly.GetExecutingAssembly().Location;
            string daemonProcessName = Assembly.GetExecutingAssembly().GetName().Name;
            string daemonProcessFileName = Path.GetFileName(daemonProcessFullPath);

            List<string> daemonProcessArgs = new List<string>();
            daemonProcessArgs.Add(daemonProcessGuid.ToString());
            daemonProcessArgs.Add(protectedProcessId.ToString());
            daemonProcessArgs.Add(processMode.ToString());
            daemonProcessArgs.Add(entryPoint);
            daemonProcessArgs.Add(delaySeconds.ToString());
            daemonProcessArgs.AddRange(args);

            ProcessStartInfo startInfo = new ProcessStartInfo(daemonProcessFullPath);
            startInfo.Arguments = string.Join(" ", daemonProcessArgs.ToArray());
            startInfo.CreateNoWindow = true;
            startInfo.ErrorDialog = false;
            startInfo.UseShellExecute = false;

            Process daemonProcess = null;

            int exitCode = 0;

            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    if (daemonProcess == null)
                    {
                        foreach (Process item in Process.GetProcesses())
                        {
                            try
                            {
                                if (item.ProcessName.Equals(daemonProcessName, StringComparison.OrdinalIgnoreCase))
                                {
                                    using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(string.Format("SELECT CommandLine FROM Win32_Process WHERE ProcessId = {0}", item.Id)))
                                    {
                                        foreach (ManagementObject managementObject in managementObjectSearcher.Get())
                                        {
                                            try
                                            {
                                                if (Regex.Match(managementObject["CommandLine"].ToString(), string.Format("{0}\" {1}", daemonProcessFileName, daemonProcessGuid.ToString()), RegexOptions.IgnoreCase).Success)
                                                {
                                                    daemonProcess = item;
                                                    break;
                                                }
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                }
                            }
                            catch
                            {
                            }

                            if (daemonProcess != null)
                            {
                                break;
                            }
                        }
                    }

                    if (daemonProcess != null)
                    {
                        if (!daemonProcess.HasExited)
                        {
                            daemonProcess.WaitForExit();
                        }

                        try
                        {
                            exitCode = daemonProcess.ExitCode;
                        }
                        catch
                        {
                        }

                        if (exitCode == -1)
                        {
                            break;
                        }
                    }

                    try
                    {
                        daemonProcess = Process.Start(startInfo);
                    }
                    catch
                    {
                    }
                }

                if (daemonProcess != null)
                {
                    daemonProcess.Dispose();
                    daemonProcess = null;
                }
            });

            thread.IsBackground = true;

            try
            {
                thread.Start();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Stop protecting.
        /// </summary>
        /// <param name="daemonProcessGuid">Daemon process Guid.</param>
        public static void StopProtect(Guid daemonProcessGuid)
        {
            StopProtectHelper(daemonProcessGuid);

            Thread.Sleep(250);

            StopProtectHelper(daemonProcessGuid);
        }

        /// <summary>
        /// Method StopProtectHelper.
        /// </summary>
        /// <param name="daemonProcessGuid">Daemon process Guid.</param>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        private static void StopProtectHelper(Guid daemonProcessGuid)
        {
            string daemonProcessFullPath = Assembly.GetExecutingAssembly().Location;
            string daemonProcessName = Assembly.GetExecutingAssembly().GetName().Name;
            string daemonProcessFileName = Path.GetFileName(daemonProcessFullPath);

            Process daemonProcess = null;

            foreach (Process item in Process.GetProcesses())
            {
                try
                {
                    if (item.ProcessName.Equals(daemonProcessName, StringComparison.OrdinalIgnoreCase))
                    {
                        using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(string.Format("SELECT CommandLine FROM Win32_Process WHERE ProcessId = {0}", item.Id)))
                        {
                            foreach (ManagementObject managementObject in managementObjectSearcher.Get())
                            {
                                try
                                {
                                    if (Regex.Match(managementObject["CommandLine"].ToString(), string.Format("{0}\" {1}", daemonProcessFileName, daemonProcessGuid.ToString()), RegexOptions.IgnoreCase).Success)
                                    {
                                        daemonProcess = item;
                                        break;
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }
                catch
                {
                }

                if (daemonProcess != null)
                {
                    break;
                }
            }

            if (daemonProcess != null)
            {
                try
                {
                    daemonProcess.Kill();
                }
                catch
                {
                }
                finally
                {
                    daemonProcess.Dispose();
                    daemonProcess = null;
                }
            }
        }
    }
}
