//-----------------------------------------------------------------------
// <copyright file="WindowsServiceConsole.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceProcess
{
    using System;
    using System.Globalization;
    using System.Security.Principal;
    using System.ServiceProcess;

    /// <summary>
    /// Class WindowsServiceConsole.
    /// </summary>
    internal static class WindowsServiceConsole
    {
        /// <summary>
        /// Field _args.
        /// </summary>
        private static string[] _args;

        /// <summary>
        /// Field _serviceStatus.
        /// </summary>
        private static ServiceControllerStatus _serviceStatus;

        /// <summary>
        /// Field _windowsService.
        /// </summary>
        private static IWindowsService _windowsService;

        /// <summary>
        /// Method Run.
        /// </summary>
        /// <param name="windowsService">IWindowsService instance.</param>
        /// <param name="args">Command line arguments.</param>
        public static void Run(IWindowsService windowsService, string[] args)
        {
            _windowsService = windowsService;
            _args = args;
            ////_serviceStatus = WindowsServiceBase.ServiceExists(_windowsService.WindowsServiceSetupInfo.ServiceName) ? WindowsServiceBase.GetServiceStatus(_windowsService.WindowsServiceSetupInfo.ServiceName) : ServiceControllerStatus.Stopped;
            _serviceStatus = ServiceControllerStatus.Stopped;

            RunWindowsServiceConsole();
        }

        /// <summary>
        /// Method RunWindowsServiceConsole.
        /// </summary>
        private static void RunWindowsServiceConsole()
        {
            WriteToConsole(ConsoleColor.Yellow, "[Launching Windows Service in console...]");
            WriteServiceInfo();

            bool canContinue = true;

            while (canContinue)
            {
                Console.WriteLine();
                WriteCommandInfo();
                canContinue = HandleConsoleInput(Console.ReadLine());
            }

            try
            {
                WriteToConsole(ConsoleColor.Yellow, "[Windows Service is exiting...]");
                _windowsService.OnStop();
                _windowsService.OnShutdown();
                WriteToConsole(ConsoleColor.Yellow, "[Windows Service has exited.]");
            }
            catch (Exception e)
            {
                ExceptionHandler.Log(e);
            }
        }

        /// <summary>
        /// Method HandleConsoleInput.
        /// </summary>
        /// <param name="input">Console input.</param>
        /// <returns>true if can continue; otherwise, false.</returns>
        private static bool HandleConsoleInput(string input)
        {
            bool canContinue = true;
            ServiceControllerStatus originalStatus = _serviceStatus;

            if (input != null)
            {
                switch (input.ToUpper(CultureInfo.InvariantCulture))
                {
                    case "S":

                        Console.WriteLine();

                        if (_serviceStatus == ServiceControllerStatus.Stopped)
                        {
                            try
                            {
                                _serviceStatus = ServiceControllerStatus.StartPending;
                                WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", _serviceStatus));

                                ////if (!WindowsServiceBase.ServiceExists(_windowsService.WindowsServiceSetupInfo.ServiceName))
                                {
                                    _windowsService.OnStart(_args);
                                    _serviceStatus = ServiceControllerStatus.Running;
                                }
                                ////else
                                ////{
                                ////    WindowsServiceBase.Start(_windowsService.WindowsServiceSetupInfo.ServiceName, _args);
                                ////    _serviceStatus = WindowsServiceBase.GetServiceStatus(_windowsService.WindowsServiceSetupInfo.ServiceName);
                                ////}
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Log(e);
                                _serviceStatus = originalStatus;
                            }
                        }

                        WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", _serviceStatus));

                        break;

                    case "T":

                        Console.WriteLine();

                        if (_serviceStatus == ServiceControllerStatus.Running || _serviceStatus == ServiceControllerStatus.Paused)
                        {
                            try
                            {
                                _serviceStatus = ServiceControllerStatus.StopPending;
                                WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", _serviceStatus));

                                ////if (!WindowsServiceBase.ServiceExists(_windowsService.WindowsServiceSetupInfo.ServiceName))
                                {
                                    _windowsService.OnStop();
                                    _serviceStatus = ServiceControllerStatus.Stopped;
                                }
                                ////else
                                ////{
                                ////    WindowsServiceBase.Stop(_windowsService.WindowsServiceSetupInfo.ServiceName);
                                ////    _serviceStatus = WindowsServiceBase.GetServiceStatus(_windowsService.WindowsServiceSetupInfo.ServiceName);
                                ////}
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Log(e);
                                _serviceStatus = originalStatus;
                            }
                        }

                        WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", _serviceStatus));

                        break;

                    case "P":

                        Console.WriteLine();

                        if (_serviceStatus == ServiceControllerStatus.Running)
                        {
                            try
                            {
                                _serviceStatus = ServiceControllerStatus.PausePending;
                                WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", _serviceStatus));

                                ////if (!WindowsServiceBase.ServiceExists(_windowsService.WindowsServiceSetupInfo.ServiceName))
                                {
                                    _windowsService.OnPause();
                                    _serviceStatus = ServiceControllerStatus.Paused;
                                }
                                ////else
                                ////{
                                ////    WindowsServiceBase.Pause(_windowsService.WindowsServiceSetupInfo.ServiceName);
                                ////    _serviceStatus = WindowsServiceBase.GetServiceStatus(_windowsService.WindowsServiceSetupInfo.ServiceName);
                                ////}
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Log(e);
                                _serviceStatus = originalStatus;
                            }
                        }

                        WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", _serviceStatus));

                        break;

                    case "R":

                        Console.WriteLine();

                        if (_serviceStatus == ServiceControllerStatus.Paused)
                        {
                            try
                            {
                                _serviceStatus = ServiceControllerStatus.ContinuePending;
                                WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", _serviceStatus));

                                ////if (!WindowsServiceBase.ServiceExists(_windowsService.WindowsServiceSetupInfo.ServiceName))
                                {
                                    _windowsService.OnContinue();
                                    _serviceStatus = ServiceControllerStatus.Running;
                                }
                                ////else
                                ////{
                                ////    WindowsServiceBase.Continue(_windowsService.WindowsServiceSetupInfo.ServiceName);
                                ////    _serviceStatus = WindowsServiceBase.GetServiceStatus(_windowsService.WindowsServiceSetupInfo.ServiceName);
                                ////}
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Log(e);
                                _serviceStatus = originalStatus;
                            }
                        }

                        WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", _serviceStatus));

                        break;

                    case "I":

                        Console.WriteLine();

                        if (!WindowsServiceBase.ServiceExists(_windowsService.WindowsServiceSetupInfo.ServiceName))
                        {
                            WindowsServiceInstaller.RuntimeInstall(_windowsService.WindowsServiceSetupInfo);
                        }
                        else
                        {
                            WriteToConsole(ConsoleColor.Red, "The specified service already exists.", true, false);
                        }

                        break;

                    case "U":

                        Console.WriteLine();

                        if (WindowsServiceBase.ServiceExists(_windowsService.WindowsServiceSetupInfo.ServiceName))
                        {
                            WindowsServiceInstaller.RuntimeUninstall(_windowsService.WindowsServiceSetupInfo);
                        }
                        else
                        {
                            WriteToConsole(ConsoleColor.Red, "The specified service does not exist as an installed service.", true, false);
                        }

                        break;

                    case "A":

                        Console.WriteLine();
                        WriteServiceInfo();
                        break;

                    case "Q":

                        canContinue = false;
                        break;

                    case "":
                    case "\n":
                    case "\r\n":
                        break;

                    default:
                        WriteToConsole(ConsoleColor.Red, string.Format("\"{0}\" is not recognized as a valid command.", input), true, false);
                        break;
                }
            }

            return canContinue;
        }

        /// <summary>
        /// Method WriteCommandInfo.
        /// </summary>
        private static void WriteCommandInfo()
        {
            WriteToConsole(ConsoleColor.White, "[S]tart       S[t]op        [P]ause       [R]esume", true, false);
            WriteToConsole(ConsoleColor.White, "[I]nstall     [U]ninstall   St[a]tus      [Q]uit", true, false);
            WriteToConsole(ConsoleColor.White, "Enter:", false, false);
        }

        /// <summary>
        /// Method WriteServiceInfo.
        /// </summary>
        private static void WriteServiceInfo()
        {
            WriteToConsole(ConsoleColor.Yellow, new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) ? "[Run as Administrator]" : "[NOT run as Administrator]", true, false);
            WriteToConsole(ConsoleColor.Yellow, string.Format("[Service Name:] {0}", _windowsService.WindowsServiceSetupInfo.ServiceName), true, false);
            WriteToConsole(ConsoleColor.Yellow, string.Format("[Display Name:] {0}", _windowsService.WindowsServiceSetupInfo.DisplayName), true, false);
            WriteToConsole(ConsoleColor.Yellow, string.Format("[Descriptione:] {0}", _windowsService.WindowsServiceSetupInfo.Description), true, false);
            WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:      ] {0}", _serviceStatus), true, false);
            WriteToConsole(ConsoleColor.Yellow, string.Format("[Installed:   ] {0}", WindowsServiceBase.ServiceExists(_windowsService.WindowsServiceSetupInfo.ServiceName)), true, false);
            WriteToConsole(ConsoleColor.Yellow, string.Format("[Assembly:    ] {0}", _windowsService.WindowsServiceSetupInfo.ServiceAssembly.FullName), true, false);
            WriteToConsole(ConsoleColor.Yellow, string.Format("[AssemblyFile:] {0}", _windowsService.WindowsServiceSetupInfo.ServiceAssembly.Location), true, false);
        }

        /// <summary>
        /// Method WriteToConsole.
        /// </summary>
        /// <param name="foregroundColor">Foreground color of the console.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="withNewLine">Whether followed by the current line terminator.</param>
        /// <param name="withTimestamp">Whether write timestamp.</param>
        private static void WriteToConsole(ConsoleColor foregroundColor, string value, bool withNewLine = true, bool withTimestamp = true)
        {
            ConsoleColor originalForeColor = Console.ForegroundColor;
            ConsoleColor originalBackgroundColor = Console.BackgroundColor;

            if (withNewLine)
            {
                if (withTimestamp)
                {
                    Console.ResetColor();
                    Console.ForegroundColor = foregroundColor;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine(string.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffUTCzzz"), value));
                }
                else
                {
                    Console.ResetColor();
                    Console.ForegroundColor = foregroundColor;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine(value);
                }
            }
            else
            {
                if (withTimestamp)
                {
                    Console.ResetColor();
                    Console.ForegroundColor = foregroundColor;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write(string.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffUTCzzz"), value));
                }
                else
                {
                    Console.ResetColor();
                    Console.ForegroundColor = foregroundColor;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write(value);
                }
            }

            Console.ResetColor();
            Console.ForegroundColor = originalForeColor;
            Console.BackgroundColor = originalBackgroundColor;
        }
    }
}
