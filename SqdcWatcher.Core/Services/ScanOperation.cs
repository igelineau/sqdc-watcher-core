using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using SqdcWatcher.DataTransferObjects.DomainDto;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using SqdcWatcher.Infrastructure.Abstractions;
using XFactory.SqdcWatcher.Core.Abstractions;
using XFactory.SqdcWatcher.Core.DataMapping;
using XFactory.SqdcWatcher.Core.Interfaces;
using XFactory.SqdcWatcher.Core.Utils;
using XFactory.SqdcWatcher.Data.Entities.Common;
using XFactory.SqdcWatcher.Data.Entities.History;
using XFactory.SqdcWatcher.Data.Entities.Markets;
using XFactory.SqdcWatcher.Data.Entities.Products;
using XFactory.SqdcWatcher.Data.Entities.ProductVariant;
using XFactory.SqdcWatcher.DataAccess;

namespace XFactory.SqdcWatcher.Core.Services
{
    [UsedImplicitly]
    public class ScanOperation<TMarketService> : IScanOperation
        where TMarketService : IMarketScanService
    {
        private readonly TimeSpan refreshProductsListInterval = TimeSpan.FromMinutes(30);
        private readonly Dictionary<string, Product> localProducts = new Dictionary<string, Product>();
        private readonly List<Product> newProducts = new List<Product>();

        private readonly ILogger<ScanOperation<TMarketService>> logger;
        private readonly Func<SqdcDbContext> dbContextFactory;
        private readonly ProductMapper productMapper;
        private readonly IEnumerable<VisitorBase<Product>> productVisitors;
        private readonly ISlackClient slackPostClient;

        private AppState appState;
        private SqdcDbContext dbContext;

        private bool mustUpdateProductsList;
        private readonly IMarketScanService marketScanService;
        private Market market;

        public ScanOperation(
            TMarketService marketScanService,
            Func<SqdcDbContext> dbContextFactory,
            ILogger<ScanOperation<TMarketService>> logger,
            ISlackClient slackPostClient,
            ProductMapper productMapper,
            IEnumerable<VisitorBase<Product>> productVisitors)
        {
            this.marketScanService = marketScanService;
            this.productMapper = productMapper;
            this.dbContextFactory = dbContextFactory;
            this.logger = logger;
            this.slackPostClient = slackPostClient;
            this.productVisitors = productVisitors;
        }

        public async Task Execute(bool forceProductsRefresh, CancellationToken cancellationToken)
        {
            await using (dbContext = dbContextFactory())
            {
                await PrepareExecution(forceProductsRefresh);
                await ExecuteInternal(cancellationToken);
                UpdateAppState();

                ReportSummaryOfPendingChanges();
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            LogProductsStatistics();
            await SendSlackNotification();
        }

        private async Task PrepareExecution(bool forceProductsRefresh)
        {
            market = await dbContext.Markets.FirstOrDefaultAsync(m => m.Id == marketScanService.GetMarketId());
            logger.LogInformation($"Preparing scanner: {market.Name}");
            appState = await dbContext.AppState.AsTracking().FirstOrDefaultAsync() ?? new AppState();

            TimeSpan timeSinceLastProductsRefresh = DateTime.Now - (appState.LastProductsListRefresh ?? DateTime.MinValue);
            mustUpdateProductsList = forceProductsRefresh || timeSinceLastProductsRefresh > refreshProductsListInterval;
        }

        private async Task ExecuteInternal(CancellationToken cancellationToken)
        {
            await Task.WhenAll(
            LoadLocalProductsList(cancellationToken),
            RefreshProducts(cancellationToken));

            await RefreshVariantsAndPrices(cancellationToken);
            await UpdateInStockStatuses(cancellationToken);
            await UpdateSpecifications(cancellationToken);

            VisitorsInvoker.ApplyVisitors(productVisitors, localProducts.Values);
        }

        private void UpdateAppState()
        {
            if (mustUpdateProductsList)
            {
                appState.LastProductsListRefresh = DateTime.Now;
            }

            if (appState.Id == 0)
            {
                dbContext.AppState.Add(appState);
            }
        }

        private void ReportSummaryOfPendingChanges()
        {
            var numberOfEntriesByType = new Dictionary<Type, (int nbAdded, int nbModified)>();
            foreach (EntityEntry entry in dbContext.ChangeTracker.Entries())
            {
                Type entryType = entry.Metadata.ClrType;
                bool isAdded = entry.State == EntityState.Added;
                bool isModified = entry.State == EntityState.Modified;
                if (!numberOfEntriesByType.TryGetValue(entryType, out (int nbAdded, int nbModified) counts))
                {
                    numberOfEntriesByType.Add(entryType, (isAdded ? 1 : 0, isModified ? 1 : 0));
                }
                else
                {
                    int newNbAdded = counts.nbAdded + (isAdded ? 1 : 0);
                    int newNbModified = counts.nbModified + (isModified ? 1 : 0);
                    numberOfEntriesByType[entryType] = (newNbAdded, newNbModified);
                }
            }

            bool hasAnyChanges = numberOfEntriesByType.Values.Any(v => v.nbAdded + v.nbModified > 0); 
            if (hasAnyChanges)
            {
                logger.LogInformation("Summary of changes to apply to database:");
                foreach ((Type key, (int nbAdded, int nbModified)) in numberOfEntriesByType)
                {
                    if (nbAdded + nbModified > 0)
                    {
                        logger.LogInformation($"{key.Name}: {nbAdded} added, {nbModified} modified");
                    }
                }
            }
            else
            {
                logger.LogInformation("No change to persist to database.");
            }
        }

        private async Task SendSlackNotification()
        {
            if (newProducts.Any())
            {
                await slackPostClient.PostToSlackAsync(ProductsFormatter.FormatForSlackTable(newProducts));
                ProductsFormatter.WriteProductsTableToConsole(newProducts);
            }
        }

        private void LogProductsStatistics()
        {
            logger.LogInformation($"{localProducts.Count} total products ({newProducts.Count} new)");
        }

        private async Task LoadLocalProductsList(CancellationToken cancellationToken)
        {
            IAsyncEnumerable<Product> query = dbContext.Products
                .AsTracking()
                .Where(p => p.MarketId == market.Id)
                .Include(p => p.Variants).ThenInclude(v => v.Specifications)
                .AsAsyncEnumerable();
            await foreach (Product product in query.WithCancellation(cancellationToken))
            {
                localProducts.Add(product.Id, product);
            }
        }

        private async Task RefreshProducts(CancellationToken cancellationToken)
        {
            if (mustUpdateProductsList)
            {
                logger.LogInformation("Refreshing local products list...");
                var sw = Stopwatch.StartNew();
                await foreach (ProductDto productDto in marketScanService.GetAllProductsAsync(market, cancellationToken))
                {
                    await AddProductIfNew(cancellationToken, productDto);
                }

                logger.LogInformation($"Refreshed products in {sw.Elapsed.ToSmartFormat()}");
            }
        }

        private async Task AddProductIfNew(CancellationToken cancellationToken, ProductDto productDto)
        {
            bool isNewProduct = !localProducts.TryGetValue(productDto.Id, out Product dbProduct);
            dbProduct = productMapper.MapCreateOrUpdate(productDto, dbProduct);
            if (isNewProduct)
            {
                await AddNewProduct(dbProduct, cancellationToken);
            }
        }

        private async Task AddNewProduct(Product dbProduct, CancellationToken cancellationToken)
        {
            localProducts.Add(dbProduct.Id, dbProduct);
            newProducts.Add(dbProduct);
            await dbContext.Products.AddAsync(dbProduct, cancellationToken);
        }

        private async Task RefreshVariantsAndPrices(CancellationToken cancelToken)
        {
            if (mustUpdateProductsList)
            {
                await marketScanService.PrepareToFetchProductsVariants(localProducts.Keys, cancelToken);
                foreach (Product product in localProducts.Values)
                {
                    await marketScanService.UpdateProductVariants(product, cancelToken);
                }

                foreach(ProductVariant variant in localProducts.Values.SelectMany(p => p.Variants))
                {
                    EntityEntry<ProductVariant> entry = dbContext.Entry(variant);
                    if (entry.State == EntityState.Detached)
                    {
                        await dbContext.ProductVariants.AddAsync(variant, cancelToken);
                    }
                }
            }
        }

        private async Task UpdateSpecifications(CancellationToken cancellationToken)
        {
            IEnumerable<ProductVariant> variantsWithoutSpecs = localProducts.Values
                .SelectMany(p => p.Variants)
                .Where(pv => !pv.Specifications.Any());
            foreach (ProductVariant variant in variantsWithoutSpecs)
            {
                try
                {
                    await marketScanService.UpdateVariantSpecifications(variant, cancellationToken);
                    await dbContext.SpecificationAttribute.AddRangeAsync(variant.Specifications, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error fetching or parsing the specifications of {variant}");
                }
            }
        }

        private async Task UpdateInStockStatuses(CancellationToken cancelToken)
        {
            List<ProductVariant> allVariants = localProducts.Values.SelectMany(p => p.Variants).ToList();
            HashSet<long> variantsIdsInStock = (await marketScanService.GetInventoryItems(allVariants.Select(v => v.Id), cancelToken)).ToHashSet();
            foreach (ProductVariant variant in allVariants)
            {
                StockStatusChangeResult result = variant.SetStockStatus(variantsIdsInStock.Contains(variant.Id));
                if (result != StockStatusChangeResult.NotChanged)
                {
                    string eventName = result == StockStatusChangeResult.BecameInStock
                        ? StockEventNames.InStock
                        : StockEventNames.OutOfStock;
                    var stockHistoryEntry = new StockHistory
                    {
                        ProductVariantId = variant.Id,
                        Timestamp = DateTime.Now,
                        Event = eventName
                    };
                    await dbContext.StockHistory.AddAsync(stockHistoryEntry, cancelToken);
                }
            }
        }
    }
}