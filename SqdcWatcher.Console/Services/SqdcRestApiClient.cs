#region

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestSharp;
using SqdcWatcher.RestApiModels;

#endregion

namespace SqdcWatcher.Services
{
    public class SqdcRestApiClient : SqdcHttpClientBase
    {
        private readonly ILogger<SqdcRestApiClient> logger;

        public SqdcRestApiClient(ILogger<SqdcRestApiClient> logger) : base(BASE_DOMAIN + "/api")
        {
            this.logger = logger;

            AddDefaultRequestHeader("Accept", "application/json, text/javascript, */*; q=0.01");
            AddDefaultRequestHeader("X-Requested-With", "XMLHttpRequest");
            AddDefaultRequestHeader("Content-Type", "application/json; charset=utf-8");
        }

        public async Task<VariantsPricesResponse> GetVariantsPrices(IEnumerable<string> productIds, CancellationToken cancelToken)
        {
            return await ExecutePostAsync<VariantsPricesResponse>("product/calculatePrices", new {products = productIds}, cancelToken);
        }

        public async Task<List<long>> GetInventoryItems(IEnumerable<long> variantsIds, CancellationToken cancelToken)
        {
            return (await ExecutePostAsync<List<string>>("inventory/findInventoryItems", new {skus = variantsIds}, cancelToken))
                .Select(long.Parse)
                .ToList();
        }

        public async Task<SpecificationsResponse> GetSpecifications(string productId, string variantId, CancellationToken cancellationToken)
        {
            return await ExecutePostAsync<SpecificationsResponse>("product/specifications",
                new {productId = productId, variantId = variantId}, cancellationToken);
        }

        private async Task<TResponseBody> ExecutePostAsync<TResponseBody>(string resource, object body, CancellationToken cancelToken)
            where TResponseBody : new()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var request = new RestRequest(resource, Method.POST, DataFormat.Json);
            request.AddJsonBody(body);

            IRestResponse<TResponseBody> response = await client.ExecutePostAsync<TResponseBody>(request, cancelToken);

            logger.Log(LogLevel.Information, $"POST {sw.ElapsedMilliseconds}ms {resource}");
            LogRequest(request, response, sw.ElapsedMilliseconds);
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

            Trace.Write(string.Format("Request completed in {0} ms\nRequest: {1}\nResponse: {2}",
                durationMs,
                JsonSerializer.Serialize(requestToLog),
                JsonSerializer.Serialize(responseToLog)));
        }
    }
}