using Ardalis.GuardClauses;
using JetBrains.Annotations;

namespace XFactory.SqdcWatcher.Data.Entities.Markets
{
    [PublicAPI]
    public class Market
    {
        public string Id { get; private set; }

        public string Name { get; set; }
        
        public Market(string id, string name)
        {
            Guard.Against.NullOrWhiteSpace(id, nameof(id));
            Guard.Against.NullOrWhiteSpace(name, nameof(name));
            
            Id = id;
            Name = name;
        }
    }
}