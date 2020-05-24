using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqdcWatcher.Infrastructure;

namespace SqdcWatcher.Slack.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSlack(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SlackConfiguration>(configuration.GetSection("Slack"));
            services.AddTransient<ISlackClient, SlackClient>();
        }
    }
}