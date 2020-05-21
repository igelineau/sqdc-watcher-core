﻿

using System.Collections.Generic;
using XFactory.SqdcWatcher.Core.Services;



namespace XFactory.SqdcWatcher.Core.RestApiModels
{
    public class SpecificationGroup
    {
        public string Title { get; set; }
        public List<SpecificationAttributeDto> Attributes { get; set; }
    }
}