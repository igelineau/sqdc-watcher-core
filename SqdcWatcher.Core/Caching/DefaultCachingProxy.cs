using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using SqdcWatcher.Infrastructure.Abstractions;
using XFactory.SqdcWatcher.Data.Entities.Markets;

namespace XFactory.SqdcWatcher.Core.Caching
{
    public abstract class DefaultCachingProxy<TInnerStore> : IRemoteProductsStore<TInnerStore>
        where TInnerStore : IRemoteProductsStore<TInnerStore>
    {
        private readonly Func<ProductDto, string> entityKeySelector;
        private readonly IRemoteProductsStore<TInnerStore> innerService;
        protected readonly Dictionary<string, ProductDto> ItemsCache = new Dictionary<string, ProductDto>();

        protected DefaultCachingProxy(TInnerStore innerService, Func<ProductDto, string> entityKeySelector)
        {
            this.innerService = innerService;
            this.entityKeySelector = entityKeySelector;
        }

        public virtual async IAsyncEnumerable<ProductDto> GetAllProductsAsync(Market market, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (IsCachedDataAvailable())
            {
                foreach (ProductDto productDto in ItemsCache.Values)
                {
                    yield return productDto;
                }
            }
            else
            {
                await foreach (ProductDto item in innerService.GetAllProductsAsync(market, cancellationToken))
                {
                    ItemsCache.Add(entityKeySelector.Invoke(item), item);
                    yield return item;
                }
            }
        }

        protected bool IsCachedDataAvailable() => ItemsCache.Any();
    }
}