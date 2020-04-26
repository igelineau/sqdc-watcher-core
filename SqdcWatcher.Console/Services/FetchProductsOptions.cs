#region

using System.Collections.Generic;

#endregion

namespace SqdcWatcher.Services
{
    public class FetchProductsOptions
    {
        public HashSet<long> VariantsWithUpToDateSpecs { get; set; }
    }
}