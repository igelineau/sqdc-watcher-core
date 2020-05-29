using XFactory.SqdcWatcher.Data.Entities.Markets;

namespace SqdcWatcher.Infrastructure.Abstractions
{
    public interface IMarketFacade
    {
        MarketIdentity MarketIdentity { get; }
    }
}