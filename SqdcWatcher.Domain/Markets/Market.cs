using JetBrains.Annotations;

namespace XFactory.SqdcWatcher.Data.Entities.Markets
{
    [PublicAPI]
    public class Market
    {
        public int Id { get; private set; }
        
        public MarketIdentity Identity { get; private set; }
        
        public Market(MarketIdentity identity, string baseUrl)
        {
            Identity = identity;
            BaseUrl = baseUrl;
        }

        public string BaseUrl { get; private set; }
    }
}