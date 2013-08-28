//-----------------------------------------------------------------------
// <copyright file="IWindowsService.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceProcess
{
    using System.ServiceProcess;

    /// <summary>
    /// Provides an interface for a service that will exist as part of a service application. <see cref="IWindowsService" /> must be derived from when creating a new service class.
    /// </summary>
    public interface IWindowsService
    {
        /// <summary>
        /// Gets or sets WindowsServiceSetup instance.
        /// </summary>
        WindowsServiceSetup WindowsServiceSetupInfo
        {
            get;
            set;
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        void OnStart(string[] args);

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        void OnStop();

        /// <summary>
        /// When implemented in a derived class, executes when a Pause command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service pauses.
        /// </summary>
        void OnPause();

        /// <summary>
        /// When implemented in a derived class, <see cref="M:System.ServiceProcess.ServiceBase.OnContinue" /> runs when a Continue command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service resumes normal functioning after being paused.
        /// </summary>
        void OnContinue();

        /// <summary>
        /// When implemented in a derived class, executes when the system is shutting down. Specifies what should occur immediately prior to the system shutting down.
        /// </summary>
        void OnShutdown();

        /// <summary>
        /// When implemented in a derived class, <see cref="M:System.ServiceProcess.ServiceBase.OnCustomCommand(System.Int32)" /> executes when the Service Control Manager (SCM) passes a custom command to the service. Specifies actions to take when a command with the specified parameter value occurs.
        /// </summary>
        /// <param name="command">The command message sent to the service.</param>
        void OnCustomCommand(int command);

        /// <summary>
        /// Executes when a change event is received from a Terminal Server session.
        /// </summary>
        /// <param name="changeDescription">A <see cref="T:System.ServiceProcess.SessionChangeDescription" /> structure that identifies the change type.</param>
        void OnSessionChange(SessionChangeDescription changeDescription);

        /// <summary>
        /// When implemented in a derived class, executes when the computer's power status has changed. This applies to laptop computers when they go into suspended mode, which is not the same as a system shutdown.
        /// </summary>
        /// <param name="powerStatus">A <see cref="T:System.ServiceProcess.PowerBroadcastStatus" /> that indicates a notification from the system about its power status.</param>
        /// <returns>When implemented in a derived class, the needs of your application determine what value to return. For example, if a QuerySuspend broadcast status is passed, you could cause your application to reject the query by returning false.</returns>
        bool OnPowerEvent(PowerBroadcastStatus powerStatus);
    }
}
