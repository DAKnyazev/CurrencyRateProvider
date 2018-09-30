using System;
using System.Threading.Tasks;
using CurrencyRateProvider.Common.Models.Report;

namespace CurrencyRateProvider.Common.Interfaces
{
    public interface IReportService
    {
        Task<RateReport> GetReport(DateTime start, DateTime end);
    }
}
