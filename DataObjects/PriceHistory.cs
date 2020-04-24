using System;
using System.ComponentModel.DataAnnotations.Schema;
using ServiceStack.DataAnnotations;
using SqdcWatcher.DataAccess;

namespace SqdcWatcher.DataObjects
{
    [TableObject]
    public class PriceHistory
    {
        [PrimaryKey]
        [AutoIncrement]
        public long Id { get; set; }
        
        [Required]
        public long VariantId { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; }
        public double? NewDisplayPrice { get; set; }
        public double? NewListPrice { get; set; }
        public double? OldListPrice { get; set; }
        public double? OldDisplayPrice { get; set; }
    }
}