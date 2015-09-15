//-----------------------------------------------------------------------
// <copyright file="WMIUtilities.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Management
{
    using System;
    using System.Collections.Generic;
    using System.Management;

    /// <summary>
    /// WMI Utilities.
    /// </summary>
    public static class WMIUtilities
    {
        /// <summary>
        /// Field WIN32_ACCOUNT.
        /// </summary>
        public const string WIN32_ACCOUNT = @"SELECT Caption FROM Win32_Account";

        /// <summary>
        /// Field MAINBOARD_SN.
        /// </summary>
        public const string MAINBOARD_SN = @"SELECT SerialNumber FROM Win32_BaseBoard";

        /// <summary>
        /// Field BIOS_SN.
        /// </summary>
        public const string BIOS_SN = @"SELECT SerialNumber FROM Win32_BIOS";

        /// <summary>
        /// Field HARDDISK_MODEL.
        /// </summary>
        public const string HARDDISK_MODEL = @"SELECT Model FROM Win32_DiskDrive";

        /// <summary>
        /// Field HARDDISK_SN.
        /// </summary>
        public const string HARDDISK_SN = @"SELECT SerialNumber FROM Win32_DiskDrive";

        /// <summary>
        /// Field HARDDISK_SIZE.
        /// </summary>
        public const string HARDDISK_SIZE = @"SELECT Size FROM Win32_DiskDrive";

        /// <summary>
        /// Field MACADDRESS.
        /// </summary>
        public const string MACADDRESS = @"SELECT MACAddress FROM Win32_NetworkAdapter";

        /// <summary>
        /// Field PHYSICALMEMORY_SN.
        /// </summary>
        public const string PHYSICALMEMORY_SN = @"SELECT SerialNumber FROM Win32_PhysicalMemory";

        /// <summary>
        /// Field PHYSICALMEMORY_SIZE.
        /// </summary>
        public const string PHYSICALMEMORY_SIZE = @"SELECT Capacity FROM Win32_PhysicalMemory";

        /// <summary>
        /// Field CPU_ID.
        /// </summary>
        public const string CPU_ID = @"SELECT ProcessorId FROM Win32_Processor";

        /// <summary>
        /// Field VOLUME_SN.
        /// </summary>
        public const string VOLUME_SN = @"SELECT SerialNumber FROM Win32_Volume";

        /// <summary>
        /// Field LOGICALDISK_SN.
        /// </summary>
        public const string LOGICALDISK_SN = @"SELECT VolumeSerialNumber FROM Win32_LogicalDisk";

        /// <summary>
        /// Field GPU.
        /// </summary>
        public const string GPU = @"SELECT VideoProcessor FROM Win32_VideoController";

        /// <summary>
        /// Field PC_MODEL.
        /// </summary>
        public const string PC_MODEL = @"SELECT Model FROM Win32_ComputerSystem";

        /// <summary>
        /// Field PC_IDENTIFYINGNUMBER.
        /// </summary>
        public const string PC_IDENTIFYINGNUMBER = "SELECT IdentifyingNumber FROM Win32_ComputerSystemProduct";

        /// <summary>
        /// Field PC_UUID.
        /// </summary>
        public const string PC_UUID = "SELECT UUID FROM Win32_ComputerSystemProduct";

        /// <summary>
        /// Field PC_OEMSTRINGARRAY.
        /// </summary>
        public const string PC_OEMSTRINGARRAY = "SELECT OEMStringArray FROM Win32_ComputerSystem";

        /// <summary>
        /// Query a WMI class and associated property.
        /// </summary>
        /// <param name="wmiClass">WMI class, ignore case.</param>
        /// <param name="classProperty">Associated property, ignore case.</param>
        /// <returns>A string list of query result.</returns>
        public static List<string> Query(string wmiClass, string classProperty)
        {
            if (string.IsNullOrEmpty(wmiClass))
            {
                throw new ArgumentNullException("WMIClass");
            }

            if (string.IsNullOrEmpty(classProperty))
            {
                throw new ArgumentNullException("ClassProperty");
            }

            return QueryWQL(string.Format("SELECT {0} FROM {1}", classProperty, wmiClass), classProperty);
        }

        /// <summary>
        /// Query a WMI class and associated property.
        /// </summary>
        /// <param name="wqlString">WQL query string, ignore case.</param>
        /// <param name="classProperty">Associated property, ignore case.</param>
        /// <returns>A string list of query result.</returns>
        public static List<string> QueryWQL(string wqlString, string classProperty = null)
        {
            if (string.IsNullOrEmpty(wqlString))
            {
                throw new ArgumentNullException("WQLString");
            }

            string classPropertyName = string.Empty;

            if (string.IsNullOrEmpty(classProperty))
            {
                if (wqlString.Split(' ')[1] != "*")
                {
                    classPropertyName = wqlString.Split(' ')[1];
                }
                else
                {
                    throw new ArgumentNullException("ClassProperty");
                }
            }
            else
            {
                classPropertyName = classProperty;
            }

            ManagementObjectSearcher managementObjectSearcher = null;
            List<string> result = new List<string>();

            try
            {
                managementObjectSearcher = new ManagementObjectSearcher(wqlString);
                ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();

                foreach (ManagementObject managementObject in managementObjectCollection)
                {
                    if (managementObject != null &&
                        managementObject.Properties != null &&
                        managementObject.Properties[classPropertyName] != null &&
                        managementObject.Properties[classPropertyName].Value != null)
                    {
                        //// Note: Also can use managementObject.GetPropertyValue(classProperty)
                        result.Add(managementObject.Properties[classPropertyName].Value.ToString());
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
                    managementObjectSearcher = null;
                }
            }

            return result;
        }
    }
}
