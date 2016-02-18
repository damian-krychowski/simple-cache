using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCache.Indexes.OneDimensional
{
    internal class IndexMemory<TIndexOn>
    {
        readonly IndexationList _indexationList = new IndexationList();
        readonly List<Guid> _entitiesWithUndefinedKey = new List<Guid>();
        readonly ConcurrentDictionary<TIndexOn, List<Guid>> _index = new ConcurrentDictionary<TIndexOn, List<Guid>>();

        public void InsertWithUndefinedKey(Guid id)
        {
            _entitiesWithUndefinedKey.Add(id);
            _indexationList.MarkIndexation(id, _entitiesWithUndefinedKey);
        }

        public IEnumerable<Guid> IndexedWithUndefinedKey => _entitiesWithUndefinedKey;

        public void Insert(Guid id, TIndexOn key)
        {
            List<Guid> indexList = GetIndexList(key);
            indexList.Add(id);
            _indexationList.MarkIndexation(id, indexList);
        }

        private List<Guid> GetIndexList(TIndexOn key)
        {
            if (!_index.ContainsKey(key))
            {
                _index.TryAdd(key, new List<Guid>());
            }

            return _index[key];
        }

        public IEnumerable<Guid> IndexedWithKey(TIndexOn key)
        {
            if (!_index.ContainsKey(key)) return Enumerable.Empty<Guid>();
            return _index[key];
        }

        public void RemoveIfStored(Guid id)
        {
            _indexationList.RemoveFromLookupAndMemory(id);
        }

        public void Clear()
        {
            _indexationList.Clear();
            _index.Clear();
            _entitiesWithUndefinedKey.Clear();
        }
    }
}