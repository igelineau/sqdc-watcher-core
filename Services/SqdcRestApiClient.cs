using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serialization.Json;
using SqdcWatcher.DataObjects;
using SqdcWatcher.RestApiModels.cs;

namespace SqdcWatcher.Services
{
    public class SqdcRestApiClient : SqdcHttpClientBase
    {
        public SqdcRestApiClient() : base(BASE_DOMAIN + "/api")
        {
            client.AddDefaultHeader("X-Requested-With", "XMLHttpRequest");
        }

        public async Task<VariantsPricesResponse> GetVariantsPrices(IEnumerable<string> productIds)
        {
            return await ExecutePostAsync<VariantsPricesResponse>("product/calculatePrices", new { products = productIds });
        }

        private async Task<TResponseBody> ExecutePostAsync<TResponseBody>(string resource, object body) where TResponseBody : new()
        {
            var request = new RestRequest(resource, Method.POST, DataFormat.Json);
            request.AddJsonBody(body);
            return await client.PostAsync<TResponseBody>(request);
        }
        
        private void LogRequest(IRestRequest request, IRestResponse response, long durationMs)
        {
            var requestToLog = new
            {
                resource = request.Resource,
                // Parameters are custom anonymous objects in order to have the parameter type as a nice string
                // otherwise it will just show the enum value
                parameters = request.Parameters.Select(parameter => new
                {
                    name = parameter.Name,
                    value = parameter.Value,
                    type = parameter.Type.ToString()
                }),
                // ToString() here to have the method as a nice string otherwise it will just show the enum value
                method = request.Method.ToString(),
                // This will generate the actual Uri used in the request
                uri = client.BuildUri(request),
            };

            var responseToLog = new
            {
                statusCode = response.StatusCode,
                content = response.Content,
                headers = response.Headers,
                // The Uri that actually responded (could be different from the requestUri if a redirection occurred)
                responseUri = response.ResponseUri,
                errorMessage = response.ErrorMessage,
            };

            Trace.Write(string.Format("Request completed in {0} ms, Request: {1}, Response: {2}",
                durationMs, 
                JsonConvert.SerializeObject(requestToLog),
                JsonConvert.SerializeObject(responseToLog)));
        }
    }
}