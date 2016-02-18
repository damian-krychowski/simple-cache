using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SimpleCache.Builder
{
    public interface ICacheBuilder<TEntity> 
        where TEntity : IEntity
    {
        ICacheBuilder<TEntity> WithIndex<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> indexExpression);
        
        ISimpleCache<TEntity> BuildUp();
        ISimpleCache<TEntity> BuildUp(IEnumerable<TEntity> entities);
    }
}
