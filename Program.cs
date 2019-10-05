using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using SqdcWatcher.Services;

namespace SqdcWatcher
{
    class Program
    {
        private static ServiceProvider serviceProvider;
        private static CancellationTokenSource cancelTokenSource;

        private static void Main(string[] args)
        {
            RegisterServices();

            var watcher = serviceProvider.GetService<ISqdcWatcher>();
            cancelTokenSource = new CancellationTokenSource(); 
            watcher.start(cancelTokenSource.Token);
        }

        private static void RegisterServices()
        {
            var collection = new ServiceCollection();
         
            collection.AddSingleton<SqdcRestApiClient>();
            collection.AddSingleton<SqdcWebClient>();
            collection.AddSingleton<SqdcProductsFetcher>();
            collection.AddSingleton<ProductsPersister>();
            collection.AddSingleton<ISqdcWatcher, SqdcHttpWatcher>();
            collection.AddSingleton<DataAccess>();
            
            string databasePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "sqdc-watcher",
                "store.db");
            if(!Directory.Exists(Path.GetDirectoryName(databasePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(databasePath));
            }
            collection.AddSingleton<IDbConnectionFactory>(
                c => new OrmLiteConnectionFactory(databasePath, SqliteDialect.Provider));

            serviceProvider =  collection.BuildServiceProvider();
        }
    }
}
