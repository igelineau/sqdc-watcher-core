using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SqdcWatcher
{
    internal enum WatcherStatus
    {
        Idle = 0,
        Running,
        Stopping
    }
    
    public class SqdcHttpWatcher : ISqdcWatcher
    {
        private readonly ISqdcClient sqdClient;
        private WatcherStatus status;

        public SqdcHttpWatcher(ISqdcClient sqdcClient)
        {
            this.sqdClient = sqdcClient;
        }

        public void start(CancellationToken cancellationToken)
        {
            if(status == WatcherStatus.Running || status == WatcherStatus.Stopping)
            {
                throw new WatcherStartException("cannot start watcher, it is already running.");
            }

            status = WatcherStatus.Running;
            Task.Run(async () => await loop(cancellationToken), cancellationToken).Wait();
        }

        private async Task loop(CancellationToken cancelToken)
        {
            while(!cancelToken.IsCancellationRequested)
            {
                List<ProductSummary> products = await sqdClient.GetProductSummaries();

                Console.WriteLine(SqdcFormatter.FormatProductsSummaries(products, ProductFormatStyle.Table));
                Console.WriteLine($"Found {products.Count} products");

                Console.WriteLine("Waiting 15 minutes until next execution");
                await Task.Delay(TimeSpan.FromMinutes(15));
            }
        }
    }
}