using System;

namespace Codout.Framework.Common.Extensions;

/// <summary>
/// Extensões comuns para tipos relacionadas a datas.
/// </summary>
public static class Dates
{
    #region Variáveis
    /// <summary>
    /// Atrás.
    /// </summary>
    public const string Ago = "atrás";

    /// <summary>
    /// Dia.
    /// </summary>
    public const string Day = "dia";

    /// <summary>
    /// Hora.
    /// </summary>
    public const string Hour = "hora";

    /// <summary>
    /// Minuto.
    /// </summary>
    public const string Minute = "minuto";

    /// <summary>
    /// Mês.
    /// </summary>
    public const string Month = "mês";

    /// <summary>
    /// Segundo.
    /// </summary>
    public const string Second = "segundo";

    /// <summary>
    /// Espaço em branco.
    /// </summary>
    public const string Space = " ";

    /// <summary>
    /// Ano.
    /// </summary>
    public const string Year = "ano";
    #endregion

    #region Date Math

    /// <summary>
    /// Returns a date in the past by days.
    /// </summary>
    /// <param name="days">The days.</param>
    /// <returns></returns>
    public static DateTime DaysAgo(this int days)
    {
        var t = new TimeSpan(days, 0, 0, 0);
        return DateTime.Now.Subtract(t);
    }

    /// <summary>
    ///  Returns a date in the future by days.
    /// </summary>
    /// <param name="days">The days.</param>
    /// <returns></returns>
    public static DateTime DaysFromNow(this int days)
    {
        var t = new TimeSpan(days, 0, 0, 0);
        return DateTime.Now.Add(t);
    }

    /// <summary>
    /// Returns a date in the past by hours.
    /// </summary>
    /// <param name="hours">The hours.</param>
    /// <returns></returns>
    public static DateTime HoursAgo(this int hours)
    {
        var t = new TimeSpan(hours, 0, 0);
        return DateTime.Now.Subtract(t);
    }

    /// <summary>
    /// Returns a date in the future by hours.
    /// </summary>
    /// <param name="hours">The hours.</param>
    /// <returns></returns>
    public static DateTime HoursFromNow(this int hours)
    {
        var t = new TimeSpan(hours, 0, 0);
        return DateTime.Now.Add(t);
    }

    /// <summary>
    /// Returns a date in the past by minutes
    /// </summary>
    /// <param name="minutes">The minutes.</param>
    /// <returns></returns>
    public static DateTime MinutesAgo(this int minutes)
    {
        var t = new TimeSpan(0, minutes, 0);
        return DateTime.Now.Subtract(t);
    }

    /// <summary>
    /// Returns a date in the future by minutes.
    /// </summary>
    /// <param name="minutes">The minutes.</param>
    /// <returns></returns>
    public static DateTime MinutesFromNow(this int minutes)
    {
        var t = new TimeSpan(0, minutes, 0);
        return DateTime.Now.Add(t);
    }

    /// <summary>
    /// Gets a date in the past according to seconds
    /// </summary>
    /// <param name="seconds">The seconds.</param>
    /// <returns></returns>
    public static DateTime SecondsAgo(this int seconds)
    {
        var t = new TimeSpan(0, 0, seconds);
        return DateTime.Now.Subtract(t);
    }

    /// <summary>
    /// Gets a date in the future by seconds.
    /// </summary>
    /// <param name="seconds">The seconds.</param>
    /// <returns></returns>
    public static DateTime SecondsFromNow(this int seconds)
    {
        var t = new TimeSpan(0, 0, seconds);
        return DateTime.Now.Add(t);
    }

    #endregion

    #region Diffs

    /// <summary>
    /// Diffs the specified date.
    /// </summary>
    /// <param name="dateOne">The date one.</param>
    /// <param name="dateTwo">The date two.</param>
    /// <returns></returns>
    public static TimeSpan Diff(this DateTime dateOne, DateTime dateTwo)
    {
        var t = dateOne.Subtract(dateTwo);
        return t;
    }

    /// <summary>
    /// Returns a double indicating the number of days between two dates (past is negative)
    /// </summary>
    /// <param name="dateOne">The date one.</param>
    /// <param name="dateTwo">The date two.</param>
    /// <returns></returns>
    public static double DiffDays(this string dateOne, string dateTwo)
    {
        DateTime dtOne;
        DateTime dtTwo;
        if (DateTime.TryParse(dateOne, out dtOne) && DateTime.TryParse(dateTwo, out dtTwo))
            return Diff(dtOne, dtTwo).TotalDays;
        return 0;
    }

    /// <summary>
    /// Returns a double indicating the number of days between two dates (past is negative)
    /// </summary>
    /// <param name="dateOne">The date one.</param>
    /// <param name="dateTwo">The date two.</param>
    /// <returns></returns>
    public static double DiffDays(this DateTime dateOne, DateTime dateTwo)
    {
        return Diff(dateOne, dateTwo).TotalDays;
    }

