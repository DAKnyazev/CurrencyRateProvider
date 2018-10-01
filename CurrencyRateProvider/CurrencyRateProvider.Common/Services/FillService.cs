using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CurrencyRateProvider.Common.DAL.Entities;
using CurrencyRateProvider.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace CurrencyRateProvider.Common.Services
{
    public class FillService : BaseCurrencyService, IFillService
    {
        private readonly ILogger _logger;
        private readonly IRestClient _client;
        private readonly string _yearUrl;
        private readonly string _dailyUrl;
        private readonly string _requestDateFormat;
        private readonly string _responseDateFormat;

        public FillService(DbContext dbContext, IConfiguration configuration, ILogger<FillService> logger) 
            : base(
                dbContext,
                configuration["FillService:RelativeCurrency:Code"],
                int.Parse(configuration["FillService:RelativeCurrency:Amount"]))
        {
            _client = new RestClient(configuration["FillService:HostUrl"]);
            _yearUrl = configuration["FillService:YearPath"];
            _dailyUrl = configuration["FillService:DailyPath"];
            _requestDateFormat = "dd.MM.yyyy";
            _responseDateFormat = "dd.MMM yyyy";
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<bool> TryFillAsync(int startYear, int endYear)
        {
            var year = 0;
            try
            {
                for (year = startYear; year <= endYear; year++)
                {
                    var count = await InsertAsync(await GetCurrencyRatesAsync(year));
                    _logger.LogInformation($"TryFillAsync method: {count} rate rows inserted.");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"TryFillAsync method failed at {year} year.", null);
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> TryFillAsync(DateTime date)
        {
            try
            {
                var count = await InsertAsync(await GetCurrencyRatesAsync(date));
                _logger.LogInformation($"TryFillAsync method: {count} rate rows inserted.");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"TryFillAsync method failed at {date:_requestDateFormat} date.", null);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Получить данные о курсах валют за выбранный год
        /// </summary>
        /// <param name="year">Год</param>
        private async Task<List<Rate>> GetCurrencyRatesAsync(int year)
        {
            var result = new List<Rate>();

            var request = new RestRequest(_yearUrl, Method.GET);
            request.AddParameter("year", year);

            var response = _client.Execute(request);

            var rows = Regex.Split(response.Content, "\r\n|\r|\n");
            var currencies = await GetCurrenciesAsync(rows[0]);

            for (var i = 1; i < rows.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(rows[i]))
                {
                    break;
                }

                if (rows[i].Trim().StartsWith("Date", StringComparison.InvariantCultureIgnoreCase))
                {
                    currencies = await GetCurrenciesAsync(rows[i]);
                    continue;
                }
                var columns = rows[i].Split("|");
                var date = DateTime.ParseExact(columns[0], _responseDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);

                for (var j = 1; j < columns.Length; j++)
                {
                    result.Add(new Rate
                    {
                        Date = date,
                        Cost = decimal.Parse(columns[j], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                        Currency = currencies[j - 1],
                        RelativeCurrency = RelativeCurrency
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Получить данные о курсах валют за выбранный день
        /// </summary>
        /// <param name="date">День</param>
        private async Task<List<Rate>> GetCurrencyRatesAsync(DateTime date)
        {
            var result = new List<Rate>();

            var request = new RestRequest(_dailyUrl, Method.GET);
            request.AddParameter("date", date.ToString(_requestDateFormat));

            var response = _client.Execute(request);

            var rows = Regex.Split(response.Content, "\r\n|\r|\n");
            var responseDate = DateTime.ParseExact(rows[0].Substring(0, rows[0].IndexOf('#')).Trim(), _responseDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);

            if (rows.Length < 2)
            {
                return result;
            }

            for (var i = 2; i < rows.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(rows[i]))
                {
                    break;
                }

                var columns = rows[i].Split("|");

                result.Add(new Rate
                {
                    Date = responseDate.Date,
                    Cost = decimal.Parse(columns[4], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                    Currency = await GetOrInsertAsync(columns[3], int.Parse(columns[2])),
                    RelativeCurrency = RelativeCurrency
                });
            }

            return result;
        }

        /// <summary>
        /// Получение списка валют из заголовка годового ответа
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        private async Task<List<Currency>> GetCurrenciesAsync(string header)
        {
            var headerColumns = header.Split("|");
            var result = new List<Currency>(headerColumns.Length - 1);
            for (var i = 1; i < headerColumns.Length; i++)
            {
                var currency = headerColumns[i].Split(' ');
                result.Add(await GetOrInsertAsync(currency[1], int.Parse(currency[0])));
            }

            return result;
        }
        
        /// <summary>
        /// Вставка уникальных курсов валют
        /// </summary>
        /// <param name="rates">Список курсов валют</param>
        private async Task<int> InsertAsync(IEnumerable<Rate> rates)
        {
            rates = rates
                .Where(rate =>
                    !DbContext
                        .Set<Rate>()
                        .Any(x => x.RelativeCurrencyId == rate.RelativeCurrencyId
                                  && x.CurrencyId == rate.CurrencyId
                                  && x.Date.Year == rate.Date.Year
                                  && x.Date.Month == rate.Date.Month
                                  && x.Date.Day == rate.Date.Day))
                .ToList();
            await DbContext.Set<Rate>().AddRangeAsync(rates);
            DbContext.SaveChanges();

            return rates.Count();
        }
    }
}
