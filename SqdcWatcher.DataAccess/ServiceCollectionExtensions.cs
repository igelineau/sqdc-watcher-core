using System;
using XFactory.SqdcWatcher.DataAccess;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqdcDbContext(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<SqdcDbContext>();
            serviceCollection.AddFactory<SqdcDbContext, SqdcDbContext>();
            return serviceCollection;
        }

        public static void AddFactory<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddTransient<TService, TImplementation>();
            services.AddScoped<Func<TService>>(x => x.GetService<TService>);
        }
    }
}