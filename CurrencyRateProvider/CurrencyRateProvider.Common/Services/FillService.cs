using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CurrencyRateProvider.Common.DAL.Entities;
using CurrencyRateProvider.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace CurrencyRateProvider.Common.Services
{
    public class FillService : IFillService
    {
        private readonly DbContext _dbContext;
        private readonly IRestClient _client;
        private readonly string _yearUrl;
        private readonly Currency _relativeCurrency;

        public FillService(DbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _client = new RestClient(configuration["CNBService:HostUrl"]);
            _yearUrl = configuration["CNBService:YearPath"];
            _relativeCurrency = GetOrInsert(configuration["RelativeCurrency:Code"], int.Parse(configuration["RelativeCurrency:Amount"])).Result;
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

        public Task<bool> TryFill(DateTime day)
        {
            throw new NotImplementedException();
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
                var date = DateTime.ParseExact(columns[0], "dd.MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);

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
            var result = await _dbContext
                .Set<Currency>()
                .FirstOrDefaultAsync(x => x.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase) 
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
            await _dbContext.Set<Rate>().AddRangeAsync(rates);
            _dbContext.SaveChanges();
        }
    }
}
