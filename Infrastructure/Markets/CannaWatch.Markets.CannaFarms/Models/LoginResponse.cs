using JetBrains.Annotations;

namespace CannaWatch.Markets.CannaFarms.Models
{
    [PublicAPI]
    public class LoginResponse
    {
        public int Id { get; set; }
        public string Token { get; set; }
    }
}