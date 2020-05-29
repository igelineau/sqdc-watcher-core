using CannaWatch.Markets.CannaFarms.Configuration;
using CannaWatch.Markets.CannaFarms.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;

namespace CannaWatch.Markets.CannaFarms.HttpClient
{
    [UsedImplicitly]
    public class CannaFarmsAuthenticator : IAuthenticator
    {
        private readonly IOptions<CannaFarmsOptions> options;

        public int? ClientId { get; private set; }

        public CannaFarmsAuthenticator(IOptions<CannaFarmsOptions> options)
        {
            this.options = options;
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            string resourcePath = request?.Resource;
            if (ClientId.HasValue || resourcePath == "login")
            {
                return;
            }
            
            const string path = "login";
            string userCode = options.Value.UserCode;
            string password = options.Value.Password;

            var loginRequest = new RestRequest(path);
            loginRequest.AddJsonBody(new LoginRequest(userCode, password));
            IRestResponse<LoginResponse> response = client.Post<LoginResponse>(loginRequest);
            ClientId = response.Data.Id;
            client.AddDefaultHeader("Authorization", $"Token {response.Data.Token}");
        }
    }
}