using System;
using System.Threading.Tasks;
using CurrencyRateProvider.Common.Interfaces;
using CurrencyRateProvider.Common.Models;
using CurrencyRateProvider.Common.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CurrencyRateProvider.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RateController : ControllerBase
    {
        private readonly IReportService _reportService;

        public RateController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<ActionResult<string>> Get(int year, int? month, string format)
        {
            var report = await _reportService.GetReport(
                new DateTime(year, month ?? 1, 1),
                new DateTime(year, month ?? 12, DateTime.DaysInMonth(year, month ?? 12)));

            return string.Equals(format, "txt", StringComparison.InvariantCultureIgnoreCase) ? report.ToString() : JsonConvert.SerializeObject(report);
        }
    }
}