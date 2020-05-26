using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using SqdcWatcher.Slack.DependencyInjection;
using XFactory.SqdcWatcher.Core;
using XFactory.SqdcWatcher.Core.Abstractions;
using XFactory.SqdcWatcher.Core.Configuration;
using XFactory.SqdcWatcher.Core.Interfaces;
using XFactory.SqdcWatcher.Core.Mappers;
using XFactory.SqdcWatcher.Core.MappingFilters;
using XFactory.SqdcWatcher.Core.Services;
using XFactory.SqdcWatcher.Core.SiteCrawling;
using XFactory.SqdcWatcher.DataAccess;

namespace XFactory.SqdcWatcher.ConsoleApp
{
    public static class Program
    {
        private const string ConfigDirName = "sqdc-watcher";

        private static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog(ConfigureSerilog)
                .ConfigureServices(ConfigureServices);

        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.AddHostedService<SqdcWorkerService>();
            services.AddTransient<ConsoleInputInterface>();

            IConfiguration config = hostContext.Configuration;
            services.Configure<SqdcConfiguration>(config.GetSection("Sqdc"));

            services.AddSlack(config);

            services.ConfigureSqdcAutoMapper();

            services.AddSqdcDbContext();
            services.AddTransient<ISqdcDataAccess, SqdcDataAccess>();

            services.AddGenericOpenTypeTransient(typeof(VisitorBase<>));
            services.AddGenericOpenTypeTransient(typeof(IMapper<,>));
            services.AddGenericOpenTypeTransient(typeof(IMappingFilter<,>));

            services.AddScoped<BecameInStockTriggerPolicy>();

            services.AddFactory<IScanOperation, ScanOperation>();
            services.AddScoped<ISqdcWatcher, SqdcHttpWatcher>();

            services.AddTransient<SqdcRestApiClient>();

            services.AddTransient<SqdcProductsFetcher>();
            services.AddTransient<SqdcProductsFileCacheProxy>();
            services.AddTransient<IRemoteStore<ProductDto>>(ctx =>
            {
                IOptions<SqdcConfiguration> sqdcConfiguration = ctx.GetRequiredService<IOptions<SqdcConfiguration>>();
                return sqdcConfiguration.Value.UseProductsHtmlCaching
                    ? (IRemoteStore<ProductDto>) ctx.GetRequiredService<SqdcProductsFileCacheProxy>()
                    : ctx.GetRequiredService<SqdcProductsFetcher>();
            });

            services.BuildServiceProvider();
        }

        private static void ConfigureSerilog(HostBuilderContext context, LoggerConfiguration configuration)
        {
            string logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigDirName, "logs");

            configuration.MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                //.MinimumLevel.Override(DbLoggerCategory.Query.Name, LogEventLevel.Information)
                .MinimumLevel.Override(DbLoggerCategory.Name, LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.File(Path.Combine(logsDirectory, "all.log"), LogEventLevel.Information)
                .WriteTo.File(Path.Combine(logsDirectory, "errors.log"), LogEventLevel.Error);
        }
    }
}