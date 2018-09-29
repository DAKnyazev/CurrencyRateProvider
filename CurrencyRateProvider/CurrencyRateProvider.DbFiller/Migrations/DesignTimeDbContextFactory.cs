using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CurrencyRateProvider.DbFiller.Migrations
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RateMigrationsDbContext>
    {
        public RateMigrationsDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var builder = new DbContextOptionsBuilder<RateMigrationsDbContext>();
            var connectionString = configuration.GetConnectionString("RateAdmin");
            builder.UseNpgsql(connectionString);
            return new RateMigrationsDbContext(builder.Options);
        }
    }
}
