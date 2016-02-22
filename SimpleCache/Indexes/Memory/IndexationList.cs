using System;
using System.Collections.Generic;

namespace SimpleCache.Indexes.Memory
{
    internal class IndexationList<TEntity>
        where TEntity:IEntity
    {
        private readonly Dictionary<Guid, List<TEntity>> _indexationList = new Dictionary<Guid, List<TEntity>>();

        public void MarkIndexation(Guid id, List<TEntity> collection)
        {
            _indexationList.Add(id, collection);
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