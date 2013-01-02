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
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

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
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return (source.DayOfWeek == DayOfWeek.Sunday) || (source.DayOfWeek == DayOfWeek.Saturday);
        }
    }
}
