using System;
using ServiceStack.DataAnnotations;

namespace SqdcWatcher.DataObjects
{
    public class StockHistory
    {
        [PrimaryKey]
        [AutoIncrement]
        public long Id { get; set; }
        
        public long ProductVariantId { get; set; }

        public DateTime Timestamp { get; set; }

        public string Event { get; set; }
    }
}