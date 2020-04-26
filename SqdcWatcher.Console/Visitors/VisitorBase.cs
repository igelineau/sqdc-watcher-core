#region

using System.Collections.Generic;

#endregion

namespace XFactory.SqdcWatcher.ConsoleApp.Visitors
{
    public abstract class VisitorBase
    {
        public static void ApplyVisitors<T>(IEnumerable<VisitorBase<T>> visitors, ICollection<T> itemsToApplyTo)
        {
            foreach (VisitorBase<T> productVisitor in visitors)
            {
                productVisitor.VisitAll(itemsToApplyTo);
            }
        }
    }

    public abstract class VisitorBase<T>
    {
        public abstract void Visit(T instance);

        public void VisitAll(IEnumerable<T> collection)
        {
            foreach (T item in collection)
            {
                Visit(item);
            }
        }
    }
}