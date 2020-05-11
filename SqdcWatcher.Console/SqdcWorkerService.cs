using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XFactory.SqdcWatcher.Core.Interfaces;

namespace XFactory.SqdcWatcher.ConsoleApp
{
    public class SqdcWorkerService : BackgroundService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<SqdcWorkerService> logger;
        private ISqdcWatcher sqdcWatcher;

        public SqdcWorkerService(IServiceScopeFactory serviceScopeFactory, ILogger<SqdcWorkerService> logger)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();
            sqdcWatcher = scope.ServiceProvider.GetRequiredService<ISqdcWatcher>();

            try
            {
                await Task.WhenAll(
                    sqdcWatcher.Start(stoppingToken),
                    WatchForConsoleKeys(sqdcWatcher, stoppingToken));
            }
            catch (OperationCanceledException)
            {
                // Will happen on CTRL+C. Normal case of we want to terminate the app !
            }
        }

        private async Task WatchForConsoleKeys(ISqdcWatcher watcher, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100, cancellationToken);

                if (!Console.KeyAvailable)
                {
                    continue;
                }

                try
                {
                    ConsoleKeyInfo key = Console.ReadKey();
                    Debug.WriteLine($"KEY PRESSED: {key.Key}");
                    if (key.Key == ConsoleKey.F5)
                    {
                        watcher.RequestRefresh();
                    }
                    else if (key.Key == ConsoleKey.F17 || key.Key == ConsoleKey.F6)
                    {
                        watcher.RequestRefresh(true);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError("Error while reading console keys", e);
                }
            }
        }
    }
}