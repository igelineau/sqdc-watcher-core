using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

namespace XFactory.SqdcWatcher.Core.Configuration
{
    public static class SerilogExtensions
    {
        public static LoggerConfiguration EntityFrameworkMinimumLevel(this LoggerConfiguration loggerConfiguration, LogEventLevel minimumLevel)
        {
            loggerConfiguration.MinimumLevel.Override(DbLoggerCategory.Name, minimumLevel);
            return loggerConfiguration;
        }
    }
}