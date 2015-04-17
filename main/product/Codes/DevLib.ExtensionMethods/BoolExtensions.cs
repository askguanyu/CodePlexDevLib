//-----------------------------------------------------------------------
// <copyright file="BoolExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;

    /// <summary>
    /// Bool Extensions.
    /// </summary>
    public static class BoolExtensions
    {
        /// <summary>
        /// Converts the value of this instance to its equivalent string representation (either "Yes" or "No").
        /// </summary>
        /// <param name="source">Source bool.</param>
        /// <returns>"Yes" or "No" string.</returns>
        public static string ToYesNoString(this bool source)
        {
            return source ? "Yes" : "No";
        }

        /// <summary>
        /// Converts the value in number format {1 , 0}.
        /// </summary>
        /// <param name="source">Source bool.</param>
        /// <returns>1 if true; otherwise, 0.</returns>
        public static int ToBitInt(this bool source)
        {
            return source ? 1 : 0;
        }

        /// <summary>
        /// Converts the value in number format {1 , 0}.
        /// </summary>
        /// <param name="source">Source bool.</param>
        /// <returns>"1" if true; otherwise, "0".</returns>
        public static string ToBitString(this bool source)
        {
            return source ? "1" : "0";
        }

        /// <summary>
        /// Convert 0 or 1 to bool.
        /// </summary>
        /// <param name="source">Source int.</param>
        /// <returns>true if 1; otherwise, false.</returns>
        public static bool BitIntToBool(this int source)
        {
            return source != 0;
        }

        /// <summary>
        /// Convert "0" or "1" to bool.
        /// </summary>
        /// <param name="source">Source int.</param>
        /// <returns>true if "1"; otherwise, false.</returns>
        public static bool BitStringToBool(this string source)
        {
            return source != "0";
        }

        /// <summary>
        /// Convert "Yes" or "No" to bool.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="ignoreCase">Indicating a case-sensitive or insensitive comparison.</param>
        /// <returns>true if "Yes"; otherwise, false.</returns>
        public static bool YesNoToBool(this string source, bool ignoreCase = true)
        {
            return "Yes".Equals(source, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }

        /// <summary>
        /// If source is true, invoke method.
        /// </summary>
        /// <param name="source">Source bool.</param>
        /// <param name="action">Delegate method.
        /// <example>E.g. <code>() => DoSomething();</code></example>
        /// </param>
        /// <returns>The source.</returns>
        public static bool IfTrue(this bool source, Action action)
        {
            if (source)
            {
                action();
            }

            return source;
        }

        /// <summary>
        /// If source is false, invoke method.
        /// </summary>
        /// <param name="source">Source bool.</param>
        /// <param name="action">Delegate method.
        /// <example>E.g. <code>() => DoSomething();</code></example>
        /// </param>
        /// <returns>The source.</returns>
        public static bool IfFalse(this bool source, Action action)
        {
            if (!source)
            {
                action();
            }

            return source;
        }
    }
}
