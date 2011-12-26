//-----------------------------------------------------------------------
// <copyright file="WMIUtilities.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Management;

    /// <summary>
    /// WMI Utilities
    /// </summary>
    public static class WMIUtilities
    {
        /// <summary>
        ///
        /// </summary>
        public const string Win32Account = @"SELECT Caption FROM Win32_Account";

        /// <summary>
        ///
        /// </summary>
        public const string MainboardSN = @"SELECT SerialNumber FROM Win32_BaseBoard";

        /// <summary>
        ///
        /// </summary>
        public const string BiosSN = @"SELECT SerialNumber FROM Win32_BIOS";

        /// <summary>
        ///
        /// </summary>
        public const string HardDiskModel = @"SELECT Model FROM Win32_DiskDrive";

        /// <summary>
        ///
        /// </summary>
        public const string HardDiskSN = @"SELECT SerialNumber FROM Win32_DiskDrive";

        /// <summary>
        ///
        /// </summary>
        public const string HardDiskSize = @"SELECT Size FROM Win32_DiskDrive";

        /// <summary>
        ///
        /// </summary>
        public const string MACAddress = @"SELECT MACAddress FROM Win32_NetworkAdapter";

        /// <summary>
        ///
        /// </summary>
        public const string PhysicalMemorySN = @"SELECT SerialNumber FROM Win32_PhysicalMemory";

        /// <summary>
        ///
        /// </summary>
        public const string PhysicalMemorySize = @"SELECT Capacity FROM Win32_PhysicalMemory";

        /// <summary>
        ///
        /// </summary>
        public const string CPUId = @"SELECT ProcessorId FROM Win32_Processor";

        /// <summary>
        ///
        /// </summary>
        public const string VolumeSN = @"SELECT SerialNumber FROM Win32_Volume";

        /// <summary>
        ///
        /// </summary>
        public const string LogicalDiskSN = @"SELECT VolumeSerialNumber FROM Win32_LogicalDisk";

        /// <summary>
        ///
        /// </summary>
        public const string PCModel = @"SELECT Model FROM Win32_ComputerSystem";

        /// <summary>
        ///
        /// </summary>
        public const string GPU = @"SELECT VideoProcessor FROM Win32_VideoController";

        /// <summary>
        /// Query a WMI class and associated property
        /// </summary>
        /// <param name="WMIClass">WMI class, ingnore case</param>
        /// <param name="ClassProperty">Associated property, ingnore case</param>
        /// <returns>A string list of query result</returns>
        public static List<string> Query(string WMIClass, string ClassProperty)
        {
            if (string.IsNullOrEmpty(WMIClass))
            {
                throw new ArgumentNullException("WMIClass");
            }

            if (string.IsNullOrEmpty(ClassProperty))
            {
                throw new ArgumentNullException("ClassProperty");
            }

            return QueryWQL(string.Format("SELECT {0} FROM {1}", ClassProperty, WMIClass), ClassProperty);
        }

        /// <summary>
        /// Query a WMI class and associated property
        /// </summary>
        /// <param name="WQLString">WQL query string, ingnore case</param>
        /// <param name="ClassProperty">Associated property, ingnore case</param>
        /// <returns>A string list of query result</returns>
        public static List<string> QueryWQL(string WQLString, string ClassProperty = null)
        {
            if (string.IsNullOrEmpty(WQLString))
            {
                throw new ArgumentNullException("WQLString");
            }

            string classProperty = string.Empty;

            if (string.IsNullOrEmpty(ClassProperty))
            {
                if (WQLString.Split(' ')[1] != "*")
                {
                    classProperty = WQLString.Split(' ')[1];
                }
                else
                {
                    throw new ArgumentNullException("ClassProperty");
                }
            }
            else
            {
                classProperty = ClassProperty;
            }

            ManagementObjectSearcher managementObjectSearcher = null;
            List<string> result = new List<string>();
            try
            {
                managementObjectSearcher = new ManagementObjectSearcher(WQLString);
                ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();
                foreach (ManagementObject managementObject in managementObjectCollection)
                {
                    if (managementObject != null &&
                        managementObject.Properties != null &&
                        managementObject.Properties[classProperty] != null &&
                        managementObject.Properties[classProperty].Value != null)
                    {
                        //// Note: Also can use managementObject.GetPropertyValue(classProperty)
                        result.Add(managementObject.Properties[classProperty].Value.ToString());
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (managementObjectSearcher != null)
                {
                    managementObjectSearcher.Dispose();
                }
            }

            return result;
        }
    }
}
