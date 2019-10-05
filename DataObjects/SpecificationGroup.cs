using System.Collections.Generic;

namespace SqdcWatcher.DataObjects
{
    public class SpecificationGroup
    {
        public string Title { get; set; }
        public List<SpecificationAttribute> Specification { get; set; }
        public string JsonContext { get; set; }
    }
}