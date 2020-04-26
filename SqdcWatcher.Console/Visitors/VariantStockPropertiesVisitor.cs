using System;
using XFactory.SqdcWatcher.Data.Entities;

namespace XFactory.SqdcWatcher.ConsoleApp.Visitors
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