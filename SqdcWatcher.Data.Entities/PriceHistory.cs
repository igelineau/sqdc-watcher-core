using System;


namespace XFactory.SqdcWatcher.Data.Entities
{
    public class PriceHistory
    {
        public long Id { get; set; }

        public long ProductVariantId { get; set; }

        public ProductVariant ProductVariant { get; set; }

        public DateTime Timestamp { get; set; }
        public double? NewDisplayPrice { get; set; }
        public double? NewListPrice { get; set; }
        public double? OldListPrice { get; set; }
        public double? OldDisplayPrice { get; set; }
    }
}