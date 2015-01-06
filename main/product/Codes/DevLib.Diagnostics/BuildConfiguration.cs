//-----------------------------------------------------------------------
// <copyright file="BuildConfiguration.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Diagnostics
{
    /// <summary>
    /// BuildConfiguration Utilities.
    /// </summary>
    public static class BuildConfiguration
    {
        /// <summary>
        /// Gets a value indicating whether the assembly was built in debug mode.
        /// </summary>
        public static bool IsDebug
        {
            get
            {
                bool isDebug = true;
#if !DEBUG
                isDebug = false;
#endif
                return isDebug;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the assembly was built in release mode.
        /// </summary>
        public static bool IsRelease
        {
            get
            {
                return !IsDebug;
            }
        }
    }
}
