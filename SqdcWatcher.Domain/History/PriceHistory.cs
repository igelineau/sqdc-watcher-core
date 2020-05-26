using System;
using JetBrains.Annotations;

namespace XFactory.SqdcWatcher.Data.Entities.History
{
    [PublicAPI]
    public class PriceHistory
    {
        public long Id { get; set; }
        public long ProductVariantId { get; set; }
        public DateTime Timestamp { get; set; }
        public double? NewDisplayPrice { get; set; }
        public double? NewListPrice { get; set; }
        public double? OldListPrice { get; set; }
        public double? OldDisplayPrice { get; set; }
    }
}