using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SqdcWatcher.DataObjects;

namespace SqdcWatcher.Services
{
    internal enum WatcherStatus
    {
        Idle = 0,
        Running,
        Stopping
    }
    
    public class SqdcHttpWatcher : ISqdcWatcher
    {
        private readonly SqdcProductsFetcher productsFetcher;
        private readonly ProductsPersister productsPersister;
        private WatcherStatus status;

        public SqdcHttpWatcher(SqdcProductsFetcher productsFetcher, ProductsPersister productsPersister)
        {
            this.productsFetcher = productsFetcher;
            this.productsPersister = productsPersister;
        }
        
        public void start(CancellationToken cancellationToken)
        {
            if(status == WatcherStatus.Running || status == WatcherStatus.Stopping)
            {
                throw new WatcherStartException("cannot start watcher, it is already running.");
            }

            status = WatcherStatus.Running;
            Task.Run(async () => await Loop(cancellationToken), cancellationToken).Wait();
        }

        private async Task Loop(CancellationToken cancelToken)
        {
            while(!cancelToken.IsCancellationRequested)
            {
                List<Product> products = await productsFetcher.GetProducts();
                productsPersister.PersistMergeProducts(products);

                //Console.WriteLine(SqdcFormatter.FormatProductsSummaries(products, ProductFormatStyle.Table));
                Console.WriteLine($"Found {products.Count} products");

                Console.WriteLine("Waiting 15 minutes until next execution");
                await Task.Delay(TimeSpan.FromMinutes(15));
            }
        }
    }
}