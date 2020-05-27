using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XFactory.SqdcWatcher.Core.Interfaces;
using XFactory.SqdcWatcher.Core.Utils;

namespace XFactory.SqdcWatcher.ConsoleApp
{
    public class SqdcWorkerService : BackgroundService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private ConsoleInputInterface consoleInputInterface;
        private ISqdcWatcher sqdcWatcher;

        public SqdcWorkerService(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory;
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