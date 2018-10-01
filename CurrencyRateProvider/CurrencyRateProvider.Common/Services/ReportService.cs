using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CurrencyRateProvider.Common.DAL.Entities;
using CurrencyRateProvider.Common.Interfaces;
using CurrencyRateProvider.Common.Models.Report;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CurrencyRateProvider.Common.Services
{
    public class ReportService : BaseCurrencyService, IReportService
    {
        private readonly List<string> _currencyCodes;
        private readonly List<Currency> _currencies;

        public ReportService(DbContext dbContext, IConfiguration configuration) 
            : base(
                dbContext, 
                configuration["ReportService:RelativeCurrency:Code"], 
                int.Parse(configuration["ReportService:RelativeCurrency:Amount"]))
        {
            _currencyCodes = configuration.GetSection("ReportService:Currencies").GetChildren().Select(x => x.Value).ToList();
            _currencies = DbContext.Set<Currency>().Where(c => _currencyCodes.Contains(c.Code)).ToList();
        }

        /// <inheritdoc />
        public async Task<RateReport> GetReportAsync(DateTime start, DateTime end)
        {
            return new RateReport
            {
                MonthlyReports = await GetMonthlyReportsAsync(start.Date, end.Date)
            };
        }

        /// <summary>
        /// Получить месячные отчеты
        /// </summary>
        /// <param name="start">Начальный месяц/год отчетов</param>
        /// <param name="end">Последний месяц/год отчетов</param>
        private async Task<List<MonthlyRateReport>> GetMonthlyReportsAsync(DateTime start, DateTime end)
        {
            var result = new List<MonthlyRateReport>();
            var currentDate = start.Date;

            while (currentDate <= end.Date)
            {
                result.Add(await GetMonthlyReportAsync(currentDate));
                currentDate = currentDate.AddMonths(1);
            }

            return result;
        }

        /// <summary>
        /// Сформировать месячный отчет
        /// </summary>
        /// <param name="date">Месяц/год отчета</param>
        private async Task<MonthlyRateReport> GetMonthlyReportAsync(DateTime date)
        {
            var result = new MonthlyRateReport(date);
            var dateGroups = await DbContext
                .Set<Rate>()
                .Where(rate => rate.Date.Year == date.Year
                                && rate.Date.Month == date.Month
                                && rate.RelativeCurrencyId == RelativeCurrency.Id
                                && _currencies.Any(x => x.Id == rate.CurrencyId))
                .GroupBy(rate => rate.Date.Date)
                .OrderBy(x => x.Key)
                .ToListAsync();

            result.WeeklyReports.AddRange(GetWeeklyReports(dateGroups));

            return result;
        }

        /// <summary>
        /// Сформировать недельные отчеты
        /// </summary>
        /// <param name="dateGroups">Сгруппированные по дате данные по курсам валют</param>
        private IEnumerable<WeeklyRateReport> GetWeeklyReports(IList<IGrouping<DateTime, Rate>> dateGroups)
        {
            var result = new List<WeeklyRateReport>();

            if (!dateGroups.Any())
            {
                return result;
            }

            var currentDate = dateGroups[0].Key;
            var currentWeekReport = new WeeklyRateReport(_currencyCodes)
            {
                StartDay = currentDate.Day,
                EndDay = currentDate.Day
            };
            var isFirst = true;
            foreach (var dateGroup in dateGroups)
            {
                if (!isFirst)
                {
                    currentDate = currentDate.AddDays(1);
                }

                isFirst = false;

                if (currentDate.Date != dateGroup.Key.Date)
                {
                    result.Add(currentWeekReport);
                    currentDate = dateGroup.Key.Date;
                    currentWeekReport = new WeeklyRateReport(_currencyCodes)
                    {
                        StartDay = currentDate.Day
                    };
                }

                currentWeekReport.AddStatistic(dateGroup.ToList(), _currencies);
                currentWeekReport.EndDay = currentDate.Day;
            }

            result.Add(currentWeekReport);

            return result;
        }
    }
}
