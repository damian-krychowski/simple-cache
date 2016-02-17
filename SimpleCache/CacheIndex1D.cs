using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SimpleCache.ExtensionMethods;

namespace SimpleCache
{

    internal class CacheIndex1D<TEntity, TIndexOn> : 
        ICacheIndex1D<TEntity, TIndexOn>, 
        ICacheIndex1D<TEntity>
        where TEntity : IEntity
    {
        #region Globals
        readonly ConcurrentDictionary<Guid, TIndexOn> _indexationList = new ConcurrentDictionary<Guid, TIndexOn>(); 
        readonly ConcurrentDictionary<TIndexOn, List<Guid>> _index = new ConcurrentDictionary<TIndexOn, List<Guid>>();
        readonly ConcurrentDictionary<TIndexOn, object> _locks = new ConcurrentDictionary<TIndexOn, object>();

        Func<TEntity, TIndexOn> _indexFunc;
        ISimpleCache<TEntity> _parentCache;
        Func<TEntity, bool> _indexedItems;
        #endregion

        #region Constructors
        #endregion

        #region ICacheIndex1D
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

        private void RemoveEntity(Guid entityId)
        {
            var indexKey = _indexationList[entityId];

            lock (_locks[indexKey])
            {
                _index[indexKey].Remove(entityId);
            }
        }

        #endregion

        #region ICacheIndex1D<TEntity>
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
            return GetIndexList(firstIndexedId)
                .Select(itemId => _parentCache.GetEntity(itemId));
        }

        #endregion

        #region Help Methods
        private List<Guid> GetIndexList(TIndexOn indexKey)
        {
            if (!_index.ContainsKey(indexKey))
            {
                _index.TryAdd(indexKey, new List<Guid>());
                _locks.TryAdd(indexKey, new object());
            }

            return _index[indexKey];
        }

        private void AddItemToIndex(TIndexOn indexKey, Guid itemId)
        {
            List<Guid> indexList = GetIndexList(indexKey);

            lock (_locks[indexKey])
            {
                indexList.Add(itemId);
            }
        }
        #endregion
    }
}
