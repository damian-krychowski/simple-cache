using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SimpleCache.Indexes.OneDimensional
{
    internal interface ICacheIndex<TEntity>
        where TEntity : IEntity
    {
        void AddOrUpdate(TEntity entity);
        void TryRemove(TEntity entity);
        void TryRemove(Guid entityId);
        void Rebuild();
        void Clear();
        bool IsOnExpression(Expression firstIndexedProperty);
    }

    public interface ICacheIndex<TEntity, TIndexedOn>
        where TEntity: IEntity
    {
        IEnumerable<TEntity> Get(TIndexedOn firstIndexedId);
        IEnumerable<TEntity> GetWithUndefined();
    }
}
