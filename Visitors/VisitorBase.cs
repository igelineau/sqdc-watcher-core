using System.Collections.Generic;

namespace SqdcWatcher.Visitors
{
    public abstract class VisitorBase<T>
    {
        public abstract void Visit(T instance);

        public void VisitAll(ICollection<T> collection)
        {
            foreach (T item in collection)
            {
                Visit(item);
            }
        }
    }
}