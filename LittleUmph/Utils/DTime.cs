using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace LittleUmph
{
    /// <summary>
    /// Common date time functions.
    /// </summary>
    public class DTime
    {
        #region [ Time Format ]
        /// <summary>
        /// Format string into 24 hour clock with only hour and minute. Example 17:34.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string Format(DateTime t, TimeFormatType type)
        {
            switch (type)
            {
                case TimeFormatType.Time24HoursNoSecond:
                    return t.ToString("HH:mm");
                case TimeFormatType.Time24HoursWithSecond:
                    return t.ToString("HH:mm:ss");
                case TimeFormatType.Time24HoursWithmillisecond:
                    return t.ToString("HH:mm:ss.fff");
                case TimeFormatType.Date24HoursAndmillisecond:
                    return t.ToString("dd/MM/yyyy HH:mm:ss.fff");
                default:
                    return t.ToLongDateString();
            }
        }
        #endregion

        #region [ DateTime Functions ]
        /// <summary>
        /// Gets the start of the month of the specified date.
        /// </summary>
        /// <param name="dateTime">The date timeFrame.</param>
        /// <returns></returns>
        public static DateTime GetStartOfMonth(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0);
        }

        /// <summary>
        /// Gets the date end of specified month.
        /// </summary>
        /// <param name="dateTime">The date timeFrame.</param>
        /// <returns></returns>
        public static DateTime GetEndOfMonth(DateTime dateTime)
        {
            DateTime endDate = dateTime.AddMonths(1).AddDays(-dateTime.Day);
            return new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59, 999);
        }

        /// <summary>
        /// Round the datetime
        /// </summary>
        /// <example>Round(dt, TimeSpan.FromMinutes(5)); => round the time to the nearest 5 minutes.</example>
        /// <param name = "dateTime"></param>
        /// <param name = "roundBy">The time use to round the time to</param>
        /// <returns></returns>        
        public static DateTime Round(DateTime dateTime, TimeSpan roundBy)
        {
            long remainder = dateTime.Ticks % roundBy.Ticks;
            if (remainder < roundBy.Ticks / 2)
            {
                // round down
                return dateTime.AddTicks(-remainder);
            }

            // round up
            return dateTime.AddTicks(roundBy.Ticks - remainder);
        }
        #endregion

        #region [ SetSystemDateTime ]
        /// <summary> This structure represents a date and time. </summary>
        private struct SystemTime
        {
            public ushort wYear, wMonth, wDayOfWeek, wDay,
               wHour, wMinute, wSecond, wMilliseconds;
        }

        /// <summary>
        /// This function retrieves the current system date
        /// and time expressed in Coordinated Universal Time (UTC).
        /// </summary>
        /// <param name="lpSystemTime">[out] Pointer to a SYSTEMTIME structure to
        /// receive the current system date and time.</param>
        [DllImport("kernel32.dll")]
        private extern static void GetSystemTime(ref SystemTime lpSystemTime);

        /// <summary>
        /// This function sets the current system date
        /// and time expressed in Coordinated Universal Time (UTC).
        /// </summary>
        /// <param name="lpSystemTime">[in] Pointer to a SYSTEMTIME structure that
        /// Contains the current system date and time.</param>
        [DllImport("kernel32.dll")]
        private extern static uint SetSystemTime(ref SystemTime lpSystemTime);

        /// <summary>
        /// Change the system time.
        /// </summary>
        /// <param name="datetime">The datetime.</param>
        public static void SetSystemDateTime(DateTime datetime)
        {
            datetime = datetime.ToUniversalTime();

            SystemTime st = new SystemTime();
            st.wYear = (ushort)datetime.Year;
            st.wMonth = (ushort)datetime.Month;
            st.wDay = (ushort)datetime.Day;
            st.wHour = (ushort)datetime.Hour;
            st.wMinute = (ushort)datetime.Minute;
            st.wSecond = (ushort)datetime.Second;
            st.wMilliseconds = (ushort)datetime.Millisecond;

            SetSystemTime(ref st);
        }
        #endregion

        #region [ IsEqualByFormat ]
        /// <summary>
        /// Compare two date only by their formated form.
        /// </summary>
        /// <param name="a">First Date</param>
        /// <param name="b">The Second Date</param>
        /// <param name="format">The format use to generate the string.</param>
        /// <returns>
        /// 	<c>true</c> if the two date are equal by date format; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEqualByFormat(DateTime a, DateTime b, TimeFormatType format)
        {
            return DTime.Format(a, format) == DTime.Format(b, format);
        }

        /// <summary>
        /// Determines whether the two dates is equal 
        /// (useful to determine if the certain part of the date is equal).
        /// </summary>
        /// <example>Just want to know if the two dates have the same year and month and ignore the rest.</example>
        /// <param name="a">First date</param>
        /// <param name="b">Second date</param>
        /// <param name="format">The format.</param>
        /// <returns>
        /// 	<c>true</c> if [is equal by date format] [the specified a]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEqualByFormat(DateTime a, DateTime b, IFormatProvider format)
        {
            return a.ToString(format).Equals(b.ToString(format));
        }
        #endregion

        #region [ UnixTimestamp ]
        /// <summary>
        /// Convert Unix to time to date and time format
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime FromUnixTimestamp(int timestamp)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(timestamp).ToLocalTime();
        }

        /// <summary>
        /// Convert date and time to unix timestamp format
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int ToUnixTimestamp(DateTime date)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date - epoch;
            return Convert.ToInt32(diff.TotalSeconds);
        }
        #endregion

        #region [ Christian Dates ]
        /// <summary>
        /// Easters sunday.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        /// <remarks>http://stackoverflow.com/questions/2510383/how-can-i-calculate-what-date-good-friday-falls-on-given-a-year</remarks>
        public static DateTime EasterSunday(int year)
        {
            int day = 0;
            int month = 0;

            int g = year % 19;
            int c = year / 100;
            int h = (c - (int)(c / 4) - (int)((8 * c + 13) / 25) + 19 * g + 15) % 30;
            int i = h - (int)(h / 28) * (1 - (int)(h / 28) * (int)(29 / (h + 1)) * (int)((21 - g) / 11));

            day = i - ((year + (int)(year / 4) + i + 2 - c + (int)(c / 4)) % 7) + 28;
            month = 3;

            if (day > 31)
            {
                month++;
                day -= 31;
            }

            return new DateTime(year, month, day);
        }

        /// <summary>
        /// Goods friday.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        public static DateTime GoodFriday(int year)
        {
            return EasterSunday(year).AddDays(-2);
        }

        /// <summary>
        /// Ascensions day.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        /// <remarks>http://www.codeproject.com/Articles/10860/Calculating-Christian-Holidays</remarks>
        public static DateTime AscensionDay(int year)
        {
            return EasterSunday(year).AddDays(39);
        }


        /// <summary>
        /// Pentecosts/Whit sunday.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        public static DateTime PentecostSunday(int year)
        {
            return EasterSunday(year).AddDays(49);
        }

        /// <summary>
        /// Firsts the sunday of advent.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        public static DateTime FirstSundayOfAdvent(int year)
        {
            int weeks = 4;
            int correction = 0;
            DateTime christmas = new DateTime(year, 12, 25);

            if (christmas.DayOfWeek != DayOfWeek.Sunday)
            {
                weeks--;
                correction = ((int)christmas.DayOfWeek - (int)DayOfWeek.Sunday);
            }
            return christmas.AddDays(-1 * ((weeks * 7) + correction));
        }

        /// <summary>
        /// Calculate Palm Sunday
        /// </summary>
        /// <remarks>
        /// Palm Sunday is the sunday one week before easter.
        /// </remarks>
        /// <param name="year">4 digit year (but not before 1583)</param>
        /// <returns>DateTime</returns>
        public static DateTime PalmSunday(int year)
        {
            return EasterSunday(year).AddDays(-7);
        }

        /// <summary>
        /// Calculate Ash Wednesday
        /// </summary>
        /// <remarks>
        /// Ash Wednesday marks the start of Lent. This is the 40 day period between before Easter
        /// </remarks>
        /// <param name="year">4 digit year (but not before 1583)</param>
        /// <returns>DateTime</returns>
        public static DateTime AshWednesday(int year)
        {
            return EasterSunday(year).AddDays(-46);
        }
        #endregion
    }
}

/*
 * http://www.codecogs.com/code/units/date/eastersunday.php
 *
    Septuagesima Sunday = easterSunday(...) -63     ninth Sunday before Easter, the third before Ash Wednesday
    Sexagesima Sunday = easterSunday(...) -56       name for the second Sunday before Ash Wednesday
    Shrove Sunday = easterSunday(...) -49           sunday b4 ash
    Shrove Tuesday = easterSunday(...) -47          tue b4 ash
    Ash Wednesday = easterSunday(...) -46           Lent Start
    Mothers Day (UK only)= easterSunday(...) -21
    Passion Sunday = easterSunday(...) -14          Fifth Sunday of Lent
    Palm Sunday = easterSunday(...) -7
    Holy or Maundy Thursday = easterSunday(...) -3
    Good Friday = easterSunday(...) -2
    Rogation Sunday = easterSunday(...) +35
    Ascension Day = easterSunday(...) +39
    Pentecost or Whitsunday = easterSunday(...) +49
    Whitmundy = easterSunday(...) +50
    Trinity Sunday = easterSunday(...) +56
    Corpus Christi = easterSunday(...) +60 (or easterSunday(...)+63 Catholic Church in the United States)
*/