using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CurrencyRateProvider.Common.DAL.Entities;
using CurrencyRateProvider.Common.Interfaces;
using CurrencyRateProvider.Common.Models;
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
            _currencies = _dbContext.Set<Currency>().Where(c => _currencyCodes.Contains(c.Code)).ToList();
        }

        public async Task<RateReport> GetReport(DateTime start, DateTime end)
        {
            return new RateReport
            {
                MonthlyReports = await GetMonthlyReports(start.Date, end.Date)
            };
        }

        public async Task<List<MonthlyRateReport>> GetMonthlyReports(DateTime start, DateTime end)
        {
            var result = new List<MonthlyRateReport>();
            var currentDate = start.Date;

            while (currentDate <= end.Date)
            {
                result.Add(await GetMonthlyReport(currentDate));
                currentDate = currentDate.AddMonths(1);
            }

            return result;
        }

        public async Task<MonthlyRateReport> GetMonthlyReport(DateTime date)
        {
            var result = new MonthlyRateReport(date);
            var dateGroups = await _dbContext
                .Set<Rate>()
                .Where(rate => rate.Date.Year == date.Year
                                && rate.Date.Month == date.Month
                                && rate.RelativeCurrencyId == _relativeCurrency.Id
                                && _currencies.Any(x => x.Id == rate.CurrencyId))
                .GroupBy(rate => rate.Date.Date)
                .OrderBy(x => x.Key)
                .ToListAsync();

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
                    result.WeeklyReports.Add(currentWeekReport);
                    currentDate = dateGroup.Key.Date;
                    currentWeekReport = new WeeklyRateReport(_currencyCodes)
                    {
                        StartDay = currentDate.Day
                    };
                }

                currentWeekReport.AddStatistic(dateGroup.ToList(), _currencies);
                currentWeekReport.EndDay = currentDate.Day;
            }

            result.WeeklyReports.Add(currentWeekReport);

            return result;
        }
    }
}
