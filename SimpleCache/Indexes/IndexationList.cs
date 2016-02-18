using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SimpleCache.Indexes
{
    internal class IndexationList
    {
        readonly ConcurrentDictionary<Guid, IList<Guid>> _indexationList = new ConcurrentDictionary<Guid, IList<Guid>>();

        public void MarkIndexation(Guid id, IList<Guid> collection)
        {
            _indexationList.AddOrUpdate(id, collection, (k, v) => collection);
        }

        public void RemoveFromLookupAndMemory(Guid id)
        {
            IList<Guid> collection;

            if (_indexationList.TryRemove(id, out collection))
            {
                collection.Remove(id);
            }
        }

        public void Clear()
        {
            _indexationList.Clear();
        }
    }
}