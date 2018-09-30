using System.Threading.Tasks;
using CurrencyRateProvider.Common.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CurrencyRateProvider.Common.Services
{
    public abstract class BaseCurrencyService
    {
        protected readonly DbContext DbContext;
        protected readonly Currency RelativeCurrency;

        protected BaseCurrencyService(DbContext dbContext, string relativeCurrencyCode, int relativeCurrencyAmount)
        {
            DbContext = dbContext;
            RelativeCurrency = GetOrInsert(relativeCurrencyCode, relativeCurrencyAmount).Result;
        }

        protected async Task<Currency> GetOrInsert(string code, int amount)
        {
            code = code.ToUpper();
            var result = await DbContext
                .Set<Currency>()
                .FirstOrDefaultAsync(x => x.Code == code
                                          && x.Amount == amount);

            if (result != null)
            {
                return result;
            }

            result = (await DbContext.Set<Currency>().AddAsync(new Currency
            {
                Amount = amount,
                Code = code
            })).Entity;

            DbContext.SaveChanges();

            return result;
        }
    }
}
