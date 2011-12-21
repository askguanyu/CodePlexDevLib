//-----------------------------------------------------------------------
// <copyright file="WMIUtilities.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Utilities
{
    using System.Collections.Generic;
    using System.Management;

    /// <summary>
    /// WMI Utilities
    /// </summary>
    public static class WMIUtilities
    {
        /// <summary>
        /// Query a WMI class and associated property
        /// </summary>
        /// <param name="WMIClass">WMI class</param>
        /// <param name="ClassProperty">Associated property</param>
        /// <returns>A string list of query result</returns>
        public static List<string> Query(string WMIClass, string ClassProperty)
        {
            return QueryWQL(string.Format("SELECT {0} FROM {1}", ClassProperty, WMIClass), ClassProperty);
        }

        /// <summary>
        /// Query a WMI class and associated property
        /// </summary>
        /// <param name="WQLString">WQL query string</param>
        /// <param name="ClassProperty">Associated property</param>
        /// <returns>A string list of query result</returns>
        public static List<string> QueryWQL(string WQLString, string ClassProperty)
        {
            ManagementObjectSearcher managementObjectSearcher = null;
            List<string> result = new List<string>();
            try
            {
                managementObjectSearcher = new ManagementObjectSearcher(WQLString);
                ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();
                foreach (ManagementObject managementObject in managementObjectCollection)
                {
                    if (managementObject.Properties[ClassProperty].Value != null)
                    {
                        result.Add(managementObject.Properties[ClassProperty].Value.ToString());
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
