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

namespace SimpleCache
{
    internal class SimpleCache<TEntity> : ISimpleCache<TEntity>
        where TEntity : IEntity
    {
       

        private readonly Dictionary<Guid, TEntity> _items = new Dictionary<Guid, TEntity>();
        private readonly List<ICacheIndex<TEntity>> _indexes = new List<ICacheIndex<TEntity>>();

        public void Initialize(CacheDefinition cacheDefinition)
        {
            var cacheIndexFactory = new CacheIndexFactory<TEntity>();

            foreach (var definition in cacheDefinition.Indexes)
            {
                var index = cacheIndexFactory.CreateCacheIndex1D(definition, this);
                _indexes.Add(index);
            }
        }

        public TEntity GetEntity(Guid id) => _items[id];

        public bool TryGetEntity(Guid id, out TEntity entity) => _items.TryGetValue(id, out entity);

        public bool ContainsEntity(Guid id) => _items.ContainsKey(id);

        public IEnumerable<TEntity> Items => _items.Values;

        public ICacheIndex<TEntity, TIndexOn> Index<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> indexExpression)
        {
            if(indexExpression == null) throw new ArgumentNullException(nameof(indexExpression));

            return FindIndex(indexExpression);
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

            _items[entity.Id] = entity;
            AddOrUpdateIndexes(entity);
        }

        public void AddOrUpdateRange(IEnumerable<TEntity> entities)
        {
            if(entities == null) throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
            {
                AddOrUpdate(entity);
            }
        }

        public void Remove(Guid id)
        {
            _items.Remove(id);
            RemoveFromIndexes(id);
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

        private void AddOrUpdateIndexes(TEntity entity)
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
            Expression<Func<TEntity, TIndexOn>> indexExpression)
        {
           var index = _indexes.FirstOrDefault(x => x.IsOnExpression(indexExpression));

            if (index == null)
            {
                throw new IndexNotFoundException();    
            }

            return index as ICacheIndex<TEntity, TIndexOn>;
        }
    }
}
