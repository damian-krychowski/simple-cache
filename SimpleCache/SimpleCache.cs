using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Messaging;
using SimpleCache.Builder;
using SimpleCache.Exceptions;
using SimpleCache.ExtensionMethods;

namespace SimpleCache
{
    internal class SimpleCache<TEntity> : ISimpleCache<TEntity>
        where TEntity : IEntity
    {
        #region Globals
        readonly ConcurrentDictionary<Guid, TEntity> _items = new ConcurrentDictionary<Guid, TEntity>();

        readonly List<ICacheIndex1D<TEntity>> _indexes1D = new List<ICacheIndex1D<TEntity>>();
        readonly List<ICacheIndex2D<TEntity>> _indexes2D = new List<ICacheIndex2D<TEntity>>();
        #endregion

        #region ISimpleCache
        public void Initialize(CacheDefinition cacheDefinition)
        {
            var cacheIndexFactory = new CacheIndexFactory<TEntity>();

            foreach(var definition in cacheDefinition.Indexes1D)
            {
                var index = cacheIndexFactory.CreateCacheIndex1D(definition, this);
                _indexes1D.Add(index);
            }

            foreach (var definition in cacheDefinition.Indexes2D)
            {
                var index = cacheIndexFactory.CreateCacheIndex2D(definition, this);
                _indexes2D.Add(index);
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

        public ICacheIndex1D<TEntity, TIndexOn> Index1D<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> firstIndexedProperty)
        {
            ICacheIndex1D<TEntity, TIndexOn> index = FindIndex(firstIndexedProperty);

            if (index == null) throw new IndexNotFoundException("Index for this property was not registered!");

            return index;
        }

        public bool ContainsIndexOn<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> firstIndexedProperty)
        {
            return FindIndex(firstIndexedProperty) != null;
        }

        public ICacheIndex2D<TEntity,TIndexOnFirst,TIndexOnSecond> Index2D<TIndexOnFirst, TIndexOnSecond>(
            Expression<Func<TEntity, TIndexOnFirst>> firstIndexedProperty,
            Expression<Func<TEntity, TIndexOnSecond>> secondIndexedProperty)
        {
            ICacheIndex2D<TEntity, TIndexOnFirst, TIndexOnSecond> index = FindIndex(firstIndexedProperty, secondIndexedProperty);

            if (index == null) throw new IndexNotFoundException("Index for this property was not registered!");

            return index;
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

        public bool ContainsIndexOn<TIndexOnFirst, TIndexOnSecond>(
            Expression<Func<TEntity, TIndexOnFirst>> firstIndexedProperty,
            Expression<Func<TEntity, TIndexOnSecond>> secondIndexedProperty)
        {
            return FindIndex(firstIndexedProperty, secondIndexedProperty) != null;
        }

        #endregion

        #region Help Methods
        private void AddToIndexes(TEntity entity)
        {
            foreach (var index in _indexes1D)
            {
                index.AddOrUpdate(entity);
            }

            foreach (var index in _indexes2D)
            {
                index.AddOrUpdate(entity);
            }
        }

        private void RemoveFromIndexes(Guid entityId)
        {
            foreach (var index in _indexes1D)
            {
                index.TryRemove(entityId);
            }

            foreach (var index in _indexes2D)
            {
                index.TryRemove(entityId);
            }
        }

        private ICacheIndex1D<TEntity, TIndexOn> FindIndex<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> firstIndexedProperty)
        {
           var index = _indexes1D.FirstOrDefault(x => x.IsOnExpression(firstIndexedProperty));

            if (index == null)
            {
                throw new IndexNotFoundException();    
            }

            return index as ICacheIndex1D<TEntity, TIndexOn>;
        }

        private ICacheIndex2D<TEntity, TIndexOnFirst, TIndexOnSecond> FindIndex<TIndexOnFirst, TIndexOnSecond>(
            Expression<Func<TEntity, TIndexOnFirst>> firstIndexedProperty,
            Expression<Func<TEntity, TIndexOnSecond>> secondIndexedProperty)
        {
            var index = _indexes2D.FirstOrDefault(x => x.IsOnExpression(firstIndexedProperty, secondIndexedProperty));

            if (index == null)
            {
                throw new IndexNotFoundException();
            }

            return index as ICacheIndex2D<TEntity, TIndexOnFirst, TIndexOnSecond>;
        }
        #endregion
    }
}
