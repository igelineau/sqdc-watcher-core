using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ServiceStack.Script;
using SqdcWatcher.DataAccess;
using SqdcWatcher.DataObjects;
using SqdcWatcher.Dto;
using SqdcWatcher.RestApiModels;

namespace SqdcWatcher.Services
{
    public enum WatcherState
    {
        Idle = 0,
        Running,
        Stopped
    }
    
    public class SqdcHttpWatcher : ISqdcWatcher
    {
        private readonly TimeSpan loopInterval = TimeSpan.FromMinutes(6);

        private readonly ILogger<SqdcHttpWatcher> logger;
        private readonly SqdcProductsFetcher productsFetcher;
        private readonly ProductsPersister productsPersister;
        private readonly SqdcDataAccess sqdcDataAccess;
        private readonly SlackPostWebHookClient slackPostClient;
        public WatcherState State { get; private set; }
        
        private bool isRefreshRequested;
        private bool isRefreshInProgress;
        private bool isFullRefreshRequested;

        public SqdcHttpWatcher(
            ILogger<SqdcHttpWatcher> logger,
            SqdcProductsFetcher productsFetcher,
            ProductsPersister productsPersister,
            SqdcDataAccess sqdcDataAccess,
            SlackPostWebHookClient slackPostClient)
        {
            this.logger = logger;
            this.productsFetcher = productsFetcher;
            this.productsPersister = productsPersister;
            this.sqdcDataAccess = sqdcDataAccess;
            this.slackPostClient = slackPostClient;
        }
        
        public void Start(CancellationToken cancellationToken)
        {
            if(State == WatcherState.Running)
            {
                throw new WatcherStartException("cannot start watcher, it is already running.");
            }

            State = WatcherState.Running;
            Task.Run(async () =>
            {
                try
                {
                    await Loop(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // do nothing
                }
                catch (Exception e)
                {
                    logger.LogError(e, e.Message);
                }
                finally
                {
                    State = WatcherState.Stopped;
                }
            }, CancellationToken.None);
        }

        public void RequestRefresh(bool fullRefresh = false)
        {
            if (isRefreshInProgress)
            {
                logger.LogInformation("Ignoring a manual refresh request made while a refresh is already in progress.");
            }
            else
            {
                isRefreshRequested = true;
                isFullRefreshRequested = fullRefresh;
            }
        }
        
        private async Task Loop(CancellationToken cancelToken)
        {
            isRefreshInProgress = true;
            
            while(!cancelToken.IsCancellationRequested)
            {
                logger.LogInformation("--- Watcher - Refresh products started ---");
                
                Stopwatch sw = Stopwatch.StartNew();
                
                try
                {
                    await ExecuteScan(cancelToken);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error while executing scan within the watcher loop");
                }
                finally
                {
                    isFullRefreshRequested = false;
                }
                
                DateTime nextExecutionTime = DateTime.Now + loopInterval;
                logger.LogInformation(
                    $"Scan completed in {Math.Round(sw.Elapsed.TotalSeconds, 1)}s. Next execution: {nextExecutionTime:H\\hmm}");
                await SleepChunksAsync(loopInterval, cancelToken);
            }
        }

        private async Task ExecuteScan(CancellationToken cancelToken)
        {
            Dictionary<string, Product> dbProducts = sqdcDataAccess.GetProducts();

            var getProductsInfoDto = new GetProductsInfoDto
            {
                VariantsWithUpToDateSpecs = new HashSet<long>(dbProducts.Values
                    .SelectMany(v => v.Variants.Where(var => var.HasSpecifications()))
                    .Select(v => v.Id))
            };
            ProductsListResult apiFetchResult = await productsFetcher.FetchProductsFromApi(
                getProductsInfoDto,
                cancelToken,
                isFullRefreshRequested);
            cancelToken.ThrowIfCancellationRequested();

            List<ProductDto> apiProducts = apiFetchResult.Products.Values.ToList();
            PersistProductsResult persistResult = productsPersister.PersistMergeProducts(apiProducts, dbProducts);
            if (apiFetchResult.RemoteFetchPerformed)
            {
                sqdcDataAccess.SetLastProductsListUpdateTimestamp(DateTime.Now);
            }

            SendSlackNotifications(persistResult);

            int nbDriedFlowersInStock = persistResult.ProductsInStock.Count(p => p.LevelTwoCategory == "Dried flowers");
            logger.LogInformation(
                $"Found {apiProducts.Count} products, {persistResult.ProductsInStock.Count} in stock ({nbDriedFlowersInStock} in Dried flowers)");
            if (persistResult.NewVariantsInStock.Any())
            {
                logger.LogInformation($"BACK IN STOCK: {persistResult.NewVariantsInStock.Count} products");
            }
        }

        private void SendSlackNotifications(PersistProductsResult persistResult)
        {
            if (persistResult.NewVariantsInStock.Any())
            {
                slackPostClient.PostToSlack(ProductsFormatter.FormatForSlackTable(persistResult.NewVariantsInStock.Keys, persistResult));
                ProductsFormatter.WriteProductsTableToConsole(persistResult.NewVariantsInStock.Keys);
            }

            if (persistResult.NewProducts.Any())
            {
                slackPostClient.PostToSlack(ProductsFormatter.FormatForSlackTable(persistResult.NewProducts, persistResult));
                ProductsFormatter.WriteProductsTableToConsole(persistResult.NewProducts);
            }
        }

        private async Task SleepChunksAsync(TimeSpan totalTime, CancellationToken cancelToken)
        {
            isRefreshInProgress = false;
            isRefreshRequested = false;
            
            TimeSpan totalWaited = TimeSpan.Zero;
            TimeSpan chunkDuration = TimeSpan.FromMilliseconds(500);
            
            while (totalWaited < totalTime && !isRefreshRequested && !cancelToken.IsCancellationRequested)
            {
                await Task.Delay(chunkDuration, cancelToken).ConfigureAwait(false);
                totalWaited += chunkDuration;
            }

            if (isRefreshRequested)
            {
                logger.LogInformation("Manual refresh was requested, proceeding...");
                isRefreshRequested = false;
            }
        }
    }
}