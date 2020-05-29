using CannaWatch.Markets.CannaFarms.Configuration;
using CannaWatch.Markets.CannaFarms.HttpClient;
using CannaWatch.Markets.CannaFarms.Implementation;
using Microsoft.Extensions.Configuration;
using SqdcWatcher.Infrastructure.Abstractions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddCannaWatchCannaFarmsMarket(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<CannaFarmsOptions>(config.GetSection("Markets:CannaFarms"));
            
            // Public API
            services.AddTransient<IMarketScanService, CannaFarmsScanService>();
            services.AddTransient<IMarketFacade, CannaFarmsScanService>();
            services.AddTransient<CannaFarmsRestClient>();
            services.AddTransient<CannaFarmsAuthenticator>();
            
            // services.AddTransient<IMarketFacade, SqdcMarketFacade>();
            // services.AddTransient<IMarketScanService, SqdcMarketFacade>();
            // services.AddFactory<IScanOperation, ScanOperation<SqdcMarketFacade>>();
            //
            // services.AddTransient<SqdcRestApiClient>();
            // services.AddTransient<SqdcHtmlParser>();
            // services.AddTransient<SqdcProductsFetcher>();
            //
            // services.AddGenericOpenTypeTransient(typeof(IMapper<,>));
            //
            // services.AddTransient<IRemoteStore<SqdcMarketFacade, ProductDto>>(ctx =>
            // {
            //     var sqdcConfiguration = ctx.GetRequiredService<IOptions<SqdcConfiguration>>();
            //     return sqdcConfiguration.Value.UseProductsHtmlCaching
            //         ? (IRemoteStore<SqdcMarketFacade, ProductDto>) ctx.GetRequiredService<ProductsFileCacheProxy<SqdcMarketFacade, SqdcProductsFetcher>>()
            //         : ctx.GetRequiredService<SqdcProductsFetcher>();
            // });

            return services;
        }
    }
}