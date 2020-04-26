#region

using System.Collections.Generic;

#endregion

namespace SqdcWatcher.Visitors
{
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