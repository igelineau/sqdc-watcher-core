using System;

namespace XFactory.SqdcWatcher.Data.Entities
{
    public class AppState
    {
        public int Id { get; set; }
        public DateTime? LastProductsListRefresh { get; set; }
    }
}