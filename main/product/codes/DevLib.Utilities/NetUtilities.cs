//-----------------------------------------------------------------------
// <copyright file="NetUtilities.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Utilities
{
    using System;
    using System.Net;

    /// <summary>
    /// Net Utilities.
    /// </summary>
    public static class NetUtilities
    {
        /// <summary>
        ///
        /// </summary>
        private static Random _random = new Random();

        /// <summary>
        /// Get Local IP Array.
        /// </summary>
        /// <returns>IPAddress[].</returns>
        public static IPAddress[] GetLocalIPArray()
        {
            try
            {
                string hostName = Dns.GetHostName();
                IPHostEntry entry = Dns.GetHostEntry(hostName);
                return entry.AddressList;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Generates a random port number.
        /// </summary>
        /// <returns>Random port number.</returns>
        public static int GetRandomPortNumber()
        {
            return _random.Next(1, 0xFFFF);
        }
    }
}
