using System.Collections.Generic;

namespace SqdcWatcher.Services
{
    public class GetProductsInfoDto
    {
        public HashSet<long> VariantsWithUpToDateSpecs{ get; set; }
    }
}