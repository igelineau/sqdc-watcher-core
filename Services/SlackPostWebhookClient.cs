using Microsoft.Extensions.Options;
using RestSharp;

namespace SqdcWatcher.Services
{
    public class SlackPostWebHookClient
    {
        private readonly SqdcAppConfiguration config;
        private RestClient client;

        public SlackPostWebHookClient(IOptions<SqdcAppConfiguration> config)
        {
            this.config = config.Value;
            client = new RestClient("https://hooks.slack.com");
        }

        public void PostToSlack(string message)
        {
            var payload = new
            {
                text = message,
                mrkdwn = true,
                mrkdwn_in = new[] {"text"}
            };
            var request = new RestRequest
            {
                Method = Method.POST,
                Resource = config.SlackPostUrl
            };
            request.AddJsonBody(payload);
            client.Post(request);
        }
    }
}