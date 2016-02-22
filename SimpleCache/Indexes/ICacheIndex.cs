using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SimpleCache.Indexes
{
    internal interface ICacheIndex<in TEntity>
        where TEntity : IEntity
    { 
        void AddOrUpdate(TEntity entity);
        void TryRemove(Guid entityId);
        void Rebuild();
        bool IsOnExpression(Expression indexExpression);
    }

    public interface ICacheIndex<TEntity, TIndexedOn>
        where TEntity: IEntity
    {

        IEnumerable<TIndexedOn> Keys { get; }

        List<TEntity> Get(TIndexedOn key);
        List<TEntity> GetWithUndefined();
    }
}
