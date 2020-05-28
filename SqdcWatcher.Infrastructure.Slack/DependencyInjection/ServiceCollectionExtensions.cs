using Microsoft.Extensions.Configuration;
using SqdcWatcher.Infrastructure.Abstractions;
using SqdcWatcher.Slack;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCannaWatchSlack(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SlackConfiguration>(configuration.GetSection("Slack"));
            services.AddTransient<ISlackClient, SlackClient>();
        }
    }
}