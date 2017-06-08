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
    internal class WindowsServiceConsole
    {
        /// <summary>
        /// Field _serviceArgs.
        /// </summary>
        private string[] _serviceArgs;

        /// <summary>
        /// Field _consoleArgs.
        /// </summary>
        private string[] _consoleArgs;

        /// <summary>
        /// Field _isConsoleMode.
        /// </summary>
        private bool _isConsoleMode = true;

        /// <summary>
        /// Field _setupInfo.
        /// </summary>
        private WindowsServiceSetup _setupInfo;

        /// <summary>
        /// Field _consoleStatus.
        /// </summary>
        private ServiceControllerStatus _consoleStatus;

        /// <summary>
        /// Field _windowsService.
        /// </summary>
        private IWindowsService _windowsService;

        /// <summary>
        /// Gets the status of the service.
        /// </summary>
        public ServiceControllerStatus ServiceStatus
        {
            get
            {
                return WindowsServiceBase.ServiceExists(this._setupInfo.ServiceName) ? WindowsServiceBase.GetServiceStatus(this._setupInfo.ServiceName) : ServiceControllerStatus.Stopped;
            }
        }

        /// <summary>
        /// Gets a value indicating whether run service in console mode or not.
        /// </summary>
        public bool IsConsoleMode
        {
            get
            {
                return this._isConsoleMode;
            }

            private set
            {
                this._isConsoleMode = value;
                Console.ResetColor();
                Console.BackgroundColor = value ? ConsoleColor.Black : ConsoleColor.DarkBlue;
            }
        }

        /// <summary>
        /// Method Run.
        /// </summary>
        /// <param name="windowsService">IWindowsService instance.</param>
        /// <param name="serviceArgs">The arguments passed by the service start command.</param>
        /// <param name="isConsoleMode">true to start in console mode; otherwise, start in windows service mode.</param>
        /// <param name="consoleArgs">The arguments for console mode.</param>
        public void Run(IWindowsService windowsService, string[] serviceArgs, bool isConsoleMode = true, string[] consoleArgs = null)
        {
            this._windowsService = windowsService;
            this.IsConsoleMode = isConsoleMode;
            this._serviceArgs = serviceArgs;
            this._consoleArgs = consoleArgs;
            this._consoleStatus = ServiceControllerStatus.Stopped;
            this._setupInfo = windowsService.ServiceSetupInfo.Clone() as WindowsServiceSetup;
            this.RunWindowsServiceConsole();
        }

        /// <summary>
        /// Method RunWindowsServiceConsole.
        /// </summary>
        private void RunWindowsServiceConsole()
        {
            this.WriteToConsole(ConsoleColor.Yellow, "[Launching Windows Service in console...]");
            this.WriteServiceInfo();
            Console.WriteLine();
            this.WriteCommandInfo();

            bool canContinue = true;

            if (this._consoleArgs != null)
            {
                canContinue = this.HandleConsoleInput(string.Join(" ", this._consoleArgs));
            }

            while (canContinue)
            {
                canContinue = this.HandleConsoleInput(Console.ReadLine());
            }

            try
            {
                this.WriteToConsole(ConsoleColor.Yellow, "[Windows Service is exiting...]");
                this._windowsService.OnStop();
                this._windowsService.OnShutdown();
                this.WriteToConsole(ConsoleColor.Yellow, "[Windows Service has exited.]");
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
            }
        }

        /// <summary>
        /// Method HandleConsoleInput.
        /// </summary>
        /// <param name="input">Console input.</param>
        /// <returns>true if can continue; otherwise, false.</returns>
        private bool HandleConsoleInput(string input)
        {
            bool canContinue = true;

            input = input == null ? null : input.Trim();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine();
                this.WriteCommandInfo();
                return canContinue;
            }

            if (input.StartsWith("n ", true, CultureInfo.InvariantCulture) && input.Length > 2)
            {
                this._setupInfo.ServiceName = input.Substring(2);
                this._setupInfo.DisplayName = this._setupInfo.ServiceName;
            }
            else if (input.StartsWith("i ", true, CultureInfo.InvariantCulture) && input.Length > 2)
            {
                this._setupInfo.ServiceName = input.Substring(2);
                this._setupInfo.DisplayName = this._setupInfo.ServiceName;
                this.Install();
            }
            else if (input.StartsWith("u ", true, CultureInfo.InvariantCulture) && input.Length > 2)
            {
                this._setupInfo.ServiceName = input.Substring(2);
                this._setupInfo.DisplayName = this._setupInfo.ServiceName;
                this.Uninstall();
            }
            else
            {
                switch (input.ToLowerInvariant())
                {
                    case "o":
                        this._setupInfo.ServiceName = this._windowsService.ServiceSetupInfo.ServiceName;
                        this._setupInfo.DisplayName = this._windowsService.ServiceSetupInfo.DisplayName;
                        Console.WriteLine();
                        this.WriteCommandInfo();
                        break;

                    case "s":
                        this.Start();
                        Console.WriteLine();
                        this.WriteCommandInfo();
                        break;

                    case "t":
                        this.Stop();
                        Console.WriteLine();
                        this.WriteCommandInfo();
                        break;

                    case "p":
                        this.Pause();
                        Console.WriteLine();
                        this.WriteCommandInfo();
                        break;

                    case "r":
                        this.Resume();
                        Console.WriteLine();
                        this.WriteCommandInfo();
                        break;

                    case "e":
                        this.Restart();
                        Console.WriteLine();
                        this.WriteCommandInfo();
                        break;

                    case "c":
                        this.IsConsoleMode = true;
                        Console.WriteLine();
                        this.WriteCommandInfo();
                        break;

                    case "v":
                        this.IsConsoleMode = false;
                        Console.WriteLine();
                        this.WriteCommandInfo();
                        break;

                    case "i":
                        this.Install();
                        Console.WriteLine();
                        this.WriteCommandInfo();
                        break;

                    case "u":
                        this.Uninstall();
                        Console.WriteLine();
                        this.WriteCommandInfo();
                        break;

                    case "a":
                        Console.WriteLine();
                        this.WriteServiceInfo();
                        Console.WriteLine();
                        this.WriteCommandInfo();
                        break;

                    case "q":
                        canContinue = false;
                        break;

                    case "\r":
                    case "\n":
                    case "\r\n":
                        this.WriteCommandInfo();
                        break;

                    case "cls":
                        Console.Clear();
                        break;

                    default:
                        this.WriteToConsole(ConsoleColor.Red, string.Format("\"{0}\" is not recognized as a valid command.", input), true, false);
                        Console.WriteLine();
                        this.WriteCommandInfo();
                        break;
                }
            }

            return canContinue;
        }

        /// <summary>
        /// Method Start.
        /// </summary>
        private void Start()
        {
            ServiceControllerStatus originalConsoleStatus = this._consoleStatus;

            if (this.IsConsoleMode)
            {
                if (this._consoleStatus == ServiceControllerStatus.Stopped)
                {
                    this._consoleStatus = ServiceControllerStatus.StartPending;
                    this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this._consoleStatus.ToString()));

                    try
                    {
                        this._windowsService.OnStart(this._serviceArgs);
                        this._consoleStatus = ServiceControllerStatus.Running;
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                        this._consoleStatus = originalConsoleStatus;
                    }
                }

                this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this._consoleStatus.ToString()));
            }
            else
            {
                if (WindowsServiceBase.ServiceExists(this._setupInfo.ServiceName))
                {
                    if (this.ServiceStatus == ServiceControllerStatus.Stopped)
                    {
                        this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this.ServiceStatus.ToString()));
                        WindowsServiceBase.Start(this._setupInfo.ServiceName, this._serviceArgs);
                    }

                    this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this.ServiceStatus.ToString()));
                }
                else
                {
                    this.WriteToConsole(ConsoleColor.Red, "The specified service does not exist as an installed service.", true, false);
                }
            }
        }

        /// <summary>
        /// Method Stop.
        /// </summary>
        private void Stop()
        {
            ServiceControllerStatus originalConsoleStatus = this._consoleStatus;

            if (this.IsConsoleMode)
            {
                if (this._consoleStatus == ServiceControllerStatus.Running || this._consoleStatus == ServiceControllerStatus.Paused)
                {
                    this._consoleStatus = ServiceControllerStatus.StopPending;
                    this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this._consoleStatus.ToString()));

                    try
                    {
                        this._windowsService.OnStop();
                        this._consoleStatus = ServiceControllerStatus.Stopped;
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                        this._consoleStatus = originalConsoleStatus;
                    }
                }

                this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this._consoleStatus.ToString()));
            }
            else
            {
                if (WindowsServiceBase.ServiceExists(this._setupInfo.ServiceName))
                {
                    if (this.ServiceStatus == ServiceControllerStatus.Running || this.ServiceStatus == ServiceControllerStatus.Paused)
                    {
                        this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this.ServiceStatus.ToString()));
                        WindowsServiceBase.Stop(this._setupInfo.ServiceName);
                    }

                    this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this.ServiceStatus.ToString()));
                }
                else
                {
                    this.WriteToConsole(ConsoleColor.Red, "The specified service does not exist as an installed service.", true, false);
                }
            }
        }

        /// <summary>
        /// Method Pause.
        /// </summary>
        private void Pause()
        {
            ServiceControllerStatus originalConsoleStatus = this._consoleStatus;

            if (this.IsConsoleMode)
            {
                if (this._consoleStatus == ServiceControllerStatus.Running)
                {
                    this._consoleStatus = ServiceControllerStatus.PausePending;
                    this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this._consoleStatus.ToString()));

                    try
                    {
                        this._windowsService.OnPause();
                        this._consoleStatus = ServiceControllerStatus.Paused;
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                        this._consoleStatus = originalConsoleStatus;
                    }
                }

                this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this._consoleStatus.ToString()));
            }
            else
            {
                if (WindowsServiceBase.ServiceExists(this._setupInfo.ServiceName))
                {
                    if (this.ServiceStatus == ServiceControllerStatus.Running)
                    {
                        this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this.ServiceStatus.ToString()));
                        WindowsServiceBase.Pause(this._setupInfo.ServiceName);
                    }

                    this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this.ServiceStatus.ToString()));
                }
                else
                {
                    this.WriteToConsole(ConsoleColor.Red, "The specified service does not exist as an installed service.", true, false);
                }
            }
        }

        /// <summary>
        /// Method Resume.
        /// </summary>
        private void Resume()
        {
            ServiceControllerStatus originalConsoleStatus = this._consoleStatus;

            if (this.IsConsoleMode)
            {
                if (this._consoleStatus == ServiceControllerStatus.Paused)
                {
                    this._consoleStatus = ServiceControllerStatus.ContinuePending;
                    this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this._consoleStatus.ToString()));

                    try
                    {
                        this._windowsService.OnContinue();
                        this._consoleStatus = ServiceControllerStatus.Running;
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Log(e);
                        this._consoleStatus = originalConsoleStatus;
                    }
                }

                this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this._consoleStatus.ToString()));
            }
            else
            {
                if (WindowsServiceBase.ServiceExists(this._setupInfo.ServiceName))
                {
                    if (this.ServiceStatus == ServiceControllerStatus.Paused)
                    {
                        this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this.ServiceStatus.ToString()));
                        WindowsServiceBase.Continue(this._setupInfo.ServiceName);
                    }

                    this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this.ServiceStatus.ToString()));
                }
                else
                {
                    this.WriteToConsole(ConsoleColor.Red, "The specified service does not exist as an installed service.", true, false);
                }
            }
        }

        /// <summary>
        /// Method Restart.
        /// </summary>
        private void Restart()
        {
            ServiceControllerStatus originalConsoleStatus = this._consoleStatus;

            if (this.IsConsoleMode)
            {
                this._consoleStatus = ServiceControllerStatus.StartPending;
                this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this._consoleStatus.ToString()));

                try
                {
                    this._windowsService.OnStop();
                    originalConsoleStatus = ServiceControllerStatus.Stopped;

                    this._windowsService.OnStart(this._serviceArgs);
                    this._consoleStatus = ServiceControllerStatus.Running;
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                    this._consoleStatus = originalConsoleStatus;
                }

                this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this._consoleStatus.ToString()));
            }
            else
            {
                if (WindowsServiceBase.ServiceExists(this._setupInfo.ServiceName))
                {
                    this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this.ServiceStatus.ToString()));
                    WindowsServiceBase.Stop(this._setupInfo.ServiceName);
                    WindowsServiceBase.Start(this._setupInfo.ServiceName, this._serviceArgs);
                    this.WriteToConsole(ConsoleColor.Yellow, string.Format("[Status:] {0}", this.ServiceStatus.ToString()));
                }
                else
                {
                    this.WriteToConsole(ConsoleColor.Red, "The specified service does not exist as an installed service.", true, false);
                }
            }
        }

        /// <summary>
        /// Method Install.
        /// </summary>
        private void Install()
        {
            if (!WindowsServiceBase.ServiceExists(this._setupInfo.ServiceName))
            {
                WindowsServiceInstaller.RuntimeInstall(this._setupInfo);
            }
            else
            {
                this.WriteToConsole(ConsoleColor.Red, "The specified service already exists.", true, false);
            }
        }

        /// <summary>
        /// Method Uninstall.
        /// </summary>
        private void Uninstall()
        {
            if (WindowsServiceBase.ServiceExists(this._setupInfo.ServiceName))
            {
                WindowsServiceInstaller.RuntimeUninstall(this._setupInfo);
            }
            else
            {
                this.WriteToConsole(ConsoleColor.Red, "The specified service does not exist as an installed service.", true, false);
            }
        }

        /// <summary>
        /// Method WriteCommandInfo.
        /// </summary>
        private void WriteCommandInfo()
        {
            this.WriteToConsole(ConsoleColor.White, string.Format("[O]riginal: {0}", this._windowsService.ServiceSetupInfo.ServiceName).PadRight(70), true, false);
            this.WriteToConsole(ConsoleColor.White, string.Format("[N]ame:     {0}", this._setupInfo.ServiceName).PadRight(70), true, false);
            this.WriteToConsole(ConsoleColor.White, string.Format("Mode:       {0}", this.IsConsoleMode ? "Console" : "Service").PadRight(70), true, false);
            this.WriteToConsole(ConsoleColor.White, string.Format("Status:     {0}", this.IsConsoleMode ? this._consoleStatus : this.ServiceStatus).PadRight(70), true, false);
            this.WriteToConsole(ConsoleColor.White, string.Format("Installed:  {0}", WindowsServiceBase.ServiceExists(this._setupInfo.ServiceName)).PadRight(70), true, false);
            this.WriteToConsole(ConsoleColor.White, " ".PadRight(70), true, false);
            this.WriteToConsole(ConsoleColor.White, "[S]tart       S[t]op        [P]ause       [R]esume      R[e]start     ", true, false);
            this.WriteToConsole(ConsoleColor.White, string.Format("{0}[I]nstall     [U]ninstall   St[a]tus      [Q]uit        ", this.IsConsoleMode ? "Ser[v]ice     " : "[C]onsole     "), true, false);
            this.WriteToConsole(ConsoleColor.White, " ".PadRight(70), true, false);
            this.WriteToConsole(ConsoleColor.White, "Enter:", false, false);
        }

        /// <summary>
        /// Method WriteServiceInfo.
        /// </summary>
        private void WriteServiceInfo()
        {
            this.WriteToConsole(ConsoleColor.Yellow, string.Format("Administrator: {0}", new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)).PadRight(70), true, false);
            this.WriteToConsole(ConsoleColor.Yellow, string.Format("Service Name:  {0}", this._setupInfo.ServiceName).PadRight(70), true, false);
            this.WriteToConsole(ConsoleColor.Yellow, string.Format("Display Name:  {0}", this._setupInfo.DisplayName).PadRight(70), true, false);
            this.WriteToConsole(ConsoleColor.Yellow, string.Format("Mode:          {0}", this.IsConsoleMode ? "Console" : "Service").PadRight(70), true, false);
            this.WriteToConsole(ConsoleColor.Yellow, string.Format("Status:        {0}", this.IsConsoleMode ? this._consoleStatus : this.ServiceStatus).PadRight(70), true, false);
            this.WriteToConsole(ConsoleColor.Yellow, string.Format("Installed:     {0}", WindowsServiceBase.ServiceExists(this._setupInfo.ServiceName)).PadRight(70), true, false);
            this.WriteToConsole(ConsoleColor.Yellow, "Description:".PadRight(70), true, false);
            this.WriteToConsole(ConsoleColor.Yellow, "    " + this._setupInfo.Description.PadRight(66), true, false);
            this.WriteToConsole(ConsoleColor.Yellow, "Assembly:".PadRight(70), true, false);
            this.WriteToConsole(ConsoleColor.Yellow, "    " + this._setupInfo.ServiceAssembly.FullName.PadRight(66), true, false);
            this.WriteToConsole(ConsoleColor.Yellow, "Assembly File: ".PadRight(70), true, false);
            this.WriteToConsole(ConsoleColor.Yellow, "    " + this._setupInfo.ServiceAssembly.Location.PadRight(66), true, false);
        }

        /// <summary>
        /// Method WriteToConsole.
        /// </summary>
        /// <param name="foregroundColor">Foreground color of the console.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="withNewLine">Whether followed by the current line terminator.</param>
        /// <param name="withTimestamp">Whether write timestamp.</param>
        private void WriteToConsole(ConsoleColor foregroundColor, string value, bool withNewLine = true, bool withTimestamp = true)
        {
            ConsoleColor originalForeColor = Console.ForegroundColor;

            if (withNewLine)
            {
                if (withTimestamp)
                {
                    Console.ForegroundColor = foregroundColor;
                    Console.WriteLine(string.Format("[{0}] {1}", DateTimeOffset.Now.ToString("o", CultureInfo.InvariantCulture), value));
                }
                else
                {
                    Console.ForegroundColor = foregroundColor;
                    Console.WriteLine(value);
                }
            }
            else
            {
                if (withTimestamp)
                {
                    Console.ForegroundColor = foregroundColor;
                    Console.Write(string.Format("[{0}] {1}", DateTimeOffset.Now.ToString("o", CultureInfo.InvariantCulture), value));
                }
                else
                {
                    Console.ForegroundColor = foregroundColor;
                    Console.Write(value);
                }
            }

            Console.ForegroundColor = originalForeColor;
        }
    }
}
