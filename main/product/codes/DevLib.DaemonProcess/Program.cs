//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DaemonProcess
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.ServiceProcess;
    using System.Threading;

    /// <summary>
    /// Class Program.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Method Main, entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        static void Main(string[] args)
        {
            //// args[0] = guid                       : daemon process guid
            //// args[1] = process id                 : protected process id
            //// args[2] = service | process          : protected process mode
            //// args[3] = entry file | service name  : protected process entry point / protected process service name
            //// args[4] = delay seconds              : protected process delay start seconds
            //// args[5] = args                       : protected process args

            if (args == null || args.Length < 4)
            {
                return;
            }

            int protectedProcessId = -1;

            int.TryParse(args[1], NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out protectedProcessId);

            string protectedProcessMode = args[2];

            string protectedProcessEntryPoint = args[3];

            int protectedProcessDelaySeconds = 3;

            try
            {
                int.TryParse(args[4], NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out protectedProcessDelaySeconds);

                if (protectedProcessDelaySeconds < 0)
                {
                    protectedProcessDelaySeconds = 0;
                }
            }
            catch
            {
            }

            string[] protectedProcessArgs = null;

            if (args.Length > 5)
            {
                protectedProcessArgs = new string[args.Length - 5];

                Array.Copy(args, 5, protectedProcessArgs, 0, protectedProcessArgs.Length);
            }

            switch (protectedProcessMode.ToLowerInvariant())
            {
                case "service":

                    ServiceController[] services = ServiceController.GetServices();

                    bool isServiceExist = false;

                    foreach (var item in services)
                    {
                        if (protectedProcessEntryPoint.Equals(item.ServiceName, StringComparison.OrdinalIgnoreCase))
                        {
                            isServiceExist = true;
                            break;
                        }
                    }

                    if (!isServiceExist)
                    {
                        break;
                    }

                    ServiceController serviceController = new ServiceController(protectedProcessEntryPoint);

                    while (true)
                    {
                        serviceController.WaitForStatus(ServiceControllerStatus.Stopped);

                        Thread.Sleep(TimeSpan.FromSeconds(protectedProcessDelaySeconds));

                        try
                        {
                            serviceController.Refresh();

                            if (serviceController.Status == ServiceControllerStatus.Stopped)
                            {
                                if (protectedProcessArgs == null || protectedProcessArgs.Length < 1)
                                {
                                    serviceController.Start();
                                }
                                else
                                {
                                    serviceController.Start(args);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            ExceptionHandler.Log(e);
                        }
                    }

                case "process":

                    Process protectedProcess = null;

                    if (protectedProcessId > 0)
                    {
                        try
                        {
                            protectedProcess = Process.GetProcessById(protectedProcessId);
                        }
                        catch
                        {
                        }
                    }

                    while (true)
                    {
                        if (protectedProcess == null)
                        {
                            foreach (Process item in Process.GetProcesses())
                            {
                                try
                                {
                                    if (item.MainModule.FileName.Equals(protectedProcessEntryPoint, StringComparison.OrdinalIgnoreCase))
                                    {
                                        protectedProcess = item;

                                        break;
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }

                        if (protectedProcess != null && !protectedProcess.HasExited)
                        {
                            protectedProcess.WaitForExit();
                        }

                        Thread.Sleep(TimeSpan.FromSeconds(protectedProcessDelaySeconds));

                        try
                        {
                            if (protectedProcessArgs == null || protectedProcessArgs.Length < 1)
                            {
                                protectedProcess = Process.Start(protectedProcessEntryPoint);
                            }
                            else
                            {
                                protectedProcess = Process.Start(protectedProcessEntryPoint, string.Join(" ", protectedProcessArgs));
                            }
                        }
                        catch (Exception e)
                        {
                            ExceptionHandler.Log(e);
                        }
                    }

                default:
                    break;
            }
        }
    }
}
