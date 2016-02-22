using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SimpleCache.Indexes
{
    internal class IndexationList<TEntity>
        where TEntity:IEntity
    {
        private readonly Dictionary<Guid, List<TEntity>> _indexationList = new Dictionary<Guid, List<TEntity>>();

        public void MarkIndexation(TEntity entity, List<TEntity> collection)
        {
            collection.Add(entity);
            _indexationList.Add(entity.Id, collection);
        }

        public void RemoveFromLookupAndMemory(Guid id)
        {
            List<TEntity> list;

            if (_indexationList.TryGetValue(id, out list))
            {
                list.RemoveAll(entity => entity.Id == id);
                _indexationList.Remove(id);
            }
        }
    }
}