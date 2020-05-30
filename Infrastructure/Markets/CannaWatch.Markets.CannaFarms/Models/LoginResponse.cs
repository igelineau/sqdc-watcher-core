using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace CannaWatch.Markets.CannaFarms.Models
{
    [PublicAPI]
    public class LoginResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
}