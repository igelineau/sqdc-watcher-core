using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using SqdcWatcher.Infrastructure.Abstractions;

namespace XFactory.SqdcWatcher.Core.Caching
{
    public abstract class DefaultCachingProxy<TEntity, TInnerStore> : IRemoteStore<TEntity>
        where TEntity : class
        where TInnerStore : class, IRemoteStore<TEntity>
    {
        private readonly Func<TEntity, string> entityKeySelector;
        private readonly TInnerStore innerService;
        protected readonly Dictionary<string, TEntity> ItemsCache = new Dictionary<string, TEntity>();

        protected DefaultCachingProxy(TInnerStore innerService, Func<TEntity, string> entityKeySelector)
        {
            this.innerService = innerService;
            this.entityKeySelector = entityKeySelector;
        }

        public virtual async IAsyncEnumerable<TEntity> GetAllItemsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (IsCachedDataAvailable())
            {
                foreach (TEntity productDto in ItemsCache.Values)
                {
                    yield return productDto;
                }
            }
            else
            {
                await foreach (TEntity item in innerService.GetAllItemsAsync(cancellationToken))
                {
                    ItemsCache.Add(entityKeySelector.Invoke(item), item);
                    yield return item;
                }
            }
        }

        protected bool IsCachedDataAvailable() => ItemsCache.Any();
    }
}