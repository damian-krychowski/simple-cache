using System;
using System.Linq.Expressions;

namespace SimpleCache.Builder
{
    public class CacheIndexDefinition<TEntity, TIndexOn> : ICacheIndexDefinition
        where TEntity : IEntity
    {
        public Expression<Func<TEntity, TIndexOn>> IndexExpression { get; set; }

        public Type IndexType => typeof (TIndexOn);

        public Expression IndexOn => IndexExpression;
    }

    public class CacheSortedIndexDefinition<TEntity, TIndexOn> : ICacheIndexDefinition
        where TEntity : IEntity
    {
        public Expression<Func<TEntity, TIndexOn>> IndexExpression { get; set; }

        public Type IndexType => typeof(TIndexOn);

        public Expression IndexOn => IndexExpression;
    }
}
