//-----------------------------------------------------------------------
// <copyright file="WindowsServiceConcrete.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceProcess
{
    using System.ServiceProcess;

    /// <summary>
    /// Class WindowsServiceConcrete.
    /// </summary>
    internal class WindowsServiceConcrete : ServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsServiceConcrete" /> class.
        /// </summary>
        /// <param name="windowsService">Instance of IWindowsService.</param>
        public WindowsServiceConcrete(IWindowsService windowsService)
        {
            this.WindowsService = windowsService;

            this.AutoLog = this.WindowsService.WindowsServiceSetupInfo.AutoLog;
            this.CanHandlePowerEvent = this.WindowsService.WindowsServiceSetupInfo.CanHandlePowerEvent;
            this.CanHandleSessionChangeEvent = this.WindowsService.WindowsServiceSetupInfo.CanHandleSessionChangeEvent;
            this.CanPauseAndContinue = this.WindowsService.WindowsServiceSetupInfo.CanPauseAndContinue;
            this.CanShutdown = this.WindowsService.WindowsServiceSetupInfo.CanShutdown;
            this.CanStop = this.WindowsService.WindowsServiceSetupInfo.CanStop;
            this.ServiceName = this.WindowsService.WindowsServiceSetupInfo.ServiceName;
        }

        /// <summary>
        /// Gets instance of IWindowsService.
        /// </summary>
        public IWindowsService WindowsService
        {
            get;
            private set;
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            this.WindowsService.OnStart(args);
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            this.WindowsService.OnStop();
        }

        /// <summary>
        /// When implemented in a derived class, <see cref="M:System.ServiceProcess.ServiceBase.OnContinue" /> runs when a Continue command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service resumes normal functioning after being paused.
        /// </summary>
        protected override void OnContinue()
        {
            this.WindowsService.OnContinue();
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Pause command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service pauses.
        /// </summary>
        protected override void OnPause()
        {
            this.WindowsService.OnPause();
        }

        /// <summary>
        /// When implemented in a derived class, executes when the system is shutting down. Specifies what should occur immediately prior to the system shutting down.
        /// </summary>
        protected override void OnShutdown()
        {
            this.WindowsService.OnShutdown();
        }

        /// <summary>
        /// When implemented in a derived class, <see cref="M:System.ServiceProcess.ServiceBase.OnCustomCommand(System.Int32)" /> executes when the Service Control Manager (SCM) passes a custom command to the service. Specifies actions to take when a command with the specified parameter value occurs.
        /// </summary>
        /// <param name="command">The command message sent to the service.</param>
        protected override void OnCustomCommand(int command)
        {
            this.WindowsService.OnCustomCommand(command);
        }

        /// <summary>
        /// When implemented in a derived class, executes when the computer's power status has changed. This applies to laptop computers when they go into suspended mode, which is not the same as a system shutdown.
        /// </summary>
        /// <param name="powerStatus">A <see cref="T:System.ServiceProcess.PowerBroadcastStatus" /> that indicates a notification from the system about its power status.</param>
        /// <returns>When implemented in a derived class, the needs of your application determine what value to return. For example, if a QuerySuspend broadcast status is passed, you could cause your application to reject the query by returning false.</returns>
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            return this.WindowsService.OnPowerEvent(powerStatus);
        }

        /// <summary>
        /// Executes when a change event is received from a Terminal Server session.
        /// </summary>
        /// <param name="changeDescription">A <see cref="T:System.ServiceProcess.SessionChangeDescription" /> structure that identifies the change type.</param>
        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            this.WindowsService.OnSessionChange(changeDescription);
        }
    }
}
