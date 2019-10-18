using System;
using System.IO;
using System.Threading;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using SqdcWatcher.DataObjects;
using SqdcWatcher.RestApiModels.cs;
using SqdcWatcher.Services;

namespace SqdcWatcher
{
    public static class Program
    {
        public static ServiceProvider ServiceProvider { get; private set; }
        private static CancellationTokenSource cancelTokenSource;
        
        
        private static void Main(string[] args)
        {
            RegisterServices();

            var watcher = ServiceProvider.GetService<ISqdcWatcher>();
            cancelTokenSource = new CancellationTokenSource(); 
            watcher.start(cancelTokenSource.Token);
        }

        private static void ConfigureMapper(IServiceCollection collection)
        {
            collection.AddSingleton(new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductDto, Product>();
                cfg.CreateMap<ProductVariantDto, ProductVariant>();
                cfg.CreateMap<SpecificationAttributeDto, SpecificationAttribute>();
            }));
        }

        private static void RegisterServices()
        {
            var collection = new ServiceCollection();
            
            ConfigureDatabase(collection);
            ConfigureMapper(collection);
            
            collection.AddSingleton<SqdcRestApiClient>();
            collection.AddSingleton<SqdcWebClient>();
            collection.AddSingleton<SqdcProductsFetcher>();
            collection.AddSingleton<ProductsPersister>();
            collection.AddSingleton<ISqdcWatcher, SqdcHttpWatcher>();
            collection.AddSingleton<DataAccess>();
            collection.AddSingleton<BecameInStockTriggerPolicy>();
            
            ServiceProvider =  collection.BuildServiceProvider();
        }

        private static void ConfigureDatabase(ServiceCollection collection)
        {
            string databasePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "sqdc-watcher",
                "store.db");
            if (!Directory.Exists(Path.GetDirectoryName(databasePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(databasePath));
            }

            collection.AddSingleton<IDbConnectionFactory>(
                c => new OrmLiteConnectionFactory(databasePath, SqliteDialect.Provider));
        }
    }
}
