using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;
using SqdcWatcher.Infrastructure;
using SqdcWatcher.Slack.Models;
using rs = RestSharp;

namespace SqdcWatcher.Slack
{
    public class SlackClient : ISlackClient
    {
        private readonly RestClient client;
        private readonly SlackConfiguration config;
        private readonly ILogger<SlackClient> logger;

        private string encodedConversationId;

        public SlackClient(IOptions<SlackConfiguration> config, ILogger<SlackClient> logger)
        {
            this.logger = logger;
            this.config = config.Value;
            client = new RestClient(config.Value.ApiBaseUrl)
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(this.config.AccessToken, "Bearer")
            };

            string postTarget = config.Value.UserName != null ? $"user {config.Value.UserName}" : $"channel {config.Value.ChannelName}";
            logger.LogInformation($"Initializing Slack client. Posting to {postTarget}");
        }

        public async Task PostToSlackAsync(string message)
        {
            string conversationId = await GetEncodedConversationIdAsync();

            logger.LogDebug($"Posting to Slack : \"{message}\"");
            var payload = new
            {
                channel = conversationId,
                text = message
            };
            var request = new RestRequest
            {
                Method = Method.POST,
                Resource = "chat.postMessage"
            };
            request.AddJsonBody(payload);
            client.Post(request);
        }

        private async Task<string> GetEncodedConversationIdAsync()
        {
            const string methodName = "conversations.list";

            if (encodedConversationId != null) return encodedConversationId;

            string type = config.UserName != null ? "im" : "private_channel";
            string name = config.UserName ?? config.ChannelName;
            var request = new RestRequest(methodName);
            request.AddQueryParameter("types", type);
            IRestResponse<ConversationsListResponse> response = await client.ExecuteAsync<ConversationsListResponse>(request);
            ValidateConversationListResponse(response);

            Func<Channel, bool> channelPredicate;
            if (config.UserName != null)
            {
                Dictionary<string, SlackUser> usersByName = (await GetUsers()).ToDictionary(u => u.Name.ToLower());
                usersByName.TryGetValue(name.ToLower(), out SlackUser user);
                if (user == null) throw new SlackException($"Slack user not found in im list results: {config.UserName}");
                channelPredicate = c => c.User == user.Id;
            }
            else
            {
                channelPredicate = c => c.Name == name;
            }

            encodedConversationId = response.Data.Channels.SingleOrDefault(channelPredicate)?.Id;
            ValidateEncodedConversationId(name, type);
            return encodedConversationId;
        }

        private async Task<List<SlackUser>> GetUsers()
        {
            const string methodName = "users.list";
            var request = new RestRequest(methodName);
            IRestResponse<SlackUsersListResponse> response = await client.ExecuteAsync<SlackUsersListResponse>(request);
            return response.Data.Members;
        }

        private static void ValidateConversationListResponse(IRestResponse<ConversationsListResponse> response)
        {
            if (!response.IsSuccessful || response.Data == null) throw new SlackException("Slack API error", response.ErrorException);

            if (response.Data?.Error != null) throw new SlackException($"Slack API error: {response.Data.Error}");
        }

        private void ValidateEncodedConversationId(string channelName, string conversationType)
        {
            if (encodedConversationId == null)
                throw new SlackException(
                    $"Could not determine the conversation ID for the channel name '{channelName}' of type '{conversationType}'."
                    + $" Make sure you have the necessary scopes.");
        }
    }
}