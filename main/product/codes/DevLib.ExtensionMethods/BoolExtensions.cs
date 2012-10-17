//-----------------------------------------------------------------------
// <copyright file="BoolExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    /// <summary>
    /// Bool Extensions.
    /// </summary>
    public static class BoolExtensions
    {
        /// <summary>
        /// Converts the value of this instance to its equivalent string representation (either "Yes" or "No").
        /// </summary>
        /// <param name="source"></param>
        /// <returns>"Yes" or "No" string.</returns>
        public static string ToYesNoString(this bool source)
        {
            return source ? "Yes" : "No";
        }

        /// <summary>
        /// Converts the value in number format {1 , 0}.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>1 if true; otherwise, 0.</returns>
        public static int ToBinaryTypeNumber(this bool source)
        {
            return source ? 1 : 0;
        }

        /// <summary>
        /// Convert int to bool.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>True if 1; otherwise, false.</returns>
        public static bool ToBool(this int source)
        {
            return source == 1 ? true : false;
        }
    }
}
