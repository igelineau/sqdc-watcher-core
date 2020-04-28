using Microsoft.Extensions.Options;
using RestSharp;
using rs = RestSharp;

namespace XFactory.SqdcWatcher.Core.Services
{
    public class SlackPostWebHookClient
    {
        private readonly rs.RestClient client;
        private readonly SqdcAppConfiguration config;

        public SlackPostWebHookClient(IOptions<SqdcAppConfiguration> config)
        {
            this.config = config.Value;
            client = new rs.RestClient("https://hooks.slack.com");
        }

        public void PostToSlack(string message)
        {
            var payload = new
            {
                text = message,
                mrkdwn = true,
                mrkdwn_in = new[] {"text"}
            };
            var request = new rs.RestRequest
            {
                Method = rs.Method.POST,
                Resource = config.SlackPostUrl
            };
            request.AddJsonBody(payload);
            client.Post(request);
        }
    }
}