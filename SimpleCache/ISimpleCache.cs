using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SimpleCache.Indexes.OneDimensional;
using SimpleCache.Indexes.TwoDimensional;

namespace SimpleCache
{

    public interface ISimpleCache<TEntity> where TEntity : IEntity
    {
        TEntity GetEntity(Guid id);

        IEnumerable<TEntity> Items { get; }

        bool ContainsIndexOn<TIndexOn>(
        Expression<Func<TEntity, TIndexOn>> firstIndexedProperty);

        ICacheIndex1D<TEntity,TIndexOn> Index1D<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> firstIndexedProperty);

        bool ContainsIndexOn<TIndexOnFirst,TIndexOnSecond>(
            Expression<Func<TEntity, TIndexOnFirst>> firstIndexedProperty,
            Expression<Func<TEntity, TIndexOnSecond>> secondIndexedProperty);

        ICacheIndex2D<TEntity, TIndexOnFirst, TIndexOnSecond> Index2D<TIndexOnFirst, TIndexOnSecond>(
            Expression<Func<TEntity, TIndexOnFirst>> firstIndexedProperty,
            Expression<Func<TEntity, TIndexOnSecond>> secondIndexedProperty);

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
