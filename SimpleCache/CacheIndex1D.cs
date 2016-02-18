using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SimpleCache.ExtensionMethods;

namespace SimpleCache
{
    internal class IndexationList
    {
        readonly ConcurrentDictionary<Guid, IList<Guid>> _indexationList = new ConcurrentDictionary<Guid, IList<Guid>>();

        public bool WasIndexed(Guid id)
        {
            return _indexationList.ContainsKey(id);
        }

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
    }


    internal class CacheIndex1D<TEntity, TIndexOn> : 
        ICacheIndex1D<TEntity, TIndexOn>, 
        ICacheIndex1D<TEntity>
        where TEntity : IEntity
    {
        private class IndexMemory
        {
            readonly IndexationList _indexationList = new IndexationList();
            readonly List<Guid> _entitiesWithUndefinedKey = new List<Guid>(); 
            readonly ConcurrentDictionary<TIndexOn, List<Guid>> _index = new ConcurrentDictionary<TIndexOn, List<Guid>>();

            public bool WasIndexed(Guid id)
            {
                return _indexationList.WasIndexed(id);
            }

            public void InsertWithUndefinedKey(Guid id)
            {
               _entitiesWithUndefinedKey.Add(id);
               _indexationList.MarkIndexation(id, _entitiesWithUndefinedKey);
            }

            public IEnumerable<Guid> IndexedWithUndefinedKey()
            {
                return _entitiesWithUndefinedKey;
            } 

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
                if(!_index.ContainsKey(key)) throw new InvalidOperationException($"Items with {key} key were not stored.");
                return _index[key];
            } 
            
            public void RemoveIfStored(Guid id)
            {
                _indexationList.RemoveFromLookupAndMemory(id);
            }
        }

        readonly List<Guid> _withUndefinedIndexingValue = new List<Guid>(); 
        readonly ConcurrentDictionary<Guid, TIndexOn> _indexationList = new ConcurrentDictionary<Guid, TIndexOn>(); 
        readonly ConcurrentDictionary<TIndexOn, List<Guid>> _index = new ConcurrentDictionary<TIndexOn, List<Guid>>();

        Func<TEntity, TIndexOn> _indexFunc;
        ISimpleCache<TEntity> _parentCache;

        public bool IsOnExpression(Expression firstIndexedProperty)
        {
            return firstIndexedProperty.Comapre(FirstPropertyWithIndex);
        }

        public void AddOrUpdate(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            
            TIndexOn indexKey = _indexFunc(entity);
            bool wasAlreadyIndexed = _indexationList.ContainsKey(entity.Id);

            if (indexKey != null)
            {
                if (wasAlreadyIndexed)
                {
                    var previousIndexKey = _indexationList[entity.Id];

                    if (!previousIndexKey.Equals(indexKey))
                    {
                        RemoveEntity(entity.Id);
                        AddItemToIndex(indexKey, entity.Id);
                    }
                }
                else
                {
                    AddItemToIndex(indexKey, entity.Id);
                }
            }
            else
            {
                _withUndefinedIndexingValue.Add(entity.Id);
            }
        }

        public void TryRemove(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            TryRemove(entity.Id);
        }

        public void TryRemove(Guid entityId)
        {
            if (_indexationList.ContainsKey(entityId))
            {
                RemoveEntity(entityId);
            }
        }

        public void Rebuild()
        {
            Clear();

            foreach (var entity in _parentCache.Items)
            {
                AddOrUpdate(entity);
            }
        }

        public void Clear()
        {
            _index.Clear();
            _indexationList.Clear();
        }

        public IEnumerable<TEntity> GetWithUndefined()
        {
            throw new NotImplementedException();
        }

        private void RemoveEntity(Guid entityId)
        {
            var indexKey = _indexationList[entityId];
            _index[indexKey].Remove(entityId);            
        }

        public void Initialize(
            Expression<Func<TEntity, TIndexOn>> firstIndexedProperty, 
            ISimpleCache<TEntity> parentCache)
        {
            if (firstIndexedProperty == null) throw new ArgumentNullException(nameof(firstIndexedProperty));
            if (parentCache == null) throw new ArgumentNullException(nameof(parentCache));

            _indexFunc = firstIndexedProperty.Compile();
            FirstPropertyWithIndex = firstIndexedProperty;
            _parentCache = parentCache;
        }

        public void BuildUp(IEnumerable<TEntity> entities)
        {
            foreach(TEntity entity in entities)
            {
                AddOrUpdate(entity);
            }
        }

        public Expression<Func<TEntity, TIndexOn>> FirstPropertyWithIndex
        {
            get;
            private set;
        }

        public IEnumerable<TEntity> Get(TIndexOn firstIndexedId)
        {
            if(firstIndexedId == null) throw  new ArgumentNullException(nameof(firstIndexedId));

            foreach (var entityId in GetIndexList(firstIndexedId))
            {
                yield return _parentCache.GetEntity(entityId);
            }
        }

        private List<Guid> GetIndexList(TIndexOn indexKey)
        {
            if (!_index.ContainsKey(indexKey))
            {
                _index.TryAdd(indexKey, new List<Guid>());
            }

            return _index[indexKey];
        }

        private void AddItemToIndex(TIndexOn indexKey, Guid itemId)
        {
            List<Guid> indexList = GetIndexList(indexKey);
            _indexationList.AddOrUpdate(itemId, indexKey, (k, v) => indexKey);
            indexList.Add(itemId);
        }
    }
}
