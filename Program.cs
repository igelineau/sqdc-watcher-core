﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using SqdcWatcher.DataAccess;
using SqdcWatcher.DataObjects;
using SqdcWatcher.MappingFilters;
using SqdcWatcher.RestApiModels;
using SqdcWatcher.Services;
using SqdcWatcher.Visitors;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace SqdcWatcher
{
    public static class Program
    {
        private const string CONFIG_DIR_NAME = "sqdc-watcher";
        
        public static ServiceProvider ServiceProvider { get; private set; }
        private static CancellationTokenSource cancelTokenSource;

        private static void Main(string[] args)
        {
            RegisterServices();

            Console.WriteLine(
                $"Slack Post URL: {ServiceProvider.GetService<IOptions<SqdcAppConfiguration>>().Value.SlackPostUrl}");
            
            Console.TreatControlCAsInput = true;
            
            Task.Run(ApplicationLoop).Wait();
            
        }

        private static void ApplicationLoop()
        {
            var watcher = ServiceProvider.GetService<ISqdcWatcher>();
            cancelTokenSource = new CancellationTokenSource();
            
            watcher.Start(cancelTokenSource.Token);
            while (watcher.State != WatcherState.Stopped)
            {
                Thread.Sleep(100);

                try
                {
                    while (!cancelTokenSource.IsCancellationRequested)
                    {
                        ConsoleKeyInfo key = Console.ReadKey();
                        Debug.WriteLine($"KEY PRESSED: {key.Key}");
                        if (key.Key == ConsoleKey.F5)
                        {
                            watcher.RequestRefresh();
                        }
                        else if(key.Key == ConsoleKey.F17)
                        {
                            watcher.RequestRefresh(true);
                        }
                        else if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.C)
                        {
                            Console.Write("\b");
                            Console.WriteLine();
                            Console.WriteLine("Control+C pressed, exiting cleanly.");
                            cancelTokenSource.Cancel();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            Console.WriteLine("Watcher state is stopped, exiting the application.");
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
            var services = new ServiceCollection();

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower()}.json", true, true)
                .Build();

            services.AddSingleton(config);
            services.AddOptions();
            services.Configure<SqdcAppConfiguration>(config.GetSection("SqdcSettings"));
            
            ConfigureLogging(services);
            ConfigureDatabase(services);
            ConfigureMapper(services);
            RegisterAllImplementationsOfType(services, typeof(VisitorBase<>));
            RegisterAllImplementationsOfType(services, typeof(MappingFilterBase<,>));
            
            services.AddSingleton<SqdcRestApiClient>();
            services.AddSingleton<SqdcWebClient>();
            services.AddSingleton<SqdcProductsFetcher>();
            services.AddSingleton<ProductsPersister>();
            services.AddSingleton<ISqdcWatcher, SqdcHttpWatcher>();
            services.AddSingleton<SqdcDataAccess>();
            services.AddSingleton<SlackPostWebHookClient>();
            services.AddSingleton<BecameInStockTriggerPolicy>();
            services.AddSingleton<StockHistoryPersister>();
            
            services.AddSingleton<DapperDataAccess>();

            ServiceProvider = services.BuildServiceProvider();
        }
        
        private static void ConfigureLogging(ServiceCollection services)
        {
            string logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), CONFIG_DIR_NAME, "logs");
            services.AddLogging(configure => configure.AddSerilog(new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.File(Path.Combine(logsDirectory, "all.log"), LogEventLevel.Information)
                .WriteTo.File(Path.Combine(logsDirectory, "errors.log"), LogEventLevel.Error)
                .CreateLogger()));

            //Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
        }

        private static void RegisterAllImplementationsOfType(ServiceCollection collection, Type baseType)
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(t => FindBaseType(t, baseType) != null))
            {
                collection.AddSingleton(type);
                collection.AddSingleton(FindBaseType(type, baseType), type);
            }
        }

        private static Type FindBaseType(Type derivedType, Type baseTypeToFind)
        {
            Type currentBaseType = derivedType.BaseType;
            while (currentBaseType != null)
            {
                if (currentBaseType == baseTypeToFind ||
                    (currentBaseType.IsGenericType && currentBaseType.GetGenericTypeDefinition() == baseTypeToFind))
                {
                    return currentBaseType;
                }
                
                currentBaseType = currentBaseType.BaseType;
            }

            return null;
        }

        private static void ConfigureDatabase(ServiceCollection collection)
        {
            string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string envSuffix = "";
            if (envName != null)
            {
                envSuffix = "_" + envName;
            }

            string databasePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                CONFIG_DIR_NAME,
                $"store{envSuffix}.db");
            
            Console.WriteLine($"Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "PRODUCTION"}");
            Console.WriteLine($"Using database : {databasePath}");
            
            if (!Directory.Exists(Path.GetDirectoryName(databasePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(databasePath));
            }

            collection.AddSingleton<IDbConnectionFactory>(
                c => new OrmLiteConnectionFactory(databasePath, SqliteDialect.Provider));
        }
    }
}
