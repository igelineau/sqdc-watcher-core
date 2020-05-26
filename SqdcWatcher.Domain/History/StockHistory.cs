using System;
using JetBrains.Annotations;

namespace XFactory.SqdcWatcher.Data.Entities.History
{
    [PublicAPI]
    public class StockHistory
    {
        public long Id { get; set; }

        public long ProductVariantId { get; set; }

        public DateTime Timestamp { get; set; }

        public string Event { get; set; }
    }
}