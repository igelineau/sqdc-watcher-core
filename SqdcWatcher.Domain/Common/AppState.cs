using System;
using JetBrains.Annotations;

namespace XFactory.SqdcWatcher.Data.Entities.Common
{
    [PublicAPI]
    public class AppState
    {
        public int Id { get; private set; }
        public DateTime? LastProductsListRefresh { get; set; }
    }
}