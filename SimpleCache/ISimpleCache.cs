using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SimpleCache.Indexes;

namespace SimpleCache
{

    public interface ISimpleCache<TEntity> where TEntity : IEntity
    {
        TEntity GetEntity(Guid id);

        IEnumerable<TEntity> Items { get; }

        bool ContainsIndexOn<TIndexOn>(
        Expression<Func<TEntity, TIndexOn>> indexExpression);

        ICacheIndex<TEntity,TIndexOn> Index<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> indexExpression);

        ICacheIndexQuery<TEntity> Query();

        void AddOrUpdate(TEntity entity);
        void AddOrUpdateRange(IEnumerable<TEntity> entities);

        void TryRemove(TEntity entity);
        void TryRemove(Guid entityId);

        void TryRemoveRange(IEnumerable<TEntity> entities);
        void TryRemoveRange(IEnumerable<Guid> entitiesIds);

        void RebuildIndexes();
        void Clear();
    }
}
