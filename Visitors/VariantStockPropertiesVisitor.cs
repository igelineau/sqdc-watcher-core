using System;
using SqdcWatcher.DataObjects;

namespace SqdcWatcher.Visitors
{
    public class VariantStockPropertiesVisitor : VisitorBase<ProductVariant>
    {
        public override void Visit(ProductVariant instance)
        {
            if (instance.InStock)
            {
                instance.LastInStockTimestamp = DateTime.Now;
            }
        }
    }
}