using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace CannaWatch.Markets.CannaFarms.Models
{
    [PublicAPI]
    public class LoginRequest
    {
        public LoginRequest(string userCode, string password)
        {
            ClientId = userCode;
            Password = password;
        }

        [JsonPropertyName("client_id")]
        public string ClientId { get; }
        
        [JsonPropertyName("password")]
        public string Password { get; }
    }
}