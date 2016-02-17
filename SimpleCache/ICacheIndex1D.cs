using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SimpleCache
{
    internal interface ICacheIndex<TEntity>
        where TEntity : IEntity
    {
        void AddOrUpdate(TEntity entity);
        void TryRemove(TEntity entity);
        void TryRemove(Guid entityId);
    }

    internal interface ICacheIndex1D<TEntity> : ICacheIndex<TEntity>
        where TEntity : IEntity
    {
        bool IsOnExpression(Expression firstIndexedProperty);
    }


    public interface ICacheIndex1D<TEntity, TIndexedOn>
        where TEntity: IEntity
    {
        IEnumerable<TEntity> Get(TIndexedOn firstIndexedId);
    }
}
