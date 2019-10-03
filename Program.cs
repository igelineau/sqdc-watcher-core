using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SqdcWatcher
{
    class Program
    {
        private static ServiceProvider serviceProvider;
        private static CancellationTokenSource cancelTokenSource;

        static void Main(string[] args)
        {
            RegisterServices();

            var watcher = serviceProvider.GetService<ISqdcWatcher>();
            cancelTokenSource = new CancellationTokenSource(); 
            watcher.start(cancelTokenSource.Token);
        }

        private static void RegisterServices()
        {
            var collection = new ServiceCollection();

            collection.AddSingleton<ISqdcWatcher, SqdcHttpWatcher>();
            collection.AddSingleton<ISqdcClient, SqdcWebClient>();

            serviceProvider =  collection.BuildServiceProvider();
        }
    }
}
