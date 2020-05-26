using System.Collections.Generic;

namespace SqdcWatcher.DataTransferObjects.RestApiModels
{
    public class SpecificationGroup
    {
        public string Title { get; set; }
        public List<SpecificationAttributeDto> Attributes { get; set; }
    }
}