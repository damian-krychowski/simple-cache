using System;
using System.Collections.Generic;

namespace SimpleCache.Builder
{
    public class CacheDefinition
    {
        public List<ICacheIndexDefinition> Indexes { get; set; }
    }
}
