using System;
using JetBrains.Annotations;
using XFactory.SqdcWatcher.DataAccess;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        [PublicAPI]
        public static IServiceCollection AddSqdcWatcherDbContext(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ISqdcDataAccess, SqdcDataAccess>();
            
            serviceCollection.AddDbContext<SqdcDbContext>();
            serviceCollection.AddFactory<SqdcDbContext, SqdcDbContext>();
            return serviceCollection;
        }
        
        public static void AddFactory<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddTransient<TService, TImplementation>();
            services.AddTransient<TImplementation>();
            services.AddScoped<Func<TService>>(x => x.GetService<TImplementation>);
        }
    }
}