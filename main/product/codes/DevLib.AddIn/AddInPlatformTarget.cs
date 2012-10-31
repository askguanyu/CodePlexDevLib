//-----------------------------------------------------------------------
// <copyright file="AddInPlatformTarget.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;

    /// <summary>
    ///
    /// </summary>
    public enum PlatformTargetEnum
    {
        AnyCPU,
        x86,
        x64,
        Itanium
    }

    /// <summary>
    ///
    /// </summary>
    public static class AddInPlatformTarget
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="platformTarget"></param>
        /// <returns></returns>
        public static string GetPlatformTargetCompilerArgument(PlatformTargetEnum platformTarget)
        {
            switch (platformTarget)
            {
                case PlatformTargetEnum.AnyCPU:
                    return "/platform:anycpu";
                case PlatformTargetEnum.x86:
                    return "/platform:x86";
                case PlatformTargetEnum.x64:
                    return "/platform:x64";
                case PlatformTargetEnum.Itanium:
                    return "/platform:Itanium";
            }

            throw new NotSupportedException(AddInConstants.AddInUnknownPlatformException);
        }
    }
}
