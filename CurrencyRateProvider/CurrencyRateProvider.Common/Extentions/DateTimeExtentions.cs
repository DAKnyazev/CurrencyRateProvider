using System;
using System.Globalization;

namespace CurrencyRateProvider.Common.Extentions
{
    public static class DateTimeExtentions
    {
        /// <summary>
        /// На одной ли неделе даты?
        /// </summary>
        /// <param name="date">Дата</param>
        /// <param name="dateToCompare">Дата для сравнения</param>
        public static bool IsWeekEquals(this DateTime date, DateTime dateToCompare)
        {
            return GetWeekNumber(date) == GetWeekNumber(dateToCompare);
        }

        /// <summary>
        /// Получения номера недели
        /// </summary>
        /// <param name="date">Дата</param>
        public static int GetWeekNumber(DateTime date)
        {
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay,
                DayOfWeek.Monday);
        }
    }
}
