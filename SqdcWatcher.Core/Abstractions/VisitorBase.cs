using System.Collections.Generic;

namespace XFactory.SqdcWatcher.Core.Abstractions
{
    public abstract class VisitorBase<T>
    {
        protected abstract void Visit(T instance);

        public void VisitAll(IEnumerable<T> collection)
        {
            foreach (T item in collection) Visit(item);
        }
    }
}