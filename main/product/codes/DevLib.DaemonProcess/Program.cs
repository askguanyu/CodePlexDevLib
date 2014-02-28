//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DaemonProcess
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.ServiceProcess;
    using System.Threading;

    /// <summary>
    /// Class Program.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1400:AccessModifierMustBeDeclared", Justification = "Reviewed.")]
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
            //// args[1] = process id | -stop         : protected process id | stop protecting
            //// args[2] = delay seconds              : protected process delay start seconds
            //// args[3] = service | process          : protected process mode
            //// args[4] = entry file | service name  : protected process entry point
            //// args[5] = args                       : protected process args

            if (args == null || args.Length == 0)
            {
                ExceptionHandler.Log(new ArgumentNullException("args"));

                Console.WriteLine(
@"args[0] = guid                       : daemon process guid
args[1] = process id | -stop         : protected process id | stop protecting
args[2] = delay seconds              : protected process delay start seconds
args[3] = service | process          : protected process mode
args[4] = entry file | service name  : protected process entry point
args[5] = args                       : protected process args");

                Environment.Exit(-1);
            }

            if (args.Length == 2)
            {
                string daemonProcessGuidString = args[0];
                string daemonProcessStopString = args[1];

                if (!daemonProcessStopString.Equals("-stop", StringComparison.OrdinalIgnoreCase))
                {
                    Environment.Exit(-1);
                }

                try
                {
                    Guid daemonProcessGuid = new Guid(daemonProcessGuidString);

                    Console.WriteLine("Stopping protecting {0}", daemonProcessGuidString);

                    DaemonProcessManager.StopProtect(daemonProcessGuid);

                    Console.WriteLine("Stopped protecting {0}", daemonProcessGuidString);

                    Environment.Exit(-1);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);

                    Environment.Exit(-1);
                }
            }

            if (args.Length < 4)
            {
                ExceptionHandler.Log(new ArgumentOutOfRangeException("args"));

                Environment.Exit(-1);
            }

            int protectedProcessId = -1;

            int.TryParse(args[1], NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out protectedProcessId);

            int protectedProcessDelaySeconds = 5;

            try
            {
                int.TryParse(args[2], NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out protectedProcessDelaySeconds);

                if (protectedProcessDelaySeconds < 0)
                {
                    protectedProcessDelaySeconds = 0;
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
            }

            string protectedProcessMode = args[3];

            string protectedProcessEntryPoint = args[4];

            string[] protectedProcessArgs = null;

            if (args.Length > 5)
            {
                protectedProcessArgs = new string[args.Length - 5];

                Array.Copy(args, 5, protectedProcessArgs, 0, protectedProcessArgs.Length);
            }

            switch (protectedProcessMode.ToLowerInvariant())
            {
                case "service":

                    try
                    {
                        ServiceController[] services = ServiceController.GetServices();

                        bool isServiceExist = false;

                        foreach (var item in services)
                        {
                            try
                            {
                                if (protectedProcessEntryPoint.Equals(item.ServiceName, StringComparison.OrdinalIgnoreCase))
                                {
                                    isServiceExist = true;

                                    break;
                                }
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Log(e);
                            }
                        }

                        if (!isServiceExist)
                        {
                            Environment.Exit(-1);
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);

                        Environment.Exit(-1);
                    }

                    ServiceController serviceController = null;

                    try
                    {
                        serviceController = new ServiceController(protectedProcessEntryPoint);
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Log(e);

                        Environment.Exit(-1);
                    }

                    while (true)
                    {
                        try
                        {
                            serviceController.WaitForStatus(ServiceControllerStatus.Stopped);

                            Thread.Sleep(TimeSpan.FromSeconds(protectedProcessDelaySeconds));

                            serviceController.Refresh();

                            if (serviceController.Status == ServiceControllerStatus.Stopped)
                            {
                                if (protectedProcessArgs == null || protectedProcessArgs.Length < 1)
                                {
                                    serviceController.Start();
                                }
                                else
                                {
                                    serviceController.Start(protectedProcessArgs);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            ExceptionHandler.Log(e);
                        }
                    }

                case "process":

                    if (protectedProcessArgs != null && protectedProcessArgs.Length > 0)
                    {
                        for (int i = 0; i < protectedProcessArgs.Length; i++)
                        {
                            protectedProcessArgs[i] = string.Format("\"{0}\"", protectedProcessArgs[i]);
                        }
                    }

                    Process protectedProcess = null;

                    if (protectedProcessId > 0)
                    {
                        try
                        {
                            protectedProcess = Process.GetProcessById(protectedProcessId);
                        }
                        catch (Exception e)
                        {
                            ExceptionHandler.Log(e);
                        }
                    }

                    while (true)
                    {
                        if (protectedProcess != null && !protectedProcess.HasExited)
                        {
                            try
                            {
                                protectedProcess.WaitForExit();

                                Thread.Sleep(TimeSpan.FromSeconds(protectedProcessDelaySeconds));

                                protectedProcess = null;
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Log(e);
                            }
                        }

                        foreach (Process item in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(protectedProcessEntryPoint)))
                        {
                            try
                            {
                                if (item.MainModule.FileName.Equals(protectedProcessEntryPoint, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (protectedProcessArgs == null || protectedProcessArgs.Length < 1)
                                    {
                                        protectedProcess = item;

                                        break;
                                    }
                                    else
                                    {
                                        List<string> commandLineArguments = DaemonProcessHelper.GetCommandLineArguments(DaemonProcessHelper.GetCommandLineByProcessId(item.Id));

                                        if (DaemonProcessHelper.CommandLineArgumentsEquals(commandLineArguments, protectedProcessArgs, StringComparison.OrdinalIgnoreCase))
                                        {
                                            protectedProcess = item;

                                            break;
                                        }
                                    }
                                }
                            }
                            catch (Win32Exception)
                            {
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Log(e);
                            }
                        }

                        if (protectedProcess == null)
                        {
                            try
                            {
                                ProcessStartInfo startInfo = new ProcessStartInfo();
                                startInfo.FileName = Path.GetFullPath(protectedProcessEntryPoint);
                                startInfo.Arguments = (protectedProcessArgs == null || protectedProcessArgs.Length < 1) ? string.Empty : string.Join(" ", protectedProcessArgs);
                                startInfo.Verb = "runas";
                                startInfo.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(protectedProcessEntryPoint));
                                protectedProcess = Process.Start(startInfo);
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Log(e);
                            }
                        }
                    }

                default:
                    Environment.Exit(-1);
                    break;
            }
        }
    }
}
