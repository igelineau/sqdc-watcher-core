using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace SqdcWatcher.DataObjects
{
    public class Product
    {
        public string Id { get; set; }
        public string Title { get; set; }
        [Required]
        public string Url { get; set; }

        [Reference]
        public List<ProductVariant> Variants { get; set; }
        
        public Product()
        {
            Variants = new List<ProductVariant>();
        }

    }
}