//-----------------------------------------------------------------------
// <copyright file="DateTimeExtensions.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ExtensionMethods
{
    using System;
    using System.Globalization;

    /// <summary>
    /// DateTime Extensions.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Gets the week number for a provided date time value based on the current culture settings.
        /// </summary>
        /// <param name="source">The DateTime.</param>
        /// <returns>The week number.</returns>
        public static int GetWeekOfYear(this DateTime source)
        {
            var culture = CultureInfo.CurrentUICulture;
            var calendar = culture.Calendar;
            var dateTimeFormat = culture.DateTimeFormat;

            return calendar.GetWeekOfYear(source, dateTimeFormat.CalendarWeekRule, dateTimeFormat.FirstDayOfWeek);
        }

        /// <summary>
        /// Whether DateTime is a weekend.
        /// </summary>
        /// <param name="source">DateTime to check.</param>
        /// <returns>true if DateTime is weekend; otherwise, false.</returns>
        public static bool IsWeekend(this DateTime source)
        {
            return (source.DayOfWeek == DayOfWeek.Sunday) || (source.DayOfWeek == DayOfWeek.Saturday);
        }

        /// <summary>
        /// Converts the specified string representation of a date and time to its <see cref="T:System.DateTime" /> equivalent using the specified format and InvariantCulture information. The format of the string representation must match the specified format exactly.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="formats">A list of possible required format of <paramref name="source" />.</param>
        /// <returns>An object that is equivalent to the date and time contained in <paramref name="source" />, as specified by <paramref name="formats" />.</returns>
        public static DateTime ToDateTime(this string source, params string[] formats)
        {
            if (source.IsNullOrEmpty())
            {
                return default(DateTime);
            }

            DateTime result = default(DateTime);

            if (!DateTime.TryParse(source, out result))
            {
                DateTime.TryParseExact(source, formats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out result);
            }

            return result;
        }
    }
}
