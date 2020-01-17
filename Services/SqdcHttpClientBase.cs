using System;
using RestSharp;

namespace SqdcWatcher.Services
{
    public class SqdcHttpClientBase
    {
        public const string BASE_DOMAIN = "https://www.sqdc.ca";
        public const string DEFAULT_LOCALE = "en-CA";

        protected readonly RestClient client;

        public SqdcHttpClientBase(string baseUrl)
        {
            client = new RestClient
            {
                BaseUrl = new Uri(baseUrl)
            };

            client.UserAgent = "sqdc-watcher";
            
            client.DefaultParameters.Clear();
            
            client.AddDefaultHeader("Accept-Language", DEFAULT_LOCALE);
            client.AddDefaultHeader("Accept-Encoding", "gzip,deflate,br");
            client.AutomaticDecompression = true;
        }
    }
}