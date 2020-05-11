using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace XFactory.SqdcWatcher.ConsoleApp
{
    public class ConsoleKeyBindingsService : IHostedService
    {
        private readonly SqdcWorkerService sqdcWorkerService;

        public ConsoleKeyBindingsService(SqdcWorkerService sqdcWorkerService)
        {
            this.sqdcWorkerService = sqdcWorkerService;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => WatchForConsoleKeys(cancellationToken));
            
            return Task.CompletedTask;
        }

        private async Task WatchForConsoleKeys(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100, cancellationToken);
                
                try
                {
                    ConsoleKeyInfo key = Console.ReadKey();
                    Debug.WriteLine($"KEY PRESSED: {key.Key}");
                    if (key.Key == ConsoleKey.F5)
                    {
                        //sqdcWorkerService.SqdcWatcher?.RequestRefresh();
                    }
                    else if (key.Key == ConsoleKey.F17 || key.Key == ConsoleKey.F6)
                    {
                        //sqdcWorkerService.SqdcWatcher?.RequestRefresh(true);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}