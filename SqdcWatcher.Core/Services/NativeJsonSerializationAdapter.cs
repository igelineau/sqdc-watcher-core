#region

using System.Text;
using System.Text.Json;
using RestClient.Net.Abstractions;

#endregion

namespace XFactory.SqdcWatcher.Core.Services
{
    public class NativeJsonSerializationAdapter : ISerializationAdapter
    {
        public TResponseBody Deserialize<TResponseBody>(byte[] data, IHeadersCollection responseHeaders)
        {
            return JsonSerializer.Deserialize<TResponseBody>(data);
        }

        public byte[] Serialize<TRequestBody>(TRequestBody value, IHeadersCollection requestHeaders)
        {
            string json = JsonSerializer.Serialize(value);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}