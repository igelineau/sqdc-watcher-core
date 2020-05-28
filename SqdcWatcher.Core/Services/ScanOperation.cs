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
using XFactory.SqdcWatcher.Core.Interfaces;
using XFactory.SqdcWatcher.Core.Mappers;
using XFactory.SqdcWatcher.Core.Utils;
using XFactory.SqdcWatcher.Data.Entities;
using XFactory.SqdcWatcher.Data.Entities.Common;
using XFactory.SqdcWatcher.Data.Entities.History;
using XFactory.SqdcWatcher.Data.Entities.Products;
using XFactory.SqdcWatcher.Data.Entities.ProductVariant;
using XFactory.SqdcWatcher.DataAccess;

namespace XFactory.SqdcWatcher.Core.Services
{
    [UsedImplicitly]
    public class ScanOperation : IScanOperation
    {
        private readonly TimeSpan refreshProductsListInterval = TimeSpan.FromMinutes(30);
        private readonly Dictionary<string, Product> localProducts = new Dictionary<string, Product>();
        private readonly List<Product> newProducts = new List<Product>();

        private readonly ILogger<ScanOperation> logger;
        private readonly Func<SqdcDbContext> dbContextFactory;
        private readonly ProductMapper productMapper;
        private readonly IEnumerable<VisitorBase<Product>> productVisitors;
        private readonly IMarketDataFetcher marketDataFetcher;
        private readonly ISlackClient slackPostClient;
        private readonly SpecificationsMapper specificationsMapper;
        private readonly IRemoteStore<ProductDto> sqdcProductsFetcher;
        private readonly VariantPricesMapper variantPricesMapper;
        private AppState appState;
        private SqdcDbContext dbContext;

        private bool mustUpdateProductsList;
        
        public ScanOperation(
            IRemoteStore<ProductDto> sqdcProductsFetcher,
            IMarketDataFetcher marketDataFetcher,
            VariantPricesMapper variantPricesMapper,
            Func<SqdcDbContext> dbContextFactory,
            ILogger<ScanOperation> logger,
            ISlackClient slackPostClient,
            ProductMapper productMapper,
            SpecificationsMapper specificationsMapper,
            IEnumerable<VisitorBase<Product>> productVisitors)
        {
            this.sqdcProductsFetcher = sqdcProductsFetcher;
            this.marketDataFetcher = marketDataFetcher;
            this.productMapper = productMapper;
            this.variantPricesMapper = variantPricesMapper;
            this.dbContextFactory = dbContextFactory;
            this.logger = logger;
            this.slackPostClient = slackPostClient;
            this.specificationsMapper = specificationsMapper;
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

        private void LogProductsStatistics()
        {
            logger.LogInformation($"{localProducts.Count} total products ({newProducts.Count} new)");
        }

        private async Task PrepareExecution(bool forceProductsRefresh)
        {
            appState = await dbContext.AppState.AsTracking().FirstOrDefaultAsync() ?? new AppState();
            mustUpdateProductsList =
                forceProductsRefresh || DateTime.Now - (appState.LastProductsListRefresh ?? DateTime.MinValue) > refreshProductsListInterval;
        }

        private async Task SendSlackNotification()
        {
            if (newProducts.Any())
            {
                await slackPostClient.PostToSlackAsync(ProductsFormatter.FormatForSlackTable(newProducts));
                ProductsFormatter.WriteProductsTableToConsole(newProducts);
            }
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
                    numberOfEntriesByType[entryType] = (counts.nbAdded + (isAdded ? 1 : 0), counts.nbModified + (isModified ? 1 : 0));
                }
            }

            if (numberOfEntriesByType.Values.Any(v => v.nbAdded + v.nbModified > 0))
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

        private async Task LoadLocalProductsList(CancellationToken cancellationToken)
        {
            IAsyncEnumerable<Product> query = dbContext.Products
                .AsTracking()
                .Include(p => p.Variants).ThenInclude(v => v.Specifications)
                .AsAsyncEnumerable();
            await foreach (Product product in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
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
                await foreach (ProductDto productDto in sqdcProductsFetcher.GetAllItemsAsync(cancellationToken))
                {
                    await AddProductIfNew(cancellationToken, productDto);
                }

                logger.LogInformation($"Refreshed products in {sw.Elapsed.ToSmartFormat()}");
            }
        }

        private async Task AddProductIfNew(CancellationToken cancellationToken, ProductDto productDto)
        {
            bool isNewProduct = !localProducts.TryGetValue(productDto.Id, out Product dbProduct);
            dbProduct = productMapper.Map(productDto, dbProduct);
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
                VariantsPricesResponse pricesResponse = await marketDataFetcher.GetVariantsPrices(localProducts.Keys, cancelToken);
                foreach (ProductPrice productPrice in pricesResponse.ProductPrices)
                {
                    Product product = localProducts[productPrice.ProductId];
                    AddVariantsToProduct(product, productPrice.VariantPrices);
                }
            }
        }

        private void AddVariantsToProduct(Product product, List<ProductVariantPrice> variantPrices)
        {
            foreach (ProductVariantPrice variantPrice in variantPrices)
            {
                long variantId = long.Parse(variantPrice.VariantId);
                ProductVariant dbVariant = product.GetVariantById(variantId);
                bool isNew = dbVariant == null;

                dbVariant = variantPricesMapper.Map(variantPrice, dbVariant);
                dbVariant.ProductId ??= product.Id;

                if (isNew)
                {
                    dbContext.ProductVariants.Add(dbVariant);
                    product.AddVariant(dbVariant);
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
                logger.LogInformation($"Fetching specifications for productId={variant.ProductId}, variantId={variant.Id}");
                try
                {
                    SpecificationsResponse response =
                        await marketDataFetcher.GetSpecifications(variant.ProductId, variant.Id.ToString(), cancellationToken);
                    specificationsMapper.Map(response, variant.Specifications);
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
            HashSet<long> variantsIdsInStock = (await marketDataFetcher.GetInventoryItems(allVariants.Select(v => v.Id), cancelToken)).ToHashSet();
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