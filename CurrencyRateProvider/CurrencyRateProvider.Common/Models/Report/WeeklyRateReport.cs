using System.Collections.Generic;
using System.Linq;
using CurrencyRateProvider.Common.DAL.Entities;

namespace CurrencyRateProvider.Common.Models.Report
{
    /// <summary>
    /// Недельный отчет о курсе валют
    /// </summary>
    public class WeeklyRateReport
    {
        public WeeklyRateReport(IEnumerable<string> currencyCodes)
        {
            RateStatistics = new Dictionary<string, RateStatistic>();
            foreach (var currencyCode in currencyCodes)
            {
                RateStatistics.Add(currencyCode, new RateStatistic());
            }
        }
        
        /// <summary>
        /// День начала рабочей недели
        /// </summary>
        public int StartDay { get; set; }

        /// <summary>
        /// День окончания рабочей недели
        /// </summary>
        public int EndDay { get; set; }

        /// <summary>
        /// Статистика по валютам
        /// </summary>
        public Dictionary<string, RateStatistic> RateStatistics { get; set; }

        /// <summary>
        /// Добавить статистику по валютам
        /// </summary>
        /// <param name="rates"></param>
        /// <param name="currencies"></param>
        public void AddStatistic(IList<Rate> rates, IList<Currency> currencies)
        {
            foreach (var rateStatistic in RateStatistics)
            {
                foreach (var currency in currencies.Where(x => x.Code == rateStatistic.Key))
                {
                    var rate = rates.FirstOrDefault(x => x.CurrencyId == currency.Id);
                    if (rate == null)
                    {
                        continue;
                    }
                    rateStatistic.Value.AddCost(rate.Cost, rate.Currency.Amount);
                }
            }
        }
    }
}
