//-----------------------------------------------------------------------
// <copyright file="WebServerManager.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Hosting.WebHost40
{
    using System;
    using System.ComponentModel;
    using System.ServiceProcess;

    /// <summary>
    /// Web server manager.
    /// </summary>
    public static class WebServerManager
    {
        /// <summary>
        /// The IIS service name.
        /// </summary>
        private const string IISServiceName = "W3SVC";

        /// <summary>
        /// The specified service does not exist as an installed service.
        /// </summary>
        private const int ERROR_SERVICE_DOES_NOT_EXIST = 1060;

        /// <summary>
        /// Determines whether IIS is installed.
        /// </summary>
        /// <returns>true if IIS is installed; otherwise, false.</returns>
        public static bool IsIISInstalled()
        {
            try
            {
                ServiceController[] services = ServiceController.GetServices();

                foreach (ServiceController item in services)
                {
                    if (IISServiceName.Equals(item.ServiceName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Determines whether IIS is running.
        /// </summary>
        /// <returns>true if IIS is running; false if IIS is not installed or IIS is installed but not running.</returns>
        public static bool IsIISRunning()
        {
            ServiceController serviceController = null;

            try
            {
                serviceController = new ServiceController(IISServiceName);
                serviceController.Refresh();

                return serviceController.Status == ServiceControllerStatus.Running;
            }
            catch (InvalidOperationException e)
            {
                Win32Exception innerException = e.InnerException as Win32Exception;

                if (innerException != null && innerException.NativeErrorCode == ERROR_SERVICE_DOES_NOT_EXIST)
                {
                    return false;
                }
                else
                {
                    InternalLogger.Log(e);
                    throw;
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
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
    }
}
