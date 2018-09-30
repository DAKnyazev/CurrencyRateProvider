using System;
using System.Threading;
using System.Threading.Tasks;
using CurrencyRateProvider.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CurrencyRateProvider.Common.Services
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private Timer _timer;
        private readonly IFillService _fillService;
        private readonly int _hour;
        private readonly int _minutes;
        private readonly int _retryDelayInMinutes;
        private readonly int _maxRetryCount;
        private int _retryCount;
        private DateTime _retryDate;

        public TimedHostedService(ILogger<TimedHostedService> logger, IFillService fillService, IConfiguration configuration)
        {
            _logger = logger;
            _fillService = fillService;
            _hour = int.Parse(configuration["TimedHostedService:StartHour"]);
            _minutes = int.Parse(configuration["TimedHostedService:StartMinutes"]);
            _retryDelayInMinutes = int.Parse(configuration["TimedHostedService:RetryDelayInMinutes"]);
            _maxRetryCount = int.Parse(configuration["TimedHostedService:MaxRetryCount"]);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");
            var startTimeSpan = GetNextStartTimeInMinutes(true);

            _timer = new Timer(
                DoWork, 
                null,
                startTimeSpan,
                startTimeSpan);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private async void DoWork(object state)
        {
            _logger.LogInformation("Timed Background Service is working.");

            var startTimeSpan = GetNextStartTimeInMinutes(await _fillService.TryFill(DateTime.Now.Date));

            _timer.Change(startTimeSpan, startTimeSpan);
        }

        private TimeSpan GetNextStartTimeInMinutes(bool isSuccessful)
        {
            if (!isSuccessful)
            {
                if (_retryDate.Date != DateTime.Now.Date)
                {
                    _retryDate = DateTime.Now.Date;
                    _retryCount = 0;
                }

                if (_retryCount < _maxRetryCount)
                {
                    _retryCount++;

                    return TimeSpan.FromMinutes(_retryDelayInMinutes);
                }
            }

            var nextStart = DateTime.Now.Date.AddHours(_hour).AddMinutes(_minutes);

            return (nextStart < DateTime.Now ? nextStart.AddDays(1) : nextStart).Subtract(DateTime.Now);
        }
    }
}
