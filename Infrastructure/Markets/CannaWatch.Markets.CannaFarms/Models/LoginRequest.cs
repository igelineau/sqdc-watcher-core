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

        public string ClientId { get; }
        public string Password { get; }
    }
}