using CurrencyRateProvider.Common.DAL;
using Microsoft.EntityFrameworkCore;

namespace CurrencyRateProvider.DbFiller.Migrations
{
    public class RateMigrationsDbContext : RateDbContext
    {
        public RateMigrationsDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
