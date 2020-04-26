#region

using System;

#endregion

namespace Models.EntityFramework
{
    public class PriceHistory
    {
        public long Id { get; set; }

        public long VariantId { get; set; }

        public ProductVariant ProductVariant { get; set; }

        public DateTime Timestamp { get; set; }
        public double? NewDisplayPrice { get; set; }
        public double? NewListPrice { get; set; }
        public double? OldListPrice { get; set; }
        public double? OldDisplayPrice { get; set; }
    }
}