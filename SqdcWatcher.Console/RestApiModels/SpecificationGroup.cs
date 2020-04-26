#region

using System.Collections.Generic;
using SqdcWatcher.Services;

#endregion

namespace SqdcWatcher.DataObjects
{
    public class SpecificationGroup
    {
        public string Title { get; set; }
        public List<SpecificationAttributeDto> Attributes { get; set; }
    }
}