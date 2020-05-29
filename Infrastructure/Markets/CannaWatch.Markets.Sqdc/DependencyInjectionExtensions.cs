using CannaWatch.Markets.Sqdc.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using SqdcWatcher.Infrastructure.Abstractions;
using XFactory.SqdcWatcher.Core.Caching;
using XFactory.SqdcWatcher.Core.Configuration;
using XFactory.SqdcWatcher.Core.DataMapping;
using XFactory.SqdcWatcher.Core.Interfaces;
using XFactory.SqdcWatcher.Core.Services;

namespace CannaWatch.Markets.Sqdc
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddCannaWatchSqdcMarket(this IServiceCollection services)
        {
            // Public API
            services.AddTransient<SqdcMarketFacade>();
            services.AddTransient<IMarketFacade, SqdcMarketFacade>();
            services.AddTransient<IMarketScanService, SqdcMarketFacade>();
            services.AddFactory<IScanOperation, ScanOperation<SqdcMarketFacade>>();
            
            services.AddTransient<SqdcRestApiClient>();
            services.AddTransient<SqdcHtmlParser>();
            services.AddTransient<SqdcProductsFetcher>();
            
            services.AddGenericOpenTypeTransient(typeof(IMapper<,>));
            
            services.AddTransient<IRemoteStore<SqdcMarketFacade, ProductDto>>(ctx =>
            {
                var sqdcConfiguration = ctx.GetRequiredService<IOptions<SqdcConfiguration>>();
                return sqdcConfiguration.Value.UseProductsHtmlCaching
                    ? (IRemoteStore<SqdcMarketFacade, ProductDto>) ctx.GetRequiredService<ProductsFileCacheProxy<SqdcMarketFacade, SqdcProductsFetcher>>()
                    : ctx.GetRequiredService<SqdcProductsFetcher>();
            });

            return services;
        }
    }
}