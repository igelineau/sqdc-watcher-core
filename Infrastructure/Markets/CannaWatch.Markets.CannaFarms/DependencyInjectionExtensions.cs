using CannaWatch.Markets.CannaFarms.Configuration;
using CannaWatch.Markets.CannaFarms.HttpClient;
using CannaWatch.Markets.CannaFarms.Implementation;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using SqdcWatcher.Infrastructure.Abstractions;
using XFactory.SqdcWatcher.Core.Caching;
using XFactory.SqdcWatcher.Core.Interfaces;
using XFactory.SqdcWatcher.Core.Services;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    [PublicAPI]
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddCannaWatchCannaFarmsMarket(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<CannaFarmsOptions>(config.GetSection("Markets:CannaFarms"));
            
            services.AddTransient<CannaFarmsScanService>();
            services.AddTransient<IMarketScanService, CannaFarmsScanService>();
            services.AddFactory<IScanOperation, ScanOperation<CannaFarmsScanService>>();
            services.AddTransient<IRemoteProductsStore<CannaFarmsScanService>, CannaFarmsScanService>();
            
            services.AddTransient<CannaFarmsRestClient>();
            services.AddTransient<CannaFarmsAuthenticator>();

            return services;
        }
        
        public static IServiceCollection AddCannaFarmsProductsFileCache(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IRemoteProductsStore<CannaFarmsScanService>, ProductsFileCacheProxy<CannaFarmsScanService>>();
            return serviceCollection;
        }
        
    }
}