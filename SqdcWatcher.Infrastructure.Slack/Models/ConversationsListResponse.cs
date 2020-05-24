using System.Collections.Generic;
using JetBrains.Annotations;

namespace SqdcWatcher.Slack.Models
{
    [UsedImplicitly]
    public class ConversationsListResponse : SlackResponse
    {
        public List<Channel> Channels { get; set; }
    }
}