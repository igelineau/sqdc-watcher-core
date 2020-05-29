using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CannaWatch.Markets.CannaFarms.Models;
using RestSharp;


namespace CannaWatch.Markets.CannaFarms.HttpClient
{
    public class CannaFarmsRestClient
    {
        private readonly CannaFarmsAuthenticator authenticator;
        private readonly Uri baseDomain = new Uri("https://ample.cannafarms.ca:3000");
        private const string BaseApiPath = "v1/portal/clients";
        
        private readonly RestClient client;

        private int? clientId;

        public CannaFarmsRestClient(CannaFarmsAuthenticator authenticator)
        {
            this.authenticator = authenticator;
            
            client = CreateRestClient(new Uri(baseDomain, BaseApiPath));
            AddDefaultHeaders();
        }

        private RestClient CreateRestClient(Uri baseApiUri)
        {
            return new RestClient
            {
                BaseUrl = new Uri(baseApiUri.ToString()),
                UserAgent = "CannaWatcher",
                AutomaticDecompression = true,
                Authenticator = authenticator
            };
        }

        private void AddDefaultHeaders()
        {
            client.DefaultParameters.Clear();
            client.AddDefaultHeader("Accept-Language", "en-CA,en-US;q=0.7,en;q=0.3");
            client.AddDefaultHeader("Accept-Encoding", "gzip,deflate,br");
            
            client.AddDefaultHeader("Accept", "application/json, text/javascript, */*; q=0.01");
            client.AddDefaultHeader("Content-Type", "application/json; charset=utf-8");
        }
        
        public async Task<List<CannaFarmProduct>> GetPurchasableProducts()
        {
            if (clientId == null)
            {
                authenticator.Authenticate(client, null);
                clientId = authenticator.ClientId 
                           ?? throw new CannaFarmsMarketFetchException("No client Id found after authentication");
            }
            
            string path = $"{clientId}/purchasable_products";
            var request = new RestRequest(path);
            IRestResponse<List<CannaFarmProduct>> response = await client.ExecuteAsync<List<CannaFarmProduct>>(request);
            return response.Data;
        }
    }
}