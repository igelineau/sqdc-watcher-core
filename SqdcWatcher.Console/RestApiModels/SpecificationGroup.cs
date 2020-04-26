#region

using System.Collections.Generic;
using XFactory.SqdcWatcher.ConsoleApp.Services;

#endregion

namespace XFactory.SqdcWatcher.ConsoleApp.RestApiModels
{
    public class SpecificationGroup
    {
        public string Title { get; set; }
        public List<SpecificationAttributeDto> Attributes { get; set; }
    }
}