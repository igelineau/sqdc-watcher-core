#region

using System;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace SqdcWatcher.DataAccess.EntityFramework
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqdcDbContext(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<SqdcDbContext>();
            serviceCollection.AddFactory<SqdcDbContext, SqdcDbContext>();
            return serviceCollection;
        }

        public static void AddFactory<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddTransient<TService, TImplementation>();
            services.AddSingleton<Func<TService>>(x => x.GetService<TService>);
        }
    }
}