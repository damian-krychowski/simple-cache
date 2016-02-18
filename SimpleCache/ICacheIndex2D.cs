using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SimpleCache
{
    internal interface ICacheIndex2D<TEntity> : ICacheIndex<TEntity>
        where TEntity:IEntity
    {
        bool IsOnExpression(
            Expression firstIndexedProperty,
            Expression secondIndexedProperty);
    }

    public interface ICacheIndex2D<out TEntity, in TIndexedOnFirst, in TIndexedOnSecond>
        where TEntity: IEntity
    {
        IEnumerable<TEntity> GetFromFirst(TIndexedOnFirst firstIndexedId);
        IEnumerable<TEntity> GetFromSecond(TIndexedOnSecond secondIndexedId);
        IEnumerable<TEntity> Get(TIndexedOnFirst firstIndexedId, TIndexedOnSecond secondIndexedId);
        IEnumerable<TEntity> GetWithFirstUndefined(TIndexedOnSecond secondIndexedId);
        IEnumerable<TEntity> GetWithSecondUndefined(TIndexedOnFirst firstIndexedId);
        IEnumerable<TEntity> GetWithBothUndefined();
    }
}
