using System.Collections.Generic;
using System.Text;

namespace CurrencyRateProvider.Common.Models.Report
{
    /// <summary>
    /// Отчет о курсах валют по месяцам
    /// </summary>
    public class RateReport
    {
        public RateReport()
        {
            MonthlyReports = new List<MonthlyRateReport>();
        }

        /// <summary>
        /// Список отчетов валют по месяцам
        /// </summary>
        public List<MonthlyRateReport> MonthlyReports { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var report in MonthlyReports)
            {
                builder.Append(report);
            }

            return builder.ToString();
        }
    }
}
