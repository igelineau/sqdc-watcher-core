using System.Collections.Generic;
using Ardalis.GuardClauses;
using Here;
using JetBrains.Annotations;

namespace XFactory.SqdcWatcher.Data.Entities.Markets
{
    [PublicAPI]
    public class MarketIdentity : ValueObject
    {
        public MarketIdentity(string name)
        {
            Guard.Against.NullOrWhiteSpace(name, nameof(name));

            Name = name;
        }
        
        public string Name { get; private set; }
        
        protected override IEnumerable<object> GetEqualityElements()
        {
            return new[] {Name};
        }
    }
}