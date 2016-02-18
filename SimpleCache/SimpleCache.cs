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
    internal class SimpleCache<TEntity> : ISimpleCacheInternal<TEntity>
        where TEntity : IEntity
    {
        readonly ConcurrentDictionary<Guid, TEntity> _items = new ConcurrentDictionary<Guid, TEntity>();

        readonly List<ICacheIndex<TEntity>> _indexes = new List<ICacheIndex<TEntity>>();

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

        public ICacheIndex<TEntity, TIndexOn> Index<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> indexExpression)
        {
            if(indexExpression == null) throw new ArgumentNullException(nameof(indexExpression));

            ICacheIndex<TEntity, TIndexOn> index = FindIndex(indexExpression);

            if (index == null) throw new IndexNotFoundException($"Index {indexExpression} was not registered!");

            return index;
        }

        public ICacheIndexQuery<TEntity> Query()
        {
            return new CacheIndexQuery<TEntity>(this);
        }

        public bool ContainsIndexOn<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> indexExpression)
        {
            if(indexExpression == null) throw  new ArgumentNullException(nameof(indexExpression));

            return _indexes.Any(x => x.IsOnExpression(indexExpression));
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

        public ICacheIndex<TEntity, TIndexOn> CreateTemporaryIndex<TIndexOn>
            (Expression<Func<TEntity, TIndexOn>> indexExpression)
        {
            var temporaryIndex = new CacheIndex<TEntity,TIndexOn>();
            temporaryIndex.Initialize(indexExpression, this);
            
            foreach (var entity in _items.Values)
            {
                temporaryIndex.AddOrUpdate(entity);
            }

            return temporaryIndex;
        }
    }
}
