using System;

namespace SimpleCache.Indexes
{
    internal interface ICacheIndex<TEntity>
        where TEntity : IEntity
    {
        void AddOrUpdate(TEntity entity);
        void TryRemove(TEntity entity);
        void TryRemove(Guid entityId);
        void Rebuild();
        void Clear();
    }
}