using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SimpleCache.Indexes;

namespace SimpleCache
{
    public interface ISimpleCache<TEntity> where TEntity : IEntity
    {
        TEntity GetEntity(Guid id);
        bool TryGetEntity(Guid id, out TEntity entity);
        bool ContainsEntity(Guid id);

        IEnumerable<TEntity> Items { get; }

        bool ContainsIndexOn<TIndexOn>(
        Expression<Func<TEntity, TIndexOn>> indexExpression);

        ICacheIndex<TEntity,TIndexOn> Index<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> indexExpression);

        ICacheIndexQuery<TEntity> Query();

        void AddOrUpdate(TEntity entity);
        void AddOrUpdateRange(IEnumerable<TEntity> entities);

        void Remove(Guid id);

        void RebuildIndexes();
    }
}
