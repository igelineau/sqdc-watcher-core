using System.Collections.Generic;
using Ardalis.GuardClauses;
using Here;
using JetBrains.Annotations;

namespace XFactory.SqdcWatcher.Data.Entities.Markets
{
    [PublicAPI]
    public abstract class MarketIdentity : ValueObject
    {
        protected MarketIdentity(string key, string displayName)
        {
            Guard.Against.NullOrWhiteSpace(key, nameof(key));
            Guard.Against.NullOrWhiteSpace(displayName, nameof(displayName));

            Key = key;
            Name = displayName;
        }
        
        public string Name { get; private set; }

        /// <summary>
        /// The Key uniquely identifies a market. It will be used to fetch the Market entity from the database.
        /// </summary>
        public string Key { get; private set; }
        
        protected override IEnumerable<object> GetEqualityElements()
        {
            return new[] {Name};
        }
    }
}