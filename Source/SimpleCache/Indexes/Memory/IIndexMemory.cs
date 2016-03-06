using System;
using System.Collections.Generic;

namespace SimpleCache.Indexes.Memory
{
    internal interface IIndexMemory<TEntity, TIndexOn>
        where TEntity : IEntity
    {
        List<TEntity> IndexedWithUndefinedKey();
        List<TIndexOn> Keys();
        List<TEntity> IndexedWithKey(TIndexOn key);

        void InsertWithUndefinedKey(TEntity entity);
        void Insert(TEntity entity, TIndexOn key);
        void RemoveIfStored(Guid id);
    }
}