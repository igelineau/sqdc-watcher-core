using System.Collections.Generic;
using XFactory.SqdcWatcher.Core.Visitors;

namespace XFactory.SqdcWatcher.Core.Abstractions
{
    public static class VisitorsInvoker
    {
        public static void ApplyVisitors<T>(IEnumerable<VisitorBase<T>> visitors, ICollection<T> itemsToApplyTo)
        {
            foreach (VisitorBase<T> productVisitor in visitors)
            {
                productVisitor.VisitAll(itemsToApplyTo);
            }
        }
    }
}