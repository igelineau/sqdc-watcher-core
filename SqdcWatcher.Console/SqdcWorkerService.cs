using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XFactory.SqdcWatcher.Core.Interfaces;
using XFactory.SqdcWatcher.Core.Utils;

namespace XFactory.SqdcWatcher.ConsoleApp
{
    public class SqdcWorkerService : BackgroundService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<SqdcWorkerService> logger;
        private ISqdcWatcher sqdcWatcher;
        private ConsoleInputInterface consoleInputInterface;

        public SqdcWorkerService(IServiceScopeFactory serviceScopeFactory, ILogger<SqdcWorkerService> logger)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();
            sqdcWatcher = scope.ServiceProvider.GetRequiredService<ISqdcWatcher>();
            consoleInputInterface = scope.ServiceProvider.GetRequiredService<ConsoleInputInterface>();
            await StartServices(stoppingToken);
        }

        private async Task StartServices(CancellationToken stoppingToken)
        {
            Task aggregatedTasks = Task.WhenAll(
                sqdcWatcher.Start(stoppingToken),
                consoleInputInterface.StartReadingConsoleKeysAsync(stoppingToken));
            await TaskAwaiterHelper.AwaitIgnoringExceptionAsync<OperationCanceledException>(aggregatedTasks);
        }
    }
}