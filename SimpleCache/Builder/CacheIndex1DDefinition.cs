using System;
using System.Linq.Expressions;

namespace SimpleCache.Builder
{
    public class CacheIndex1DDefinition<TEntity, TIndexOn> : ICacheIndexDefinition1D
        where TEntity : IEntity
    {
        public Expression<Func<TEntity, TIndexOn>> IndexFirstDimension { get; set; }

        public Type FirstIndexOn => typeof (TIndexOn);

        public Expression IndexOnProperty => IndexFirstDimension;
    }
}
