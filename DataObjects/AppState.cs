using System;
using ServiceStack.DataAnnotations;
using SqdcWatcher.DataAccess;

namespace SqdcWatcher.DataObjects
{
    [TableObject]
    public class AppState
    {
        [PrimaryKey]
        public string Key { get; set; }
        
        public DateTime? LastProductsListRefresh { get; set; }
    }
}