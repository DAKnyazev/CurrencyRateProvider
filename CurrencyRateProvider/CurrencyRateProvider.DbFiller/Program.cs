using System;
using System.IO;
using CurrencyRateProvider.Common.DAL;
using CurrencyRateProvider.Common.Interfaces;
using CurrencyRateProvider.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyRateProvider.DbFiller
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();
            var fillService = serviceProvider.GetRequiredService<IFillService>();
            if (fillService.TryFill(1998, 2018).Result)
            {
                Console.WriteLine("Finished successfully");
            }
            else
            {
                Console.WriteLine("Failed");
            }

            Console.ReadKey();
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            var configuration = GetConfiguration();
            services.AddDbContext<RateDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("RateUser")));
            services.AddSingleton<DbContext, RateDbContext>();
            services.AddSingleton<IFillService, FillService>();
            services.AddSingleton<IConfiguration>(configuration);
        }

        public static IConfigurationRoot GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            return builder.Build();
        }
    }
}
