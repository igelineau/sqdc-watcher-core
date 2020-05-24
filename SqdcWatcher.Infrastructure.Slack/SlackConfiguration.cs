namespace SqdcWatcher.Slack
{
    public class SlackConfiguration
    {
        public string ApiBaseUrl { get; set; }
        public string AccessToken { get; set; }
        public string ChannelName { get; set; }
        public string UserName { get; set; }
    }
}