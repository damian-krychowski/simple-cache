using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCache.Indexes
{
    internal class IndexMemory<TEntity,TIndexOn>
        where TEntity : IEntity
    {
        readonly IndexationList<TEntity> _indexationList = new IndexationList<TEntity>();
        readonly List<TEntity> _entitiesWithUndefinedKey = new List<TEntity>();
        readonly Dictionary<TIndexOn, List<TEntity>> _index = new Dictionary<TIndexOn, List<TEntity>>();

        public void InsertWithUndefinedKey(TEntity entity)
        {
            _entitiesWithUndefinedKey.Add(entity);
            _indexationList.MarkIndexation(entity.Id, _entitiesWithUndefinedKey);
        }

        public IEnumerable<TEntity> IndexedWithUndefinedKey => _entitiesWithUndefinedKey;

        public void Insert(TEntity entity, TIndexOn key)
        {
            List<TEntity> indexList = GetIndexList(key);
            indexList.Add(entity);
            _indexationList.MarkIndexation(entity.Id, indexList);
        }

        private List<TEntity> GetIndexList(TIndexOn key)
        {
            List<TEntity> list;

            if (_index.TryGetValue(key, out list))
            {
                return list;
            }

            list = new List<TEntity>();

            _index.Add(key, list);

            return list;
        }

        public IEnumerable<TEntity> IndexedWithKey(TIndexOn key)
        {
            if (!_index.ContainsKey(key)) return Enumerable.Empty<TEntity>();
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

        public IEnumerable<TIndexOn> Keys => _index.Keys;
    }
}