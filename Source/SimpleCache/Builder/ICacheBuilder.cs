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

        ISortedIndexBuilder<TEntity> WithSortedIndex<TIndexOn, TSortedBy>(
            Expression<Func<TEntity, TIndexOn>> indexExpression,
            Func<TEntity, TSortedBy> orderBySelector) 
            where TSortedBy : IComparable<TSortedBy>;

        ISimpleCache<TEntity> BuildUp();
        ISimpleCache<TEntity> BuildUp(IEnumerable<TEntity> entities);
    }
}
