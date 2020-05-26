using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XFactory.SqdcWatcher.Data.Entities.Common;
using XFactory.SqdcWatcher.Data.Entities.History;
using XFactory.SqdcWatcher.Data.Entities.Products;
using XFactory.SqdcWatcher.Data.Entities.ProductVariant;

namespace XFactory.SqdcWatcher.DataAccess
{
    public class SqdcDataAccess : ISqdcDataAccess
    {
        private readonly Func<SqdcDbContext> dbContextFactory;

        public SqdcDataAccess(Func<SqdcDbContext> dbContextFactory, ILogger<SqdcDataAccess> logger)
        {
            this.dbContextFactory = dbContextFactory;

            using SqdcDbContext context = dbContextFactory();
            logger.LogInformation($"Using ConnectionString: {context.GetConnectionString()}");
        }

        public async Task<IEnumerable<Product>> GetProductsWithRelationsAsync(bool asTracking = false)
        {
            await using SqdcDbContext dbContext = dbContextFactory();
            return await dbContext.Products
                .AsTracking(asTracking ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking)
                .Include(p => p.Variants).ThenInclude(v => v.Specifications)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsSummary()
        {
            await using SqdcDbContext dbContext = dbContextFactory();
            return await dbContext.Products.Include(p => p.Variants)
                .ToListAsync();
        }

        public async Task SaveProducts(List<Product> products)
        {
            await using SqdcDbContext dbContext = dbContextFactory();
            HashSet<string> existingProductIds = dbContext.Products.Select(p => p.Id).ToHashSet();
            HashSet<long> existingVariantIds = dbContext.ProductVariants.Select(pv => pv.Id).ToHashSet();

            List<ProductVariant> variants = products.SelectMany(p => p.Variants).ToList();
            await dbContext.ProductVariants.AddRangeAsync(variants.Where(v => !existingVariantIds.Contains(v.Id)));
            dbContext.ProductVariants.UpdateRange(variants.Where(v => existingVariantIds.Contains(v.Id)));

            await dbContext.Products.AddRangeAsync(products.Where(p => !existingProductIds.Contains(p.Id)));
            dbContext.Products.UpdateRange(products.Where(p => existingProductIds.Contains(p.Id)));

            // do not update existing Specifications
            IEnumerable<SpecificationAttribute> attributesToDetach = dbContext.SpecificationAttribute.Local
                .Where(sa => dbContext.Entry(sa).State == EntityState.Modified)
                .ToList();
            foreach (SpecificationAttribute spec in attributesToDetach)
            {
                dbContext.Entry(spec).State = EntityState.Detached;
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task<AppState> GetAppStateAsync()
        {
            await using SqdcDbContext context = dbContextFactory();
            return await context.AppState.FirstOrDefaultAsync();
        }

        public async Task UpdateAppStateAsync(AppState state)
        {
            await using SqdcDbContext context = dbContextFactory();
            context.AppState.Update(state);
            await context.SaveChangesAsync();
        }

        public async Task InsertPriceHistoryEntry(PriceHistory priceHistoryEntry)
        {
            await using SqdcDbContext context = dbContextFactory();
            await context.PriceHistory.AddAsync(priceHistoryEntry);
            await context.SaveChangesAsync();
        }
    }
}