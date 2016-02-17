using System;
using System.Collections.Generic;

namespace SimpleCache.Builder
{
    public class CacheDefinition
    {
        public List<ICacheIndexDefinition1D> Indexes1D { get; set; }
        public List<ICacheIndexDefinition2D> Indexes2D { get; set; }
    }
}
