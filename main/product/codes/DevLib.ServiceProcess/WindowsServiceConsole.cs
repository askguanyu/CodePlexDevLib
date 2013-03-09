//-----------------------------------------------------------------------
// <copyright file="WindowsServiceConsole.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceProcess
{
    using System;
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
            _serviceStatus = ServiceControllerStatus.Stopped;

            RunWindowsServiceConsole();
        }

        /// <summary>
        /// Method RunWindowsServiceConsole.
        /// </summary>
        private static void RunWindowsServiceConsole()
        {
            WriteToConsole(true, ConsoleColor.Yellow, "[Launch Windows Service...]");
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
                Console.WriteLine();
                _windowsService.OnStop();
                _windowsService.OnShutdown();
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
                switch (input.ToUpper())
                {
                    case "S":
                        Console.WriteLine();

                        if (_serviceStatus == ServiceControllerStatus.Stopped)
                        {
                            try
                            {
                                _serviceStatus = ServiceControllerStatus.StartPending;
                                WriteToConsole(true, ConsoleColor.Yellow, "[Status:] {0}", _serviceStatus);
                                _windowsService.OnStart(_args);
                                _serviceStatus = ServiceControllerStatus.Running;
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Log(e);
                                _serviceStatus = originalStatus;
                            }
                        }

                        WriteToConsole(true, ConsoleColor.Yellow, "[Status:] {0}", _serviceStatus);

                        break;

                    case "T":
                        Console.WriteLine();

                        if (_serviceStatus == ServiceControllerStatus.Running || _serviceStatus == ServiceControllerStatus.Paused)
                        {
                            try
                            {
                                _serviceStatus = ServiceControllerStatus.StopPending;
                                WriteToConsole(true, ConsoleColor.Yellow, "[Status:] {0}", _serviceStatus);
                                _windowsService.OnStop();
                                _serviceStatus = ServiceControllerStatus.Stopped;
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Log(e);
                                _serviceStatus = originalStatus;
                            }
                        }

                        WriteToConsole(true, ConsoleColor.Yellow, "[Status:] {0}", _serviceStatus);

                        break;

                    case "P":
                        Console.WriteLine();

                        if (_serviceStatus == ServiceControllerStatus.Running)
                        {
                            try
                            {
                                _serviceStatus = ServiceControllerStatus.PausePending;
                                WriteToConsole(true, ConsoleColor.Yellow, "[Status:] {0}", _serviceStatus);
                                _windowsService.OnPause();
                                _serviceStatus = ServiceControllerStatus.Paused;
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Log(e);
                                _serviceStatus = originalStatus;
                            }
                        }

                        WriteToConsole(true, ConsoleColor.Yellow, "[Status:] {0}", _serviceStatus);

                        break;

                    case "R":
                        Console.WriteLine();

                        if (_serviceStatus == ServiceControllerStatus.Paused)
                        {
                            try
                            {
                                _serviceStatus = ServiceControllerStatus.ContinuePending;
                                WriteToConsole(true, ConsoleColor.Yellow, "[Status:] {0}", _serviceStatus);
                                _windowsService.OnContinue();
                                _serviceStatus = ServiceControllerStatus.Running;
                            }
                            catch (Exception e)
                            {
                                ExceptionHandler.Log(e);
                                _serviceStatus = originalStatus;
                            }
                        }

                        WriteToConsole(true, ConsoleColor.Yellow, "[Status:] {0}", _serviceStatus);

                        break;

                    case "I":
                        Console.WriteLine();

                        if (!WindowsServiceBase.ServiceExists(_windowsService.WindowsServiceSetupInfo.ServiceName))
                        {
                            WindowsServiceInstaller.RuntimeInstall(_windowsService.WindowsServiceSetupInfo);
                        }
                        else
                        {
                            WriteToConsole(true, ConsoleColor.Red, "The specified service already exists.");
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
                            WriteToConsole(true, ConsoleColor.Red, "The specified service does not exist as an installed service.");
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
                        break;

                    default:
                        WriteToConsole(true, ConsoleColor.Red, "\"{0}\" is not recognized as a valid command.", input);
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
            WriteToConsole(true, ConsoleColor.White, "[S]tart       S[t]op        [P]ause       [R]esume");
            WriteToConsole(true, ConsoleColor.White, "[I]nstall     [U]ninstall   St[a]tus      [Q]uit");
            WriteToConsole(false, ConsoleColor.White, "Enter:");
        }

        /// <summary>
        /// Method WriteServiceInfo.
        /// </summary>
        private static void WriteServiceInfo()
        {
            WriteToConsole(true, ConsoleColor.Yellow, new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) ? "[Run as Administrator]" : "[NOT run as Administrator]");
            WriteToConsole(true, ConsoleColor.Yellow, "[Service Name:] {0}", _windowsService.WindowsServiceSetupInfo.ServiceName);
            WriteToConsole(true, ConsoleColor.Yellow, "[Display Name:] {0}", _windowsService.WindowsServiceSetupInfo.DisplayName);
            WriteToConsole(true, ConsoleColor.Yellow, "[Descriptione:] {0}", _windowsService.WindowsServiceSetupInfo.Description);
            WriteToConsole(true, ConsoleColor.Yellow, "[Status:      ] {0}", _serviceStatus);
            WriteToConsole(true, ConsoleColor.Yellow, "[Installed:   ] {0}", WindowsServiceBase.ServiceExists(_windowsService.WindowsServiceSetupInfo.ServiceName));
            WriteToConsole(true, ConsoleColor.Yellow, "[Assembly:    ] {0}", _windowsService.WindowsServiceSetupInfo.ServiceAssembly.FullName);
        }

        /// <summary>
        /// Method WriteToConsole.
        /// </summary>
        /// <param name="withNewLine">Whether followed by the current line terminator.</param>
        /// <param name="foregroundColor">Foreground color of the console.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="formatArguments">An array of objects to write using format.</param>
        private static void WriteToConsole(bool withNewLine, ConsoleColor foregroundColor, string format, params object[] formatArguments)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;

            if (withNewLine)
            {
                Console.WriteLine(format, formatArguments);
            }
            else
            {
                Console.Write(format, formatArguments);
            }

            Console.ForegroundColor = originalColor;
        }
    }
}
