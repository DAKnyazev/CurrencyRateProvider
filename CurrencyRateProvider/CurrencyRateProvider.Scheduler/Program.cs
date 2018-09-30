using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using CurrencyRateProvider.Common.DAL;
using CurrencyRateProvider.Common.Interfaces;
using CurrencyRateProvider.Common.Services;
using Microsoft.EntityFrameworkCore;

namespace CurrencyRateProvider.Scheduler
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureLogging((hostContext, config) =>
                {
                    config.AddConsole();
                    config.AddDebug();
                }).ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddEnvironmentVariables();
                    config.AddJsonFile("appsettings.json", optional: false);
                    config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: false);
                    config.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging();
                    services.AddHostedService<TimedHostedService>();
                    services.AddDbContext<RateDbContext>(options => options.UseNpgsql(hostContext.Configuration.GetConnectionString("RateUser")));
                    services.AddSingleton<DbContext, RateDbContext>();
                    services.AddSingleton<IFillService, FillService>();
                })
                .UseConsoleLifetime()
                .Build();

            using (host)
            {
                // Start the host
                await host.StartAsync();

                // Wait for the host to shutdown
                await host.WaitForShutdownAsync();
            }
        }
    }
}
