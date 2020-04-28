#region

using System;
using RestSharp;
using rs = RestSharp;

#endregion

namespace XFactory.SqdcWatcher.Core.Services
{
    public class SqdcHttpClientBase
    {
        public const string BASE_DOMAIN = "https://www.sqdc.ca";
        public const string DEFAULT_LOCALE = "en-CA";

        protected readonly rs.RestClient client;

        public SqdcHttpClientBase(string baseUrl)
        {
            client = new rs.RestClient
            {
                BaseUrl = new Uri(baseUrl)
            };

            client.UserAgent = "sqdc-watcher";

            client.DefaultParameters.Clear();

            client.AddDefaultHeader("Accept-Language", DEFAULT_LOCALE);
            client.AddDefaultHeader("Accept-Encoding", "gzip,deflate,br");
            client.AutomaticDecompression = true;
        }

        protected void AddDefaultRequestHeader(string name, string value)
        {
            client.AddDefaultHeader(name, value);
        }
    }
}