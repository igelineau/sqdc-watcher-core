using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using XFactory.SqdcWatcher.Core;
using XFactory.SqdcWatcher.Core.Mappers;
using XFactory.SqdcWatcher.Core.MappingFilters;
using XFactory.SqdcWatcher.Core.RestApiModels;
using XFactory.SqdcWatcher.Core.Services;
using XFactory.SqdcWatcher.Core.Visitors;
using XFactory.SqdcWatcher.Data.Entities;
using XFactory.SqdcWatcher.DataAccess;

namespace XFactory.SqdcWatcher.ConsoleApp
{
    public static class Program
    {
        private const string CONFIG_DIR_NAME = "sqdc-watcher";
        private static CancellationTokenSource cancelTokenSource;

        public static ServiceProvider ServiceProvider { get; private set; }

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
                        else if (key.Key == ConsoleKey.F17 || key.Key == ConsoleKey.F6)
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

            services.AddSqdcDbContext();

            ConfigureLogging(services);
            ConfigureMapper(services);

            RegisterAllImplementationsOfType(services, typeof(VisitorBase<>));
            RegisterAllImplementationsOfType(services, typeof(IMapper<,>));
            RegisterAllImplementationsOfType(services, typeof(IMappingFilter<,>));

            services.AddFactory<IScanOperation, ScanOperation>();

            services.AddTransient<VariantStockStatusUpdater>();
            services.AddSingleton<SqdcRestApiClient>();
            services.AddSingleton<SqdcWebClient>();
            services.AddSingleton<ISqdcWatcher, SqdcHttpWatcher>();
            services.AddSingleton<SlackPostWebHookClient>();
            services.AddSingleton<BecameInStockTriggerPolicy>();

            services.AddSingleton<ISqdcDataAccess, SqdcDataAccess>();

            ServiceProvider = services.BuildServiceProvider();
        }

        private static void ConfigureLogging(ServiceCollection services)
        {
            string logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), CONFIG_DIR_NAME, "logs");
            services.AddLogging(configure => configure.AddSerilog(new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                //.MinimumLevel.Override(DbLoggerCategory.Query.Name, LogEventLevel.Information)
                .MinimumLevel.Override(DbLoggerCategory.Name, LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.File(Path.Combine(logsDirectory, "all.log"), LogEventLevel.Information)
                .WriteTo.File(Path.Combine(logsDirectory, "errors.log"), LogEventLevel.Error)
                .CreateLogger()));

            //Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
        }

        private static void RegisterAllImplementationsOfType(ServiceCollection collection, Type baseType)
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => !t.IsAbstract && FindBaseType(t, baseType) != null))
            {
                collection.AddTransient(type);
                collection.AddTransient(FindBaseType(type, baseType), type);
            }
        }

        private static Type FindBaseType(Type derivedType, Type baseTypeToFind)
        {
            Type implementedInterface = FindImplementedInterface(derivedType, baseTypeToFind);
            if (implementedInterface != null)
            {
                return implementedInterface;
            }

            // concrete type
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

        private static Type FindImplementedInterface(Type type, Type interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                return null;
            }

            return type
                .GetInterfaces()
                .FirstOrDefault(i => i == interfaceType || (i.IsConstructedGenericType && i.GetGenericTypeDefinition() == interfaceType));
        }
    }
}