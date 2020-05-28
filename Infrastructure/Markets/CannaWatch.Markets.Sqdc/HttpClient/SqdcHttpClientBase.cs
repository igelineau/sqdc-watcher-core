using System;
using RestSharp;
using rs = RestSharp;


namespace CannaWatch.Markets.Sqdc.HttpClient
{
    public abstract class SqdcHttpClientBase
    {
        public const string BaseDomain = "https://www.sqdc.ca";
        protected const string DefaultLocale = "en-CA";

        protected readonly rs.RestClient Client;

        protected SqdcHttpClientBase(string baseUrl)
        {
            Client = CreateRestClient(baseUrl);
            AddDefaultHeaders();
        }

        private static rs.RestClient CreateRestClient(string baseUrl)
        {
            return new rs.RestClient
            {
                BaseUrl = new Uri(baseUrl), UserAgent = "sqdc-watcher",
                AutomaticDecompression = true
            };
        }

        private void AddDefaultHeaders()
        {
            Client.DefaultParameters.Clear();
            Client.AddDefaultHeader("Accept-Language", DefaultLocale);
            Client.AddDefaultHeader("Accept-Encoding", "gzip,deflate,br");
        }

        protected void AddDefaultRequestHeader(string name, string value)
        {
            Client.AddDefaultHeader(name, value);
        }
    }
}