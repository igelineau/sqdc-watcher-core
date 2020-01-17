using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SqdcWatcher.DataObjects;

namespace SqdcWatcher.Visitors
{
    [UsedImplicitly]
    public class MoveSpecificationsToPropertiesVisitor : VisitorBase<Product>
    {
        public override void Visit(Product product)
        {
            bool hasAssignedProduct = false;
            foreach(ProductVariant variant in product.Variants)
            {
                var specsNamesToRemove = new List<string>();
                if(!hasAssignedProduct)
                {
                    specsNamesToRemove = SpecificationCopier.CopySpecificationsToObject(product, variant.Specifications);
                    hasAssignedProduct = true;
                }

                IEnumerable<string> allSpecsToRemove =
                    specsNamesToRemove.Union(SpecificationCopier.CopySpecificationsToObject(variant, variant.Specifications));
                variant.Specifications.RemoveAll(spec => allSpecsToRemove.Contains(spec.PropertyName));
            }
        }
    }
}