    /// <summary>
    /// Returns a double indicating the number of days between two dates (past is negative)
    /// </summary>
    /// <param name="dateOne">The date one.</param>
    /// <param name="dateTwo">The date two.</param>
    /// <returns></returns>
    public static double DiffHours(this string dateOne, string dateTwo)
    {
        DateTime dtOne;
        DateTime dtTwo;
        if (DateTime.TryParse(dateOne, out dtOne) && DateTime.TryParse(dateTwo, out dtTwo))
            return Diff(dtOne, dtTwo).TotalHours;
        return 0;
    }

    /// <summary>
    /// Returns a double indicating the number of days between two dates (past is negative)
    /// </summary>
    /// <param name="dateOne">The date one.</param>
    /// <param name="dateTwo">The date two.</param>
    /// <returns></returns>
    public static double DiffHours(this DateTime dateOne, DateTime dateTwo)
    {
        return Diff(dateOne, dateTwo).TotalHours;
    }

    /// <summary>
    /// Returns a double indicating the number of days between two dates (past is negative)
    /// </summary>
    /// <param name="dateOne">The date one.</param>
    /// <param name="dateTwo">The date two.</param>
    /// <returns></returns>
    public static double DiffMinutes(this string dateOne, string dateTwo)
    {
        DateTime dtOne;
        DateTime dtTwo;
        if (DateTime.TryParse(dateOne, out dtOne) && DateTime.TryParse(dateTwo, out dtTwo))
            return Diff(dtOne, dtTwo).TotalMinutes;
        return 0;
    }

    /// <summary>
    /// Returns a double indicating the number of days between two dates (past is negative)
    /// </summary>
    /// <param name="dateOne">The date one.</param>
    /// <param name="dateTwo">The date two.</param>
    /// <returns></returns>
    public static double DiffMinutes(this DateTime dateOne, DateTime dateTwo)
    {
        return Diff(dateOne, dateTwo).TotalMinutes;
    }

    /// <summary>
    /// Displays the difference in time between the two dates. Return example is "12 years 4 months 24 days 8 hours 33 minutes 5 seconds"
    /// </summary>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <returns></returns>
    public static string ReadableDiff(this DateTime startTime, DateTime endTime)
    {
        string result;

        var seconds = endTime.Second - startTime.Second;
        var minutes = endTime.Minute - startTime.Minute;
        var hours = endTime.Hour - startTime.Hour;
        var days = endTime.Day - startTime.Day;
        var months = endTime.Month - startTime.Month;
        var years = endTime.Year - startTime.Year;

        if (seconds < 0)
        {
            minutes--;
            seconds += 60;
        }
        if (minutes < 0)
        {
            hours--;
            minutes += 60;
        }
        if (hours < 0)
        {
            days--;
            hours += 24;
        }

        if (days < 0)
        {
            months--;
            int previousMonth = (endTime.Month == 1) ? 12 : endTime.Month - 1;
            int year = (previousMonth == 12) ? endTime.Year - 1 : endTime.Year;
            days += DateTime.DaysInMonth(year, previousMonth);
        }
        if (months < 0)
        {
            years--;
            months += 12;
        }

        //put this in a readable format
        if (years > 0)
        {
            result = years.Pluralize(Year);
            if (months != 0)
                result += ", " + months.Pluralize(Month);
            result += " " + Ago;
        }
        else if (months > 0)
        {
            result = months.Pluralize(Month);
            if (days != 0)
                result += ", " + days.Pluralize(Day);
            result += " " + Ago;
        }
        else if (days > 0)
        {
            result = days.Pluralize(Day);
            if (hours != 0)
                result += ", " + hours.Pluralize(Hour);
            result += " " + Ago;
        }
        else if (hours > 0)
        {
            result = hours.Pluralize(Hour);
            if (minutes != 0)
                result += ", " + minutes.Pluralize(Minute);
            result += " " + Ago;
        }
        else if (minutes > 0)
            result = minutes.Pluralize(Minute) + " " + Ago;
        else
            result = seconds.Pluralize(Second) + " " + Ago;

        return result;
    }

    #endregion

    #region CountWeekdays
    /// <summary>
    /// Counts the number of weekdays between two dates.
    /// </summary>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <returns></returns>
    public static int CountWeekdays(this DateTime startTime, DateTime endTime)
    {
        TimeSpan ts = endTime - startTime;
        Console.WriteLine(ts.Days);
        int cnt = 0;
        for (int i = 0; i < ts.Days; i++)
        {
            DateTime dt = startTime.AddDays(i);
            if (IsWeekDay(dt))
                cnt++;
        }
        return cnt;
    }
    #endregion

    #region CountWeekends
    /// <summary>
    /// Counts the number of weekends between two dates.
    /// </summary>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <returns></returns>
    public static int CountWeekends(this DateTime startTime, DateTime endTime)
    {
        TimeSpan ts = endTime - startTime;
        Console.WriteLine(ts.Days);
        int cnt = 0;
        for (int i = 0; i < ts.Days; i++)
        {
            DateTime dt = startTime.AddDays(i);
            if (IsWeekEnd(dt))
                cnt++;
        }
        return cnt;
    }
    #endregion

