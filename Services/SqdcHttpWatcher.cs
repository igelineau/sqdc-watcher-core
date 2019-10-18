using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alba.CsConsoleFormat;
using SqdcWatcher.DataObjects;
using SqdcWatcher.Dto;
using SqdcWatcher.RestApiModels.cs;

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
        private readonly DataAccess dataAccess;
        private WatcherStatus status;

        public SqdcHttpWatcher(SqdcProductsFetcher productsFetcher, ProductsPersister productsPersister, DataAccess dataAccess)
        {
            this.productsFetcher = productsFetcher;
            this.productsPersister = productsPersister;
            this.dataAccess = dataAccess;
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
                Dictionary<string, Product> dbProducts = dataAccess.GetProducts();
                Stopwatch sw = Stopwatch.StartNew();
                ProductsListResult apiFetchResult = await productsFetcher.FetchProductsFromApi(new GetProductsInfoDto
                {
                    VariantsWithUpToDateSpecs = new HashSet<long>(dbProducts.Values
                        .SelectMany(v => v.Variants.Where(var => var.HasSpecifications()))
                        .Select(v => v.Id))
                });
                Console.WriteLine($"API refresh completed in {Math.Round(sw.Elapsed.TotalSeconds, 1)}s");
                List<ProductDto> apiProducts = apiFetchResult.Products.Values.ToList();
                PersistProductsResult persistResult = productsPersister.PersistMergeProducts(apiProducts, dbProducts);
                if (apiFetchResult.RemoteFetchPerformed)
                {
                    dataAccess.SetLastProductsListUpdateTimestamp(DateTime.Now);
                }
                
                Console.WriteLine();
                Console.WriteLine($"All Products In Stock => {persistResult.ProductsInStock.Count}");
                ProductsFormatter.WriteProductsTableToConsole(
                    persistResult.ProductsInStock
                        .Where(p => p.LevelTwoCategory == "Dried flowers")
                        .OrderBy(p => p.ProducerName).ThenBy(p => p.Brand));
                
                Console.WriteLine($"Products newly in stock => {persistResult.NewVariantsInStock.Count}");
                ProductsFormatter.WriteProductsTableToConsole(persistResult.NewVariantsInStock.Keys);
                
                Console.WriteLine($"Total: {apiProducts.Count} products");
                
                Console.WriteLine("Waiting 15 minutes until next execution");
                await Task.Delay(TimeSpan.FromMinutes(15));
            }
        }
    }
}