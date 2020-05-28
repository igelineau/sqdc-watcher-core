using CannaWatch.Markets.Sqdc.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using SqdcWatcher.Infrastructure.Abstractions;
using XFactory.SqdcWatcher.Core.Configuration;
using XFactory.SqdcWatcher.Core.SiteCrawling;

namespace CannaWatch.Markets.Sqdc
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddCannaWatchSqdcMarket(this IServiceCollection services)
        {
            services.AddTransient<SqdcRestApiClient>();
            services.AddTransient<SqdcProductsFetcher>();
            services.AddTransient<IProductHtmlParser, SqdcHtmlParser>();
            services.AddTransient<IMarketDataFetcher, SqdcRestApiClient>();

            services.AddTransient<IRemoteStore<ProductDto>>(ctx =>
            {
                IOptions<SqdcConfiguration> sqdcConfiguration = ctx.GetRequiredService<IOptions<SqdcConfiguration>>();
                return sqdcConfiguration.Value.UseProductsHtmlCaching
                    ? (IRemoteStore<ProductDto>) ctx.GetRequiredService<ProductsFileCacheProxy>()
                    : ctx.GetRequiredService<SqdcProductsFetcher>();
            });

            return services;
        }
    }
}