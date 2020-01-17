using System;

namespace SqdcWatcher.DataObjects
{
    public class AppState
    {
        public string Key { get; set; }
        public DateTime? LastProductsListRefresh { get; set; }
    }
}