    #region IsDate
    /// <summary>
    /// Verifies if the object is a date
    /// </summary>
    /// <param name="dt">The dt.</param>
    /// <returns>
    /// 	<c>true</c> if the specified dt is date; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsDate(this object dt)
    {
        DateTime newDate;
        return DateTime.TryParse(dt.ToString(), out newDate);
    }
    #endregion

    #region IsWeekDay
    /// <summary>
    /// Checks to see if the date is a week day (Mon - Fri)
    /// </summary>
    /// <param name="dt">The dt.</param>
    /// <returns>
    /// 	<c>true</c> if [is week day] [the specified dt]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsWeekDay(this DateTime dt)
    {
        return (dt.DayOfWeek != DayOfWeek.Saturday && dt.DayOfWeek != DayOfWeek.Sunday);
    }
    #endregion

    #region IsWeekEnd
    /// <summary>
    /// Checks to see if the date is Saturday or Sunday
    /// </summary>
    /// <param name="dt">The dt.</param>
    /// <returns>
    /// 	<c>true</c> if [is week end] [the specified dt]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsWeekEnd(this DateTime dt)
    {
        return (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday);
    }
    #endregion

    #region TimeDiff
    /// <summary>
    /// Displays the difference in time between the two dates. Return example is "12 years 4 months 24 days 8 hours 33 minutes 5 seconds"
    /// </summary>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <returns></returns>
    public static string TimeDiff(this DateTime startTime, DateTime endTime)
    {
        int seconds = endTime.Second - startTime.Second;
        int minutes = endTime.Minute - startTime.Minute;
        int hours = endTime.Hour - startTime.Hour;
        int days = endTime.Day - startTime.Day;
        int months = endTime.Month - startTime.Month;
        int years = endTime.Year - startTime.Year;
        if (seconds < 0)
        {
            minutes--;
            seconds += 60;
        }
        if (minutes < 0)
        {
            hours--;
            minutes += 60;
        }
        if (hours < 0)
        {
            days--;
            hours += 24;
        }
        if (days < 0)
        {
            months--;
            int previousMonth = (endTime.Month == 1) ? 12 : endTime.Month - 1;
            int year = (previousMonth == 12) ? endTime.Year - 1 : endTime.Year;
            days += DateTime.DaysInMonth(year, previousMonth);
        }
        if (months < 0)
        {
            years--;
            months += 12;
        }

        string sYears = FormatString(Year, String.Empty, years);
        string sMonths = FormatString(Month, sYears, months);
        string sDays = FormatString(Day, sMonths, days);
        string sHours = FormatString(Hour, sDays, hours);
        string sMinutes = FormatString(Minute, sHours, minutes);
        string sSeconds = FormatString(Second, sMinutes, seconds);

        return String.Concat(sYears, sMonths, sDays, sHours, sMinutes, sSeconds);
    }
    #endregion

    #region GetFormattedMonthAndDay
    /// <summary>
    /// Given a datetime object, returns the formatted month and day, i.e. "April 15th"
    /// </summary>
    /// <param name="date">The date to extract the string from</param>
    /// <returns></returns>
    public static string GetFormattedMonthAndDay(this DateTime date)
    {
        return String.Concat(String.Format("{0:MMMM}", date), " ", GetDateDayWithSuffix(date));
    }
    #endregion

    #region GetDateDayWithSuffix
    /// <summary>
    /// Given a datetime object, returns the formatted day, "15th"
    /// </summary>
    /// <param name="date">The date to extract the string from</param>
    /// <returns></returns>
    public static string GetDateDayWithSuffix(this DateTime date)
    {
        int dayNumber = date.Day;
        string suffix = "th";

        if (dayNumber == 1 || dayNumber == 21 || dayNumber == 31)
            suffix = "st";
        else if (dayNumber == 2 || dayNumber == 22)
            suffix = "nd";
        else if (dayNumber == 3 || dayNumber == 23)
            suffix = "rd";

        return String.Concat(dayNumber, suffix);
    }
    #endregion

    #region FormatString
    /// <summary>
    /// Remove leading strings with zeros and adjust for singular/plural
    /// </summary>
    /// <param name="str">The STR.</param>
    /// <param name="previousStr">The previous STR.</param>
    /// <param name="t">The t.</param>
    /// <returns></returns>
    private static string FormatString(this string str, string previousStr, int t)
    {
        if ((t == 0) && (previousStr.Length == 0))
            return String.Empty;

        string suffix = (t == 1) ? String.Empty : "s";
        return String.Concat(t, Space, str, suffix, Space);
    }
    #endregion

    #region GetAge
    /// <summary>
    /// Calc age with date at
    /// </summary>
    /// <param name="dateOfBirth">Date of Birth</param>
    /// <param name="dateAsAt">Date as at</param>
    /// <returns></returns>
    public static int GetAge(this DateTime dateOfBirth, DateTime dateAsAt)
    {
        return dateAsAt.Year - dateOfBirth.Year - (dateOfBirth.DayOfYear < dateAsAt.DayOfYear ? 0 : 1);
    }
    #endregion
}
