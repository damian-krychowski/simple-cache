using System.Collections.Generic;
using System.Linq.Expressions;

namespace SimpleCache.Indexes.OneDimensional
{
    internal interface ICacheIndex1D<TEntity> : ICacheIndex<TEntity>
        where TEntity : IEntity
    {
        bool IsOnExpression(Expression firstIndexedProperty);
    }


    public interface ICacheIndex1D<TEntity, TIndexedOn>
        where TEntity: IEntity
    {
        IEnumerable<TEntity> Get(TIndexedOn firstIndexedId);
        IEnumerable<TEntity> GetWithUndefined();
    }
}
