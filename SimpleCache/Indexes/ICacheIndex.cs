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
        bool IsOnExpression(Expression indexExpression);
    }

    public interface ICacheIndex<TEntity, TIndexedOn>
        where TEntity: IEntity
    {

        IEnumerable<TIndexedOn> Keys { get; }

        IEnumerable<TEntity> Get(TIndexedOn key);
        IEnumerable<TEntity> GetWithUndefined();

        IEnumerable<Guid> GetIds(TIndexedOn key);
        IEnumerable<Guid> GetIdsWithUndefined();
    }
}
