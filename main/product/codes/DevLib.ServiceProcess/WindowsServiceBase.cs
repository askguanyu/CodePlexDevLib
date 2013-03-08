//-----------------------------------------------------------------------
// <copyright file="WindowsServiceBase.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ServiceProcess
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.ServiceProcess;

    /// <summary>
    /// Class WindowsServiceBase.
    /// </summary>
    public static class WindowsServiceBase
    {
        /// <summary>
        /// Registers the executable for a service with the Service Control Manager (SCM).
        /// </summary>
        /// <param name="windowsService">A <see cref="IWindowsService" /> which indicates a service to start.</param>
        /// <param name="args">Command line arguments.</param>
        public static void Run(IWindowsService windowsService, string[] args = null)
        {
            if (Environment.UserInteractive)
            {
                WindowsServiceConsole.Run(windowsService, args);
            }
            else
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                ServiceBase.Run(new WindowsServiceConcrete(windowsService));
            }
        }

        /// <summary>
        /// Starts a service, passing the specified arguments.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <param name="args">An array of arguments to pass to the service when it starts.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public static bool Start(string serviceName, string[] args = null)
        {
            ServiceController serviceController = null;

            if (ServiceExists(serviceName))
            {
                try
                {
                    serviceController = new ServiceController(serviceName);
                    serviceController.Start(args);
                    serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(60));
                    return serviceController.Status == ServiceControllerStatus.Running;
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    return false;
                }
                finally
                {
                    if (serviceController != null)
                    {
                        serviceController.Dispose();
                        serviceController = null;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Stops this service and any services that are dependent on this service.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public static bool Stop(string serviceName)
        {
            ServiceController serviceController = null;

            if (ServiceExists(serviceName))
            {
                try
                {
                    serviceController = new ServiceController(serviceName);

                    if (serviceController.CanStop)
                    {
                        serviceController.Stop();
                        serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(60));
                        return serviceController.Status == ServiceControllerStatus.Stopped;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    return false;
                }
                finally
                {
                    if (serviceController != null)
                    {
                        serviceController.Dispose();
                        serviceController = null;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Suspends a service's operation.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public static bool Pause(string serviceName)
        {
            ServiceController serviceController = null;

            if (ServiceExists(serviceName))
            {
                try
                {
                    serviceController = new ServiceController(serviceName);

                    if (serviceController.CanPauseAndContinue)
                    {
                        serviceController.Pause();
                        serviceController.WaitForStatus(ServiceControllerStatus.Paused, TimeSpan.FromSeconds(60));
                        return serviceController.Status == ServiceControllerStatus.Paused;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    return false;
                }
                finally
                {
                    if (serviceController != null)
                    {
                        serviceController.Dispose();
                        serviceController = null;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Continues a service after it has been paused.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public static bool Continue(string serviceName)
        {
            ServiceController serviceController = null;

            if (ServiceExists(serviceName))
            {
                try
                {
                    serviceController = new ServiceController(serviceName);

                    if (serviceController.CanPauseAndContinue)
                    {
                        serviceController.Continue();
                        serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(60));
                        return serviceController.Status == ServiceControllerStatus.Running;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    return false;
                }
                finally
                {
                    if (serviceController != null)
                    {
                        serviceController.Dispose();
                        serviceController = null;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Executes a custom command on the service.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <param name="command">An application-defined command flag that indicates which custom command to execute.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public static bool ExecuteCommand(string serviceName, int command)
        {
            ServiceController serviceController = null;

            if (ServiceExists(serviceName))
            {
                try
                {
                    serviceController = new ServiceController(serviceName);
                    serviceController.ExecuteCommand(command);
                    return true;
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    return false;
                }
                finally
                {
                    if (serviceController != null)
                    {
                        serviceController.Dispose();
                        serviceController = null;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the status of the service.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <returns>One of the <see cref="T:System.ServiceProcess.ServiceControllerStatus" /> values that indicates whether the service is running, stopped, or paused, or whether a start, stop, pause, or continue command is pending.</returns>
        public static ServiceControllerStatus GetServiceStatus(string serviceName)
        {
            ServiceController serviceController = null;

            if (ServiceExists(serviceName))
            {
                try
                {
                    serviceController = new ServiceController(serviceName);
                    return serviceController.Status;
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    return ServiceControllerStatus.Stopped;
                }
                finally
                {
                    if (serviceController != null)
                    {
                        serviceController.Dispose();
                        serviceController = null;
                    }
                }
            }
            else
            {
                return ServiceControllerStatus.Stopped;
            }
        }

        /// <summary>
        /// Determines whether the specified service exists.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <returns>true if the specified service exists; otherwise, false.</returns>
        public static bool ServiceExists(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();

            foreach (ServiceController item in services)
            {
                if (serviceName.Equals(item.ServiceName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
