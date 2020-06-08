using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using XFactory.SqdcWatcher.Core.Configuration;

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
            IConfiguration config = hostContext.Configuration;
            services.Configure<SqdcConfiguration>(config.GetSection("Sqdc"));
            
            services.AddHostedService<SqdcWorkerService>();
            services.AddTransient<ConsoleInputInterface>();

            services.AddCannaWatch();
            services.AddCannaWatchSlack(config);
            services.AddCannaWatchSqdcMarket();
            services.AddCannaWatchCannaFarmsMarket(config);

            services.BuildServiceProvider();
        }

        private static void ConfigureSerilog(HostBuilderContext context, LoggerConfiguration configuration)
        {
            string logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigDirName, "logs");

            configuration.MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .EntityFrameworkMinimumLevel( LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.File(Path.Combine(logsDirectory, "all.log"), LogEventLevel.Information)
                .WriteTo.File(Path.Combine(logsDirectory, "errors.log"), LogEventLevel.Error);
        }
    }
}