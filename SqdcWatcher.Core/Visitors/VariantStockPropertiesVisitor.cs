using System;
using XFactory.SqdcWatcher.Data.Entities;

namespace XFactory.SqdcWatcher.Core.Visitors
{
    public class VariantStockPropertiesVisitor : VisitorBase<ProductVariant>
    {
        protected override void Visit(ProductVariant instance)
        {
            if (instance.InStock)
            {
                instance.LastInStockTimestamp = DateTime.Now;
            }
        }
    }
}