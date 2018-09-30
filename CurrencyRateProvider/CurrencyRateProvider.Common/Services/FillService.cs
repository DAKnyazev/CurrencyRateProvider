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
using RestSharp;

namespace CurrencyRateProvider.Common.Services
{
    public class FillService : IFillService
    {
        private readonly DbContext _dbContext;
        private readonly IRestClient _client;
        private readonly string _yearUrl;
        private readonly string _dailyUrl;
        private readonly Currency _relativeCurrency;
        private readonly string _requestDateFormat;
        private readonly string _responseDateFormat;

        public FillService(DbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _client = new RestClient(configuration["CNBService:HostUrl"]);
            _yearUrl = configuration["CNBService:YearPath"];
            _dailyUrl = configuration["CNBService:DailyPath"];
            _relativeCurrency = GetOrInsert(configuration["RelativeCurrency:Code"], int.Parse(configuration["RelativeCurrency:Amount"])).Result;
            _requestDateFormat = "dd.MM.yyyy";
            _responseDateFormat = "dd.MMM yyyy";
        }

        public async Task<bool> TryFill(int startYear, int endYear)
        {
            try
            {
                for (var year = startYear; year <= endYear; year++)
                {
                    await Insert(await GetCurrencyRates(year));
                }
            }
            catch (Exception exception)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> TryFill(DateTime date)
        {
            try
            {
                await Insert(await GetCurrencyRates(date));
            }
            catch (Exception exception)
            {
                return false;
            }

            return true;
        }

        private async Task<List<Rate>> GetCurrencyRates(int year)
        {
            var result = new List<Rate>();

            var request = new RestRequest(_yearUrl, Method.GET);
            request.AddParameter("year", year);

            var response = _client.Execute(request);

            var rows = Regex.Split(response.Content, "\r\n|\r|\n");
            var currencies = await GetHeaderCurrencies(rows[0]);

            for (var i = 1; i < rows.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(rows[i]))
                {
                    break;
                }

                if (rows[i].Trim().StartsWith("Date", StringComparison.InvariantCultureIgnoreCase))
                {
                    currencies = await GetHeaderCurrencies(rows[i]);
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
                        RelativeCurrency = _relativeCurrency
                    });
                }
            }

            return result;
        }

        private async Task<List<Rate>> GetCurrencyRates(DateTime date)
        {
            var result = new List<Rate>();

            var request = new RestRequest(_dailyUrl, Method.GET);
            request.AddParameter("date", date.ToString(_requestDateFormat));

            var response = _client.Execute(request);

            var rows = Regex.Split(response.Content, "\r\n|\r|\n");
            var responseDate = DateTime.ParseExact(rows[0].Substring(0, rows[0].IndexOf('#')).Trim(), _responseDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);

            if (responseDate.Date != date.Date)
            {
                return result;
            }

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
                    Date = date,
                    Cost = decimal.Parse(columns[4], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                    Currency = await GetOrInsert(columns[3], int.Parse(columns[2])),
                    RelativeCurrency = _relativeCurrency
                });
            }

            return result;
        }

        private async Task<List<Currency>> GetHeaderCurrencies(string header)
        {
            var headerColumns = header.Split("|");
            var result = new List<Currency>(headerColumns.Length - 1);
            for (var i = 1; i < headerColumns.Length; i++)
            {
                var currency = headerColumns[i].Split(' ');
                result.Add(await GetOrInsert(currency[1], int.Parse(currency[0])));
            }

            return result;
        }

        private async Task<Currency> GetOrInsert(string code, int amount)
        {
            code = code.ToUpper();
            var result = await _dbContext
                .Set<Currency>()
                .FirstOrDefaultAsync(x => x.Code == code
                                          && x.Amount == amount);

            if (result != null)
            {
                return result;
            }

            result = (await _dbContext.Set<Currency>().AddAsync(new Currency
                {
                    Amount = amount,
                    Code = code
                })).Entity;

            _dbContext.SaveChanges();

            return result;
        }

        private async Task Insert(IEnumerable<Rate> rates)
        {
            rates = rates
                .Where(rate =>
                    !_dbContext
                        .Set<Rate>()
                        .Any(x => x.RelativeCurrencyId == rate.RelativeCurrencyId
                                  && x.CurrencyId == rate.CurrencyId
                                  && x.Date.Year == rate.Date.Year
                                  && x.Date.Month == rate.Date.Month
                                  && x.Date.Day == rate.Date.Day));
            await _dbContext.Set<Rate>().AddRangeAsync(rates);
            _dbContext.SaveChanges();
        }
    }
}
