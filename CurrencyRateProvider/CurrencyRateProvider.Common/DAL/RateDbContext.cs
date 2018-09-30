using CurrencyRateProvider.Common.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CurrencyRateProvider.Common.DAL
{
    public class RateDbContext : DbContext
    {
        public RateDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Currency> Currencies { get; set; }

        public DbSet<Rate> Rates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Currency>().HasIndex(c => new { c.Code, c.Amount }).IsUnique(true);
            modelBuilder.Entity<Rate>().HasIndex(r => new { r.CurrencyId, r.RelativeCurrencyId, r.Date }).IsUnique(true);
        }
    }
}
