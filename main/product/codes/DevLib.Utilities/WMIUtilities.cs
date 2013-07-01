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
    /// WMI Utilities.
    /// </summary>
    public static class WMIUtilities
    {
        /// <summary>
        /// Const Field WIN32_ACCOUNT.
        /// </summary>
        public const string WIN32_ACCOUNT = @"SELECT Caption FROM Win32_Account";

        /// <summary>
        /// Const Field MAINBOARD_SN.
        /// </summary>
        public const string MAINBOARD_SN = @"SELECT SerialNumber FROM Win32_BaseBoard";

        /// <summary>
        /// Const Field BIOS_SN.
        /// </summary>
        public const string BIOS_SN = @"SELECT SerialNumber FROM Win32_BIOS";

        /// <summary>
        /// Const Field HARDDISK_MODEL.
        /// </summary>
        public const string HARDDISK_MODEL = @"SELECT Model FROM Win32_DiskDrive";

        /// <summary>
        /// Const Field HARDDISK_SN.
        /// </summary>
        public const string HARDDISK_SN = @"SELECT SerialNumber FROM Win32_DiskDrive";

        /// <summary>
        /// Const Field HARDDISK_SIZE.
        /// </summary>
        public const string HARDDISK_SIZE = @"SELECT Size FROM Win32_DiskDrive";

        /// <summary>
        /// Const Field MACADDRESS.
        /// </summary>
        public const string MACADDRESS = @"SELECT MACAddress FROM Win32_NetworkAdapter";

        /// <summary>
        /// Const Field PHYSICALMEMORY_SN.
        /// </summary>
        public const string PHYSICALMEMORY_SN = @"SELECT SerialNumber FROM Win32_PhysicalMemory";

        /// <summary>
        /// Const Field PHYSICALMEMORY_SIZE.
        /// </summary>
        public const string PHYSICALMEMORY_SIZE = @"SELECT Capacity FROM Win32_PhysicalMemory";

        /// <summary>
        /// Const Field CPU_ID.
        /// </summary>
        public const string CPU_ID = @"SELECT ProcessorId FROM Win32_Processor";

        /// <summary>
        /// Const Field VOLUME_SN.
        /// </summary>
        public const string VOLUME_SN = @"SELECT SerialNumber FROM Win32_Volume";

        /// <summary>
        /// Const Field LOGICALDISK_SN.
        /// </summary>
        public const string LOGICALDISK_SN = @"SELECT VolumeSerialNumber FROM Win32_LogicalDisk";

        /// <summary>
        /// Const Field GPU.
        /// </summary>
        public const string GPU = @"SELECT VideoProcessor FROM Win32_VideoController";

        /// <summary>
        /// Const Field PC_MODEL.
        /// </summary>
        public const string PC_MODEL = @"SELECT Model FROM Win32_ComputerSystem";

        /// <summary>
        /// Const Field PC_IDENTIFYINGNUMBER.
        /// </summary>
        public const string PC_IDENTIFYINGNUMBER = "SELECT IdentifyingNumber FROM Win32_ComputerSystemProduct";

        /// <summary>
        /// Const Field PC_UUID.
        /// </summary>
        public const string PC_UUID = "SELECT UUID FROM Win32_ComputerSystemProduct";

        /// <summary>
        /// Const Field PC_OEMSTRINGARRAY.
        /// </summary>
        public const string PC_OEMSTRINGARRAY = "SELECT OEMStringArray FROM Win32_ComputerSystem";

        /// <summary>
        /// Query a WMI class and associated property.
        /// </summary>
        /// <param name="WMIClass">WMI class, ignore case.</param>
        /// <param name="ClassProperty">Associated property, ignore case.</param>
        /// <returns>A string list of query result.</returns>
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
        /// Query a WMI class and associated property.
        /// </summary>
        /// <param name="WQLString">WQL query string, ignore case.</param>
        /// <param name="ClassProperty">Associated property, ignore case.</param>
        /// <returns>A string list of query result.</returns>
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
                    managementObjectSearcher = null;
                }
            }

            return result;
        }
    }
}
