#region

using System;

#endregion

namespace Models.EntityFramework
{
    public class AppState
    {
        public int Id { get; set; }

        public DateTime? LastProductsListRefresh { get; set; }
    }
}