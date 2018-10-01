using System;
using System.Threading.Tasks;
using CurrencyRateProvider.Common.Models.Report;

namespace CurrencyRateProvider.Common.Interfaces
{
    public interface IReportService
    {
        /// <summary>
        /// Получить отчет
        /// </summary>
        /// <param name="start">Начальный месяц/год отчетов</param>
        /// <param name="end">Последний месяц/год отчетов</param>
        Task<RateReport> GetReportAsync(DateTime start, DateTime end);
    }
}
