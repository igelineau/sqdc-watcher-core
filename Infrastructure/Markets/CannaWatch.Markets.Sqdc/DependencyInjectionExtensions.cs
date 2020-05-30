using CannaWatch.Markets.Sqdc.Implementation;
using SqdcWatcher.Infrastructure.Abstractions;
using XFactory.SqdcWatcher.Core.Caching;
using XFactory.SqdcWatcher.Core.DataMapping;
using XFactory.SqdcWatcher.Core.Interfaces;
using XFactory.SqdcWatcher.Core.Services;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddCannaWatchSqdcMarket(this IServiceCollection services)
        {
            // Public API
            services.AddTransient<SqdcMarketFacade>();
            services.AddTransient<IMarketScanService, SqdcMarketFacade>();
            services.AddFactory<IScanOperation, ScanOperation<SqdcMarketFacade>>();
            services.AddTransient<IRemoteProductsStore<SqdcProductsFetcher>, SqdcProductsFetcher>();

            services.AddTransient<SqdcRestApiClient>();
            services.AddTransient<SqdcHtmlParser>();
            services.AddTransient<SqdcProductsFetcher>();

            services.AddGenericOpenTypeTransient(typeof(IMapper<,>));

            return services;
        }

        public static IServiceCollection AddSqdcProductsFileCache(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IRemoteProductsStore<SqdcProductsFetcher>, ProductsFileCacheProxy<SqdcProductsFetcher>>();
            return serviceCollection;
        }
    }
}