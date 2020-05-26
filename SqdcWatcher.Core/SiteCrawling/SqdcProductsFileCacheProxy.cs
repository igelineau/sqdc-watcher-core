using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SqdcWatcher.DataTransferObjects.RestApiModels;

namespace XFactory.SqdcWatcher.Core.SiteCrawling
{
    public class SqdcProductsFileCacheProxy : DefaultCachingProxy<ProductDto, SqdcProductsFetcher>
    {
        private readonly string productsCacheFile;

        public SqdcProductsFileCacheProxy(IHostEnvironment hostEnvironment, SqdcProductsFetcher innerService) : base(innerService, product => product.Id)
        {
            productsCacheFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "sqdc-watcher",
                $"products-cache{hostEnvironment.EnvironmentName}.json");
        }

        public override async IAsyncEnumerable<ProductDto> GetAllItemsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            bool isCachedDataAvailable = IsCachedDataAvailable();
            bool persistedCacheExists = PersistedCacheExists();
            bool loadingFromPersistentCache = !isCachedDataAvailable && persistedCacheExists;

            IAsyncEnumerable<ProductDto> productsEnumerable;
            if (loadingFromPersistentCache)
            {
                productsEnumerable = (await LoadFromPersistedCache(cancellationToken)).ToAsyncEnumerable();
            }
            else
            {
                productsEnumerable = base.GetAllItemsAsync(cancellationToken);
            }

            var productsToPersist = new List<ProductDto>();
            await foreach (ProductDto productDto in productsEnumerable.WithCancellation(cancellationToken))
            {
                if (!persistedCacheExists)
                {
                    productsToPersist.Add(productDto);
                }

                if (loadingFromPersistentCache)
                {
                    ItemsCache.Add(productDto.Id, productDto);
                }

                yield return productDto;
            }

            if (productsToPersist.Any())
            {
                Directory.CreateDirectory(Path.GetDirectoryName(productsCacheFile));
                await File.WriteAllTextAsync(productsCacheFile, JsonSerializer.Serialize(productsToPersist), cancellationToken);
            }
        }

        private bool PersistedCacheExists()
        {
            return File.Exists(productsCacheFile);
        }

        private async Task<List<ProductDto>> LoadFromPersistedCache(CancellationToken cancellationToken)
        {
            using FileStream inputStream = File.OpenRead(productsCacheFile);
            return await JsonSerializer.DeserializeAsync<List<ProductDto>>(inputStream, new JsonSerializerOptions(), cancellationToken);
        }
    }
}