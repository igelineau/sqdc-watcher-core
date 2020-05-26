using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using XFactory.SqdcWatcher.Core.Abstractions;
using XFactory.SqdcWatcher.Core.Services;
using XFactory.SqdcWatcher.Data.Entities.Products;
using XFactory.SqdcWatcher.Data.Entities.ProductVariant;

namespace XFactory.SqdcWatcher.Core.Visitors
{
    [UsedImplicitly]
    public class MoveSpecificationsToPropertiesVisitor : VisitorBase<Product>
    {
        protected override void Visit(Product instance)
        {
            bool hasAssignedProduct = false;
            foreach (ProductVariant variant in instance.Variants)
            {
                var specsNamesToRemove = new List<string>();
                if (!hasAssignedProduct)
                {
                    specsNamesToRemove = SpecificationToUpperEntities.CopySpecificationsToObject(instance, variant.Specifications);
                    hasAssignedProduct = true;
                }

                IEnumerable<string> allSpecsToRemove =
                    specsNamesToRemove.Union(SpecificationToUpperEntities.CopySpecificationsToObject(variant, variant.Specifications));
                variant.Specifications.RemoveAll(spec => allSpecsToRemove.Contains(spec.PropertyName));
            }
        }
    }
}