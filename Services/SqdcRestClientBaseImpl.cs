using System;
using RestClient.Net;
using RestClient.Net.Abstractions;

namespace SqdcWatcher.Services
{
    public class SqdcRestClientBaseImpl
    {
        public const string BASE_DOMAIN = "https://www.sqdc.ca";
        public const string DEFAULT_LOCALE = "en-CA";

        protected readonly Client client;

        public SqdcRestClientBaseImpl(string baseUrl)
        {
            client = new Client(new NativeJsonSerializationAdapter(), new Uri(baseUrl));
            
            client.DefaultRequestHeaders.Clear();
            
            AddDefaultRequestHeader("User-Agent", "sqdc-watcher");
            AddDefaultRequestHeader("Accept-Language", DEFAULT_LOCALE);
            AddDefaultRequestHeader("Accept-Encoding", "gzip,deflate,br");
        }

        protected void AddDefaultRequestHeader(string name, string value)
        {
            client.DefaultRequestHeaders.Add(name, value);
        }
    }
}