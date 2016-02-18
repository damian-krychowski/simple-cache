using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Messaging;
using SimpleCache.Builder;
using SimpleCache.Exceptions;
using SimpleCache.ExtensionMethods;
using SimpleCache.Indexes;
using SimpleCache.Indexes.OneDimensional;

namespace SimpleCache
{
    internal class SimpleCache<TEntity> : ISimpleCache<TEntity>
        where TEntity : IEntity
    {
        #region Globals
        readonly ConcurrentDictionary<Guid, TEntity> _items = new ConcurrentDictionary<Guid, TEntity>();

        readonly List<ICacheIndex<TEntity>> _indexes = new List<ICacheIndex<TEntity>>();
        #endregion

        #region ISimpleCache
        public void Initialize(CacheDefinition cacheDefinition)
        {
            var cacheIndexFactory = new CacheIndexFactory<TEntity>();

            foreach (var definition in cacheDefinition.Indexes)
            {
                var index = cacheIndexFactory.CreateCacheIndex1D(definition, this);
                _indexes.Add(index);
            }
        }

        public TEntity GetEntity(Guid id)
        {
            if (_items.ContainsKey(id))
            {
                return _items[id];
            }
            return default(TEntity);
        }

        public IEnumerable<TEntity> Items => _items.Values;

        public ICacheIndex<TEntity, TIndexOn> Index1D<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> firstIndexedProperty)
        {
            if(firstIndexedProperty == null) throw new ArgumentNullException(nameof(firstIndexedProperty));

            ICacheIndex<TEntity, TIndexOn> index = FindIndex(firstIndexedProperty);

            if (index == null) throw new IndexNotFoundException("Index for this property was not registered!");

            return index;
        }

        public bool ContainsIndexOn<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> firstIndexedProperty)
        {
            if(firstIndexedProperty == null) throw  new ArgumentNullException(nameof(firstIndexedProperty));

            return FindIndex(firstIndexedProperty) != null;
        }


        public void AddOrUpdate(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            _items.AddOrUpdate(entity.Id, entity, (k, v) => entity);
            AddToIndexes(entity);
        }

        public void AddOrUpdateRange(IEnumerable<TEntity> entities)
        {
            if(entities == null) throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
            {
                AddOrUpdate(entity);
            }
        }

        public void TryRemove(TEntity entity)
        {
            if(entity == null) throw new ArgumentNullException(nameof(entity));
            TryRemove(entity.Id);
        }

        public void TryRemove(Guid entityId)
        {
            TEntity entity;
            _items.TryRemove(entityId, out entity);
            RemoveFromIndexes(entityId);
        }

        public void TryRemoveRange(IEnumerable<TEntity> entities)
        {
            if(entities == null) throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
            {
                TryRemove(entity);
            }
        }

        public void TryRemoveRange(IEnumerable<Guid> entitiesIds)
        {
            if (entitiesIds == null) throw new ArgumentNullException(nameof(entitiesIds));

            foreach (var entityId in entitiesIds)
            {
                TryRemove(entityId);
            }
        }

        public void RebuildIndexes()
        {
            foreach (var cacheIndex in _indexes)
            {
                cacheIndex.Rebuild();
            }
        }

        public void Clear()
        {
            foreach (var cacheIndex in _indexes)
            {
                cacheIndex.Clear();
            }

            _items.Clear();
        }
        #endregion

        #region Help Methods

        private void AddToIndexes(TEntity entity)
        {
            foreach (var cacheIndex in _indexes)
            {
                cacheIndex.AddOrUpdate(entity);
            }
        }

        private void RemoveFromIndexes(Guid entityId)
        {
            foreach (var cacheIndex in _indexes)
            {
                cacheIndex.TryRemove(entityId);
            }
        }

        private ICacheIndex<TEntity, TIndexOn> FindIndex<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> firstIndexedProperty)
        {
           var index = _indexes.FirstOrDefault(x => x.IsOnExpression(firstIndexedProperty));

            if (index == null)
            {
                throw new IndexNotFoundException();    
            }

            return index as ICacheIndex<TEntity, TIndexOn>;
        }
        #endregion
    }
}
