using System.Collections.Generic;

namespace SqdcWatcher.Slack.Models
{
    public class SlackUsersListResponse : SlackResponse
    {
        public List<SlackUser> Members { get; set; }
    }
}