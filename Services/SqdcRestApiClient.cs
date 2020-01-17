using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alba.CsConsoleFormat;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serialization.Json;
using SqdcWatcher.DataObjects;
using SqdcWatcher.RestApiModels;

namespace SqdcWatcher.Services
{
    public class SqdcRestApiClient : SqdcHttpClientBase
    {
        private readonly ILogger<SqdcRestApiClient> logger;

        public SqdcRestApiClient(ILogger<SqdcRestApiClient> logger) : base(BASE_DOMAIN + "/api")
        {
            this.logger = logger;
            
            client.AddDefaultHeader("Accept", "application/json, text/javascript, */*; q=0.01");
            client.AddDefaultHeader("X-Requested-With", "XMLHttpRequest");
            client.AddDefaultHeader("Content-Type", "application/json; charset=utf-8");
        }

        public async Task<VariantsPricesResponse> GetVariantsPrices(IEnumerable<string> productIds, CancellationToken cancelToken)
        {
            return await ExecutePostAsync<VariantsPricesResponse>("product/calculatePrices", new { products = productIds }, cancelToken);
        }

        public async Task<List<string>> GetInventoryItems(IEnumerable<string> variantsIds, CancellationToken cancelToken)
        {
            return await ExecutePostAsync<List<string>>("inventory/findInventoryItems", new {skus = variantsIds}, cancelToken);
        }

        public async Task<SpecificationsResponse> GetSpecifications(string productId, string variantId, CancellationToken cancellationToken)
        {
            return await ExecutePostAsync<SpecificationsResponse>("product/specifications",
                new {productId = productId, variantId = variantId}, cancellationToken);
        }

        private async Task<TResponseBody> ExecutePostAsync<TResponseBody>(string resource, object body, CancellationToken cancelToken) where TResponseBody : new()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var request = new RestRequest(resource, Method.POST, DataFormat.Json);
            request.AddJsonBody(body);
            
            IRestResponse<TResponseBody> response = await client.ExecutePostTaskAsync<TResponseBody>(request, cancelToken);
            
            logger.Log(LogLevel.Information, $"POST {sw.ElapsedMilliseconds}ms {resource}");
            if (response.IsSuccessful)
            {
                
                return response.Data;
            }
            
            throw new SqdcHttpClientException((response.Data as BaseResponse)?.Message ?? response.ErrorMessage, response.ErrorException);
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