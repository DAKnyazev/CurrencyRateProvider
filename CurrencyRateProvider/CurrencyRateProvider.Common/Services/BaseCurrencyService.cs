using System.Threading.Tasks;
using CurrencyRateProvider.Common.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CurrencyRateProvider.Common.Services
{
    public abstract class BaseCurrencyService
    {
        protected readonly DbContext _dbContext;
        protected readonly Currency _relativeCurrency;

        protected BaseCurrencyService(DbContext dbContext, string relativeCurrencyCode, int relativeCurrencyAmount)
        {
            _dbContext = dbContext;
            _relativeCurrency = GetOrInsert(relativeCurrencyCode, relativeCurrencyAmount).Result;
        }

        protected async Task<Currency> GetOrInsert(string code, int amount)
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
    }
}
