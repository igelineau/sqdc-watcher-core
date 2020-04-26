#region

using System;

#endregion

namespace Models.EntityFramework
{
    public class StockHistory
    {
        public long Id { get; set; }

        public long ProductVariantId { get; set; }

        public DateTime Timestamp { get; set; }

        public string Event { get; set; }
    }
}