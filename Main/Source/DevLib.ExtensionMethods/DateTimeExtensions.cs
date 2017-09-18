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
        public const long TicksPerMillisecond = 10000;
        public const long TicksPerSecond = TicksPerMillisecond * 1000;
        public const long TicksPerMinute = TicksPerSecond * 60;
        public const long TicksPerHour = TicksPerMinute * 60;
        public const long TicksPerDay = TicksPerHour * 24;
        public const int DaysPerYear = 365;
        public const int DaysPer4Years = DaysPerYear * 4 + 1;       // 1461
        public const int DaysPer100Years = DaysPer4Years * 25 - 1;  // 36524
        public const int DaysPer400Years = DaysPer100Years * 4 + 1; // 146097
        public const int DaysTo1970 = DaysPer400Years * 4 + DaysPer100Years * 3 + DaysPer4Years * 17 + DaysPerYear; // 719,162
        public const int DaysTo10000 = DaysPer400Years * 25 - 366;  // 3652059
        public const long UnixEpochTicks = TimeSpan.TicksPerDay * DaysTo1970; // 621,355,968,000,000,000
        public const long UnixEpochSeconds = UnixEpochTicks / TimeSpan.TicksPerSecond; // 62,135,596,800
        public const long UnixEpochMilliseconds = UnixEpochTicks / TimeSpan.TicksPerMillisecond; // 62,135,596,800,000
        public const long MinTicks = 0;
        public const long MaxTicks = DaysTo10000 * TicksPerDay - 1;
        public const long MinSeconds = MinTicks / TimeSpan.TicksPerSecond - UnixEpochSeconds;
        public const long MaxSeconds = MaxTicks / TimeSpan.TicksPerSecond - UnixEpochSeconds;
        public const long MinMilliseconds = MinTicks / TimeSpan.TicksPerMillisecond - UnixEpochMilliseconds;
        public const long MaxMilliseconds = MaxTicks / TimeSpan.TicksPerMillisecond - UnixEpochMilliseconds;

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

        /// <summary>
        /// Converts a time to the time in another time zone based on the time zone offset.
        /// </summary>
        /// <param name="source">The date and time to convert.</param>
        /// <param name="destTimeZoneUtcOffset">The UTC offset of the destination time zone.</param>
        /// <returns>A System.DateTime value that represents the date and time in the destination time zone.</returns>
        public static DateTime ToTimeZone(this DateTime source, double destTimeZoneUtcOffset)
        {
            return new DateTimeOffset(source).ToOffset(TimeSpan.FromHours(destTimeZoneUtcOffset)).DateTime;
        }

        /// <summary>
        /// Converts a time to the time in another time zone based on the time zone's identifier.
        /// </summary>
        /// <param name="source">The date and time to convert.</param>
        /// <param name="destTimeZoneId">The identifier of the destination time zone.</param>
        /// <returns>A System.DateTime value that represents the date and time in the destination time zone.</returns>
        public static DateTime ToTimeZone(this DateTime source, string destTimeZoneId)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(source, destTimeZoneId);
        }

        /// <summary>
        /// Converts a time to the time in another time zone based on the time zone offset.
        /// </summary>
        /// <param name="source">The date and time to convert.</param>
        /// <param name="destTimeZoneUtcOffset">The UTC offset of the destination time zone.</param>
        /// <returns>A System.DateTime value that represents the date and time in the destination time zone.</returns>
        public static DateTimeOffset ToTimeZone(this DateTimeOffset source, double destTimeZoneUtcOffset)
        {
            return source.ToOffset(TimeSpan.FromHours(destTimeZoneUtcOffset));
        }

        /// <summary>
        /// Converts a time to the time in another time zone based on the time zone's identifier.
        /// </summary>
        /// <param name="source">The date and time to convert.</param>
        /// <param name="destTimeZoneId">The identifier of the destination time zone.</param>
        /// <returns>A System.DateTime value that represents the date and time in the destination time zone.</returns>
        public static DateTimeOffset ToTimeZone(this DateTimeOffset source, string destTimeZoneId)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(source, destTimeZoneId);
        }

        /// <summary>
        /// Returns the number of seconds that have elapsed since 1970-01-01T00:00:00Z.
        /// </summary>
        /// <param name="source">The source DateTimeOffset.</param>
        /// <returns>The number of seconds that have elapsed since 1970-01-01T00:00:00Z.</returns>
        public static long ToUnixTimeSeconds(this DateTimeOffset source)
        {
            long seconds = source.Ticks / TimeSpan.TicksPerSecond;
            return seconds - UnixEpochSeconds;
        }

        /// <summary>
        /// Returns the number of milliseconds that have elapsed since 1970-01-01T00:00:00.000Z.
        /// </summary>
        /// <param name="source">The source DateTimeOffset.</param>
        /// <returns>The number of milliseconds that have elapsed since 1970-01-01T00:00:00.000Z.</returns>
        public static long ToUnixTimeMilliseconds(this DateTimeOffset source)
        {
            long milliseconds = source.Ticks / TimeSpan.TicksPerMillisecond;
            return milliseconds - UnixEpochMilliseconds;
        }

        /// <summary>
        /// Converts a Unix time expressed as the number of seconds that have elapsed since 1970-01-01T00:00:00Z to a DateTimeOffset value.
        /// </summary>
        /// <param name="source">A Unix time, expressed as the number of seconds that have elapsed since 1970-01-01T00:00:00Z (January 1, 1970, at 12:00 AM UTC). For Unix times before this date, its value is negative.</param>
        /// <returns>A date and time value that represents the same moment in time as the Unix time.</returns>
        public static DateTimeOffset FromUnixTimeSeconds(this long source)
        {
            if (source < MinSeconds || source > MaxSeconds)
            {
                throw new ArgumentOutOfRangeException("source");
            }

            long ticks = source * TimeSpan.TicksPerSecond + UnixEpochTicks;
            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }

        /// <summary>
        /// Converts a Unix time expressed as the number of milliseconds that have elapsed since 1970-01-01T00:00:00Z to a DateTimeOffset value.
        /// </summary>
        /// <param name="source">A Unix time, expressed as the number of milliseconds that have elapsed since 1970-01-01T00:00:00Z (January 1, 1970, at 12:00 AM UTC). For Unix times before this date, its value is negative.</param>
        /// <returns>A date and time value that represents the same moment in time as the Unix time.</returns>
        public static DateTimeOffset FromUnixTimeMilliseconds(this long source)
        {
            if (source < MinMilliseconds || source > MaxMilliseconds)
            {
                throw new ArgumentOutOfRangeException("source");
            }

            long ticks = source * TimeSpan.TicksPerMillisecond + UnixEpochTicks;
            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }

        /// <summary>
        /// Returns the number of seconds that have elapsed since 1970-01-01T00:00:00Z.
        /// </summary>
        /// <param name="source">The source DateTime.</param>
        /// <returns>The number of seconds that have elapsed since 1970-01-01T00:00:00Z.</returns>
        public static long ToUnixTimeSeconds(this DateTime source)
        {
            long seconds = source.Ticks / TimeSpan.TicksPerSecond;
            return seconds - UnixEpochSeconds;
        }

        /// <summary>
        /// Returns the number of milliseconds that have elapsed since 1970-01-01T00:00:00.000Z.
        /// </summary>
        /// <param name="source">The source DateTime.</param>
        /// <returns>The number of milliseconds that have elapsed since 1970-01-01T00:00:00.000Z.</returns>
        public static long ToUnixTimeMilliseconds(this DateTime source)
        {
            long milliseconds = source.Ticks / TimeSpan.TicksPerMillisecond;
            return milliseconds - UnixEpochMilliseconds;
        }
    }

    /// <summary>
    /// Represents any time zone in the world.
    /// </summary>
    public static class TimeZoneInfoId
    {
        /// <summary>
        /// StandardName: Dateline Standard Time
        /// DisplayName: (UTC-12:00) International Date Line West
        /// </summary>
        public const string Dateline_Standard_Time = "Dateline Standard Time";

        /// <summary>
        /// StandardName: UTC-11
        /// DisplayName: (UTC-11:00) Coordinated Universal Time-11
        /// </summary>
        public const string UTC_Minus_11 = "UTC-11";

        /// <summary>
        /// StandardName: Aleutian Standard Time
        /// DisplayName: (UTC-10:00) Aleutian Islands
        /// </summary>
        public const string Aleutian_Standard_Time = "Aleutian Standard Time";

        /// <summary>
        /// StandardName: Hawaiian Standard Time
        /// DisplayName: (UTC-10:00) Hawaii
        /// </summary>
        public const string Hawaiian_Standard_Time = "Hawaiian Standard Time";

        /// <summary>
        /// StandardName: Marquesas Standard Time
        /// DisplayName: (UTC-09:30) Marquesas Islands
        /// </summary>
        public const string Marquesas_Standard_Time = "Marquesas Standard Time";

        /// <summary>
        /// StandardName: Alaskan Standard Time
        /// DisplayName: (UTC-09:00) Alaska
        /// </summary>
        public const string Alaskan_Standard_Time = "Alaskan Standard Time";

        /// <summary>
        /// StandardName: UTC-09
        /// DisplayName: (UTC-09:00) Coordinated Universal Time-09
        /// </summary>
        public const string UTC_Minus_09 = "UTC-09";

        /// <summary>
        /// StandardName: Pacific Standard Time (Mexico)
        /// DisplayName: (UTC-08:00) Baja California
        /// </summary>
        public const string Pacific_Standard_Time_Mexico = "Pacific Standard Time (Mexico)";

        /// <summary>
        /// StandardName: UTC-08
        /// DisplayName: (UTC-08:00) Coordinated Universal Time-08
        /// </summary>
        public const string UTC_Minus_08 = "UTC-08";

        /// <summary>
        /// StandardName: Pacific Standard Time
        /// DisplayName: (UTC-08:00) Pacific Time (US and Canada)
        /// </summary>
        public const string Pacific_Standard_Time = "Pacific Standard Time";

        /// <summary>
        /// StandardName: US Mountain Standard Time
        /// DisplayName: (UTC-07:00) Arizona
        /// </summary>
        public const string US_Mountain_Standard_Time = "US Mountain Standard Time";

        /// <summary>
        /// StandardName: Mountain Standard Time (Mexico)
        /// DisplayName: (UTC-07:00) Chihuahua, La Paz, Mazatlan
        /// </summary>
        public const string Mountain_Standard_Time_Mexico = "Mountain Standard Time (Mexico)";

        /// <summary>
        /// StandardName: Mountain Standard Time
        /// DisplayName: (UTC-07:00) Mountain Time (US and Canada)
        /// </summary>
        public const string Mountain_Standard_Time = "Mountain Standard Time";

        /// <summary>
        /// StandardName: Central America Standard Time
        /// DisplayName: (UTC-06:00) Central America
        /// </summary>
        public const string Central_America_Standard_Time = "Central America Standard Time";

        /// <summary>
        /// StandardName: Central Standard Time
        /// DisplayName: (UTC-06:00) Central Time (US and Canada)
        /// </summary>
        public const string Central_Standard_Time = "Central Standard Time";

        /// <summary>
        /// StandardName: Easter Island Standard Time
        /// DisplayName: (UTC-06:00) Easter Island
        /// </summary>
        public const string Easter_Island_Standard_Time = "Easter Island Standard Time";

        /// <summary>
        /// StandardName: Central Standard Time (Mexico)
        /// DisplayName: (UTC-06:00) Guadalajara, Mexico City, Monterrey
        /// </summary>
        public const string Central_Standard_Time_Mexico = "Central Standard Time (Mexico)";

        /// <summary>
        /// StandardName: Canada Central Standard Time
        /// DisplayName: (UTC-06:00) Saskatchewan
        /// </summary>
        public const string Canada_Central_Standard_Time = "Canada Central Standard Time";

        /// <summary>
        /// StandardName: SA Pacific Standard Time
        /// DisplayName: (UTC-05:00) Bogota, Lima, Quito, Rio Branco
        /// </summary>
        public const string SA_Pacific_Standard_Time = "SA Pacific Standard Time";

        /// <summary>
        /// StandardName: Eastern Standard Time (Mexico)
        /// DisplayName: (UTC-05:00) Chetumal
        /// </summary>
        public const string Eastern_Standard_Time_Mexico = "Eastern Standard Time (Mexico)";

        /// <summary>
        /// StandardName: Eastern Standard Time
        /// DisplayName: (UTC-05:00) Eastern Time (US and Canada)
        /// </summary>
        public const string Eastern_Standard_Time = "Eastern Standard Time";

        /// <summary>
        /// StandardName: Haiti Standard Time
        /// DisplayName: (UTC-05:00) Haiti
        /// </summary>
        public const string Haiti_Standard_Time = "Haiti Standard Time";

        /// <summary>
        /// StandardName: Cuba Standard Time
        /// DisplayName: (UTC-05:00) Havana
        /// </summary>
        public const string Cuba_Standard_Time = "Cuba Standard Time";

        /// <summary>
        /// StandardName: US Eastern Standard Time
        /// DisplayName: (UTC-05:00) Indiana (East)
        /// </summary>
        public const string US_Eastern_Standard_Time = "US Eastern Standard Time";

        /// <summary>
        /// StandardName: Paraguay Standard Time
        /// DisplayName: (UTC-04:00) Asuncion
        /// </summary>
        public const string Paraguay_Standard_Time = "Paraguay Standard Time";

        /// <summary>
        /// StandardName: Atlantic Standard Time
        /// DisplayName: (UTC-04:00) Atlantic Time (Canada)
        /// </summary>
        public const string Atlantic_Standard_Time = "Atlantic Standard Time";

        /// <summary>
        /// StandardName: Venezuela Standard Time
        /// DisplayName: (UTC-04:00) Caracas
        /// </summary>
        public const string Venezuela_Standard_Time = "Venezuela Standard Time";

        /// <summary>
        /// StandardName: Central Brazilian Standard Time
        /// DisplayName: (UTC-04:00) Cuiaba
        /// </summary>
        public const string Central_Brazilian_Standard_Time = "Central Brazilian Standard Time";

        /// <summary>
        /// StandardName: SA Western Standard Time
        /// DisplayName: (UTC-04:00) Georgetown, La Paz, Manaus, San Juan
        /// </summary>
        public const string SA_Western_Standard_Time = "SA Western Standard Time";

        /// <summary>
        /// StandardName: Pacific SA Standard Time
        /// DisplayName: (UTC-04:00) Santiago
        /// </summary>
        public const string Pacific_SA_Standard_Time = "Pacific SA Standard Time";

        /// <summary>
        /// StandardName: Turks and Caicos Standard Time
        /// DisplayName: (UTC-04:00) Turks and Caicos
        /// </summary>
        public const string Turks_And_Caicos_Standard_Time = "Turks And Caicos Standard Time";

        /// <summary>
        /// StandardName: Newfoundland Standard Time
        /// DisplayName: (UTC-03:30) Newfoundland
        /// </summary>
        public const string Newfoundland_Standard_Time = "Newfoundland Standard Time";

        /// <summary>
        /// StandardName: Tocantins Standard Time
        /// DisplayName: (UTC-03:00) Araguaina
        /// </summary>
        public const string Tocantins_Standard_Time = "Tocantins Standard Time";

        /// <summary>
        /// StandardName: E. South America Standard Time
        /// DisplayName: (UTC-03:00) Brasilia
        /// </summary>
        public const string E__South_America_Standard_Time = "E. South America Standard Time";

        /// <summary>
        /// StandardName: SA Eastern Standard Time
        /// DisplayName: (UTC-03:00) Cayenne, Fortaleza
        /// </summary>
        public const string SA_Eastern_Standard_Time = "SA Eastern Standard Time";

        /// <summary>
        /// StandardName: Argentina Standard Time
        /// DisplayName: (UTC-03:00) City of Buenos Aires
        /// </summary>
        public const string Argentina_Standard_Time = "Argentina Standard Time";

        /// <summary>
        /// StandardName: Greenland Standard Time
        /// DisplayName: (UTC-03:00) Greenland
        /// </summary>
        public const string Greenland_Standard_Time = "Greenland Standard Time";

        /// <summary>
        /// StandardName: Montevideo Standard Time
        /// DisplayName: (UTC-03:00) Montevideo
        /// </summary>
        public const string Montevideo_Standard_Time = "Montevideo Standard Time";

        /// <summary>
        /// StandardName: Magallanes Standard Time
        /// DisplayName: (UTC-03:00) Punta Arenas
        /// </summary>
        public const string Magallanes_Standard_Time = "Magallanes Standard Time";

        /// <summary>
        /// StandardName: Saint Pierre Standard Time
        /// DisplayName: (UTC-03:00) Saint Pierre and Miquelon
        /// </summary>
        public const string Saint_Pierre_Standard_Time = "Saint Pierre Standard Time";

        /// <summary>
        /// StandardName: Bahia Standard Time
        /// DisplayName: (UTC-03:00) Salvador
        /// </summary>
        public const string Bahia_Standard_Time = "Bahia Standard Time";

        /// <summary>
        /// StandardName: UTC-02
        /// DisplayName: (UTC-02:00) Coordinated Universal Time-02
        /// </summary>
        public const string UTC_Minus_02 = "UTC-02";

        /// <summary>
        /// StandardName: Mid-Atlantic Standard Time
        /// DisplayName: (UTC-02:00) Mid-Atlantic - Old
        /// </summary>
        public const string Mid_Minus_Atlantic_Standard_Time = "Mid-Atlantic Standard Time";

        /// <summary>
        /// StandardName: Azores Standard Time
        /// DisplayName: (UTC-01:00) Azores
        /// </summary>
        public const string Azores_Standard_Time = "Azores Standard Time";

        /// <summary>
        /// StandardName: Cabo Verde Standard Time
        /// DisplayName: (UTC-01:00) Cabo Verde Is.
        /// </summary>
        public const string Cape_Verde_Standard_Time = "Cape Verde Standard Time";

        /// <summary>
        /// StandardName: Coordinated Universal Time
        /// DisplayName: (UTC) Coordinated Universal Time
        /// </summary>
        public const string UTC = "UTC";

        /// <summary>
        /// StandardName: Morocco Standard Time
        /// DisplayName: (UTC+00:00) Casablanca
        /// </summary>
        public const string Morocco_Standard_Time = "Morocco Standard Time";

        /// <summary>
        /// StandardName: GMT Standard Time
        /// DisplayName: (UTC+00:00) Dublin, Edinburgh, Lisbon, London
        /// </summary>
        public const string GMT_Standard_Time = "GMT Standard Time";

        /// <summary>
        /// StandardName: Greenwich Standard Time
        /// DisplayName: (UTC+00:00) Monrovia, Reykjavik
        /// </summary>
        public const string Greenwich_Standard_Time = "Greenwich Standard Time";

        /// <summary>
        /// StandardName: W. Europe Standard Time
        /// DisplayName: (UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna
        /// </summary>
        public const string W__Europe_Standard_Time = "W. Europe Standard Time";

        /// <summary>
        /// StandardName: Central Europe Standard Time
        /// DisplayName: (UTC+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague
        /// </summary>
        public const string Central_Europe_Standard_Time = "Central Europe Standard Time";

        /// <summary>
        /// StandardName: Romance Standard Time
        /// DisplayName: (UTC+01:00) Brussels, Copenhagen, Madrid, Paris
        /// </summary>
        public const string Romance_Standard_Time = "Romance Standard Time";

        /// <summary>
        /// StandardName: Central European Standard Time
        /// DisplayName: (UTC+01:00) Sarajevo, Skopje, Warsaw, Zagreb
        /// </summary>
        public const string Central_European_Standard_Time = "Central European Standard Time";

        /// <summary>
        /// StandardName: W. Central Africa Standard Time
        /// DisplayName: (UTC+01:00) West Central Africa
        /// </summary>
        public const string W__Central_Africa_Standard_Time = "W. Central Africa Standard Time";

        /// <summary>
        /// StandardName: Namibia Standard Time
        /// DisplayName: (UTC+01:00) Windhoek
        /// </summary>
        public const string Namibia_Standard_Time = "Namibia Standard Time";

        /// <summary>
        /// StandardName: Jordan Standard Time
        /// DisplayName: (UTC+02:00) Amman
        /// </summary>
        public const string Jordan_Standard_Time = "Jordan Standard Time";

        /// <summary>
        /// StandardName: GTB Standard Time
        /// DisplayName: (UTC+02:00) Athens, Bucharest
        /// </summary>
        public const string GTB_Standard_Time = "GTB Standard Time";

        /// <summary>
        /// StandardName: Middle East Standard Time
        /// DisplayName: (UTC+02:00) Beirut
        /// </summary>
        public const string Middle_East_Standard_Time = "Middle East Standard Time";

        /// <summary>
        /// StandardName: Egypt Standard Time
        /// DisplayName: (UTC+02:00) Cairo
        /// </summary>
        public const string Egypt_Standard_Time = "Egypt Standard Time";

        /// <summary>
        /// StandardName: E. Europe Standard Time
        /// DisplayName: (UTC+02:00) Chisinau
        /// </summary>
        public const string E__Europe_Standard_Time = "E. Europe Standard Time";

        /// <summary>
        /// StandardName: Syria Standard Time
        /// DisplayName: (UTC+02:00) Damascus
        /// </summary>
        public const string Syria_Standard_Time = "Syria Standard Time";

        /// <summary>
        /// StandardName: West Bank Gaza Standard Time
        /// DisplayName: (UTC+02:00) Gaza, Hebron
        /// </summary>
        public const string West_Bank_Standard_Time = "West Bank Standard Time";

        /// <summary>
        /// StandardName: South Africa Standard Time
        /// DisplayName: (UTC+02:00) Harare, Pretoria
        /// </summary>
        public const string South_Africa_Standard_Time = "South Africa Standard Time";

        /// <summary>
        /// StandardName: FLE Standard Time
        /// DisplayName: (UTC+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius
        /// </summary>
        public const string FLE_Standard_Time = "FLE Standard Time";

        /// <summary>
        /// StandardName: Jerusalem Standard Time
        /// DisplayName: (UTC+02:00) Jerusalem
        /// </summary>
        public const string Israel_Standard_Time = "Israel Standard Time";

        /// <summary>
        /// StandardName: Russia TZ 1 Standard Time
        /// DisplayName: (UTC+02:00) Kaliningrad
        /// </summary>
        public const string Kaliningrad_Standard_Time = "Kaliningrad Standard Time";

        /// <summary>
        /// StandardName: Libya Standard Time
        /// DisplayName: (UTC+02:00) Tripoli
        /// </summary>
        public const string Libya_Standard_Time = "Libya Standard Time";

        /// <summary>
        /// StandardName: Arabic Standard Time
        /// DisplayName: (UTC+03:00) Baghdad
        /// </summary>
        public const string Arabic_Standard_Time = "Arabic Standard Time";

        /// <summary>
        /// StandardName: Turkey Standard Time
        /// DisplayName: (UTC+03:00) Istanbul
        /// </summary>
        public const string Turkey_Standard_Time = "Turkey Standard Time";

        /// <summary>
        /// StandardName: Arab Standard Time
        /// DisplayName: (UTC+03:00) Kuwait, Riyadh
        /// </summary>
        public const string Arab_Standard_Time = "Arab Standard Time";

        /// <summary>
        /// StandardName: Belarus Standard Time
        /// DisplayName: (UTC+03:00) Minsk
        /// </summary>
        public const string Belarus_Standard_Time = "Belarus Standard Time";

        /// <summary>
        /// StandardName: Russia TZ 2 Standard Time
        /// DisplayName: (UTC+03:00) Moscow, St. Petersburg, Volgograd
        /// </summary>
        public const string Russian_Standard_Time = "Russian Standard Time";

        /// <summary>
        /// StandardName: E. Africa Standard Time
        /// DisplayName: (UTC+03:00) Nairobi
        /// </summary>
        public const string E__Africa_Standard_Time = "E. Africa Standard Time";

        /// <summary>
        /// StandardName: Iran Standard Time
        /// DisplayName: (UTC+03:30) Tehran
        /// </summary>
        public const string Iran_Standard_Time = "Iran Standard Time";

        /// <summary>
        /// StandardName: Arabian Standard Time
        /// DisplayName: (UTC+04:00) Abu Dhabi, Muscat
        /// </summary>
        public const string Arabian_Standard_Time = "Arabian Standard Time";

        /// <summary>
        /// StandardName: Astrakhan Standard Time
        /// DisplayName: (UTC+04:00) Astrakhan, Ulyanovsk
        /// </summary>
        public const string Astrakhan_Standard_Time = "Astrakhan Standard Time";

        /// <summary>
        /// StandardName: Azerbaijan Standard Time
        /// DisplayName: (UTC+04:00) Baku
        /// </summary>
        public const string Azerbaijan_Standard_Time = "Azerbaijan Standard Time";

        /// <summary>
        /// StandardName: Russia TZ 3 Standard Time
        /// DisplayName: (UTC+04:00) Izhevsk, Samara
        /// </summary>
        public const string Russia_Time_Zone_3 = "Russia Time Zone 3";

        /// <summary>
        /// StandardName: Mauritius Standard Time
        /// DisplayName: (UTC+04:00) Port Louis
        /// </summary>
        public const string Mauritius_Standard_Time = "Mauritius Standard Time";

        /// <summary>
        /// StandardName: Saratov Standard Time
        /// DisplayName: (UTC+04:00) Saratov
        /// </summary>
        public const string Saratov_Standard_Time = "Saratov Standard Time";

        /// <summary>
        /// StandardName: Georgian Standard Time
        /// DisplayName: (UTC+04:00) Tbilisi
        /// </summary>
        public const string Georgian_Standard_Time = "Georgian Standard Time";

        /// <summary>
        /// StandardName: Caucasus Standard Time
        /// DisplayName: (UTC+04:00) Yerevan
        /// </summary>
        public const string Caucasus_Standard_Time = "Caucasus Standard Time";

        /// <summary>
        /// StandardName: Afghanistan Standard Time
        /// DisplayName: (UTC+04:30) Kabul
        /// </summary>
        public const string Afghanistan_Standard_Time = "Afghanistan Standard Time";

        /// <summary>
        /// StandardName: West Asia Standard Time
        /// DisplayName: (UTC+05:00) Ashgabat, Tashkent
        /// </summary>
        public const string West_Asia_Standard_Time = "West Asia Standard Time";

        /// <summary>
        /// StandardName: Russia TZ 4 Standard Time
        /// DisplayName: (UTC+05:00) Ekaterinburg
        /// </summary>
        public const string Ekaterinburg_Standard_Time = "Ekaterinburg Standard Time";

        /// <summary>
        /// StandardName: Pakistan Standard Time
        /// DisplayName: (UTC+05:00) Islamabad, Karachi
        /// </summary>
        public const string Pakistan_Standard_Time = "Pakistan Standard Time";

        /// <summary>
        /// StandardName: India Standard Time
        /// DisplayName: (UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi
        /// </summary>
        public const string India_Standard_Time = "India Standard Time";

        /// <summary>
        /// StandardName: Sri Lanka Standard Time
        /// DisplayName: (UTC+05:30) Sri Jayawardenepura
        /// </summary>
        public const string Sri_Lanka_Standard_Time = "Sri Lanka Standard Time";

        /// <summary>
        /// StandardName: Nepal Standard Time
        /// DisplayName: (UTC+05:45) Kathmandu
        /// </summary>
        public const string Nepal_Standard_Time = "Nepal Standard Time";

        /// <summary>
        /// StandardName: Central Asia Standard Time
        /// DisplayName: (UTC+06:00) Astana
        /// </summary>
        public const string Central_Asia_Standard_Time = "Central Asia Standard Time";

        /// <summary>
        /// StandardName: Bangladesh Standard Time
        /// DisplayName: (UTC+06:00) Dhaka
        /// </summary>
        public const string Bangladesh_Standard_Time = "Bangladesh Standard Time";

        /// <summary>
        /// StandardName: Omsk Standard Time
        /// DisplayName: (UTC+06:00) Omsk
        /// </summary>
        public const string Omsk_Standard_Time = "Omsk Standard Time";

        /// <summary>
        /// StandardName: Myanmar Standard Time
        /// DisplayName: (UTC+06:30) Yangon (Rangoon)
        /// </summary>
        public const string Myanmar_Standard_Time = "Myanmar Standard Time";

        /// <summary>
        /// StandardName: SE Asia Standard Time
        /// DisplayName: (UTC+07:00) Bangkok, Hanoi, Jakarta
        /// </summary>
        public const string SE_Asia_Standard_Time = "SE Asia Standard Time";

        /// <summary>
        /// StandardName: Altai Standard Time
        /// DisplayName: (UTC+07:00) Barnaul, Gorno-Altaysk
        /// </summary>
        public const string Altai_Standard_Time = "Altai Standard Time";

        /// <summary>
        /// StandardName: W. Mongolia Standard Time
        /// DisplayName: (UTC+07:00) Hovd
        /// </summary>
        public const string W__Mongolia_Standard_Time = "W. Mongolia Standard Time";

        /// <summary>
        /// StandardName: Russia TZ 6 Standard Time
        /// DisplayName: (UTC+07:00) Krasnoyarsk
        /// </summary>
        public const string North_Asia_Standard_Time = "North Asia Standard Time";

        /// <summary>
        /// StandardName: Novosibirsk Standard Time
        /// DisplayName: (UTC+07:00) Novosibirsk
        /// </summary>
        public const string N__Central_Asia_Standard_Time = "N. Central Asia Standard Time";

        /// <summary>
        /// StandardName: Tomsk Standard Time
        /// DisplayName: (UTC+07:00) Tomsk
        /// </summary>
        public const string Tomsk_Standard_Time = "Tomsk Standard Time";

        /// <summary>
        /// StandardName: China Standard Time
        /// DisplayName: (UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi
        /// </summary>
        public const string China_Standard_Time = "China Standard Time";

        /// <summary>
        /// StandardName: Russia TZ 7 Standard Time
        /// DisplayName: (UTC+08:00) Irkutsk
        /// </summary>
        public const string North_Asia_East_Standard_Time = "North Asia East Standard Time";

        /// <summary>
        /// StandardName: Malay Peninsula Standard Time
        /// DisplayName: (UTC+08:00) Kuala Lumpur, Singapore
        /// </summary>
        public const string Singapore_Standard_Time = "Singapore Standard Time";

        /// <summary>
        /// StandardName: W. Australia Standard Time
        /// DisplayName: (UTC+08:00) Perth
        /// </summary>
        public const string W__Australia_Standard_Time = "W. Australia Standard Time";

        /// <summary>
        /// StandardName: Taipei Standard Time
        /// DisplayName: (UTC+08:00) Taipei
        /// </summary>
        public const string Taipei_Standard_Time = "Taipei Standard Time";

        /// <summary>
        /// StandardName: Ulaanbaatar Standard Time
        /// DisplayName: (UTC+08:00) Ulaanbaatar
        /// </summary>
        public const string Ulaanbaatar_Standard_Time = "Ulaanbaatar Standard Time";

        /// <summary>
        /// StandardName: North Korea Standard Time
        /// DisplayName: (UTC+08:30) Pyongyang
        /// </summary>
        public const string North_Korea_Standard_Time = "North Korea Standard Time";

        /// <summary>
        /// StandardName: Aus Central W. Standard Time
        /// DisplayName: (UTC+08:45) Eucla
        /// </summary>
        public const string Aus_Central_W__Standard_Time = "Aus Central W. Standard Time";

        /// <summary>
        /// StandardName: Transbaikal Standard Time
        /// DisplayName: (UTC+09:00) Chita
        /// </summary>
        public const string Transbaikal_Standard_Time = "Transbaikal Standard Time";

        /// <summary>
        /// StandardName: Tokyo Standard Time
        /// DisplayName: (UTC+09:00) Osaka, Sapporo, Tokyo
        /// </summary>
        public const string Tokyo_Standard_Time = "Tokyo Standard Time";

        /// <summary>
        /// StandardName: Korea Standard Time
        /// DisplayName: (UTC+09:00) Seoul
        /// </summary>
        public const string Korea_Standard_Time = "Korea Standard Time";

        /// <summary>
        /// StandardName: Russia TZ 8 Standard Time
        /// DisplayName: (UTC+09:00) Yakutsk
        /// </summary>
        public const string Yakutsk_Standard_Time = "Yakutsk Standard Time";

        /// <summary>
        /// StandardName: Cen. Australia Standard Time
        /// DisplayName: (UTC+09:30) Adelaide
        /// </summary>
        public const string Cen__Australia_Standard_Time = "Cen. Australia Standard Time";

        /// <summary>
        /// StandardName: AUS Central Standard Time
        /// DisplayName: (UTC+09:30) Darwin
        /// </summary>
        public const string AUS_Central_Standard_Time = "AUS Central Standard Time";

        /// <summary>
        /// StandardName: E. Australia Standard Time
        /// DisplayName: (UTC+10:00) Brisbane
        /// </summary>
        public const string E__Australia_Standard_Time = "E. Australia Standard Time";

        /// <summary>
        /// StandardName: AUS Eastern Standard Time
        /// DisplayName: (UTC+10:00) Canberra, Melbourne, Sydney
        /// </summary>
        public const string AUS_Eastern_Standard_Time = "AUS Eastern Standard Time";

        /// <summary>
        /// StandardName: West Pacific Standard Time
        /// DisplayName: (UTC+10:00) Guam, Port Moresby
        /// </summary>
        public const string West_Pacific_Standard_Time = "West Pacific Standard Time";

        /// <summary>
        /// StandardName: Tasmania Standard Time
        /// DisplayName: (UTC+10:00) Hobart
        /// </summary>
        public const string Tasmania_Standard_Time = "Tasmania Standard Time";

        /// <summary>
        /// StandardName: Russia TZ 9 Standard Time
        /// DisplayName: (UTC+10:00) Vladivostok
        /// </summary>
        public const string Vladivostok_Standard_Time = "Vladivostok Standard Time";

        /// <summary>
        /// StandardName: Lord Howe Standard Time
        /// DisplayName: (UTC+10:30) Lord Howe Island
        /// </summary>
        public const string Lord_Howe_Standard_Time = "Lord Howe Standard Time";

        /// <summary>
        /// StandardName: Bougainville Standard Time
        /// DisplayName: (UTC+11:00) Bougainville Island
        /// </summary>
        public const string Bougainville_Standard_Time = "Bougainville Standard Time";

        /// <summary>
        /// StandardName: Russia TZ 10 Standard Time
        /// DisplayName: (UTC+11:00) Chokurdakh
        /// </summary>
        public const string Russia_Time_Zone_10 = "Russia Time Zone 10";

        /// <summary>
        /// StandardName: Magadan Standard Time
        /// DisplayName: (UTC+11:00) Magadan
        /// </summary>
        public const string Magadan_Standard_Time = "Magadan Standard Time";

        /// <summary>
        /// StandardName: Norfolk Standard Time
        /// DisplayName: (UTC+11:00) Norfolk Island
        /// </summary>
        public const string Norfolk_Standard_Time = "Norfolk Standard Time";

        /// <summary>
        /// StandardName: Sakhalin Standard Time
        /// DisplayName: (UTC+11:00) Sakhalin
        /// </summary>
        public const string Sakhalin_Standard_Time = "Sakhalin Standard Time";

        /// <summary>
        /// StandardName: Central Pacific Standard Time
        /// DisplayName: (UTC+11:00) Solomon Is., New Caledonia
        /// </summary>
        public const string Central_Pacific_Standard_Time = "Central Pacific Standard Time";

        /// <summary>
        /// StandardName: Russia TZ 11 Standard Time
        /// DisplayName: (UTC+12:00) Anadyr, Petropavlovsk-Kamchatsky
        /// </summary>
        public const string Russia_Time_Zone_11 = "Russia Time Zone 11";

        /// <summary>
        /// StandardName: New Zealand Standard Time
        /// DisplayName: (UTC+12:00) Auckland, Wellington
        /// </summary>
        public const string New_Zealand_Standard_Time = "New Zealand Standard Time";

        /// <summary>
        /// StandardName: UTC+12
        /// DisplayName: (UTC+12:00) Coordinated Universal Time+12
        /// </summary>
        public const string UTC_Plus_12 = "UTC+12";

        /// <summary>
        /// StandardName: Fiji Standard Time
        /// DisplayName: (UTC+12:00) Fiji
        /// </summary>
        public const string Fiji_Standard_Time = "Fiji Standard Time";

        /// <summary>
        /// StandardName: Kamchatka Standard Time
        /// DisplayName: (UTC+12:00) Petropavlovsk-Kamchatsky - Old
        /// </summary>
        public const string Kamchatka_Standard_Time = "Kamchatka Standard Time";

        /// <summary>
        /// StandardName: Chatham Islands Standard Time
        /// DisplayName: (UTC+12:45) Chatham Islands
        /// </summary>
        public const string Chatham_Islands_Standard_Time = "Chatham Islands Standard Time";

        /// <summary>
        /// StandardName: UTC+13
        /// DisplayName: (UTC+13:00) Coordinated Universal Time+13
        /// </summary>
        public const string UTC_Plus_13 = "UTC+13";

        /// <summary>
        /// StandardName: Tonga Standard Time
        /// DisplayName: (UTC+13:00) Nuku'alofa
        /// </summary>
        public const string Tonga_Standard_Time = "Tonga Standard Time";

        /// <summary>
        /// StandardName: Samoa Standard Time
        /// DisplayName: (UTC+13:00) Samoa
        /// </summary>
        public const string Samoa_Standard_Time = "Samoa Standard Time";

        /// <summary>
        /// StandardName: Line Islands Standard Time
        /// DisplayName: (UTC+14:00) Kiritimati Island
        /// </summary>
        public const string Line_Islands_Standard_Time = "Line Islands Standard Time";
    }
}
