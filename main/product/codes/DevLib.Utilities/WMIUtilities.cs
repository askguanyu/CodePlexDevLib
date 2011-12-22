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
