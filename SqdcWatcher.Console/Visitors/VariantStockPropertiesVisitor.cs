#region

using System;
using Models.EntityFramework;

#endregion

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