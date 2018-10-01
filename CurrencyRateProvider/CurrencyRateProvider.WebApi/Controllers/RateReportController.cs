using System;
using System.Threading.Tasks;
using CurrencyRateProvider.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CurrencyRateProvider.WebApi.Controllers
{
    /// <summary>
    /// Контроллер отчетов по курсам валют
    /// </summary>
    [Route("api/rate/report")]
    [ApiController]
    public class RateReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public RateReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Получить отчет по курсам валют
        /// </summary>
        /// <param name="year">Год</param>
        /// <param name="month">Месяц (если не указан, то отчет строиться за весь год)</param>
        /// <param name="format">Формат</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<string>> Get(int year, int? month, string format)
        {
            var report = await _reportService.GetReportAsync(
                new DateTime(year, month ?? 1, 1),
                new DateTime(year, month ?? 12, DateTime.DaysInMonth(year, month ?? 12)));

            return string.Equals(format, "txt", StringComparison.InvariantCultureIgnoreCase) ? report.ToString() : JsonConvert.SerializeObject(report);
        }
    }
}