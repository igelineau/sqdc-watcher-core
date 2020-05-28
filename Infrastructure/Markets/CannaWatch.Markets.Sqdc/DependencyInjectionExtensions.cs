using CannaWatch.Markets.Sqdc.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using SqdcWatcher.Infrastructure.Abstractions;
using XFactory.SqdcWatcher.Core.Caching;
using XFactory.SqdcWatcher.Core.Configuration;

namespace CannaWatch.Markets.Sqdc
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddCannaWatchSqdcMarket(this IServiceCollection services)
        {
            services.AddTransient<SqdcRestApiClient>();
            
            services.AddTransient<IProductHtmlParser, SqdcHtmlParser>();
            services.AddTransient<IMarketDataFetcher, SqdcRestApiClient>();

            services.AddTransient<SqdcProductsFetcher>();
            services.AddTransient<IRemoteStore<ProductDto>>(ctx =>
            {
                var sqdcConfiguration = ctx.GetRequiredService<IOptions<SqdcConfiguration>>();
                return sqdcConfiguration.Value.UseProductsHtmlCaching
                    ? (IRemoteStore<ProductDto>) ctx.GetRequiredService<ProductsFileCacheProxy<SqdcProductsFetcher>>()
                    : ctx.GetRequiredService<SqdcProductsFetcher>();
            });

            return services;
        }
    }
}