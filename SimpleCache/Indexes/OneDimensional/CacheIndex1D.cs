﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SimpleCache.ExtensionMethods;

namespace SimpleCache.Indexes.OneDimensional
{
    internal class CacheIndex1D<TEntity, TIndexOn> : 
        ICacheIndex1D<TEntity, TIndexOn>, 
        ICacheIndex1D<TEntity>
        where TEntity : IEntity
    {
        private readonly Index1DMemory<TIndexOn> _memory = new Index1DMemory<TIndexOn>();

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

            _memory.RemoveIfStored(entity.Id);

            if (indexKey == null)
            {
                _memory.InsertWithUndefinedKey(entity.Id);
            }
            else
            {
                _memory.Insert(entity.Id, indexKey);
            }
        }

        public void TryRemove(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            TryRemove(entity.Id);
        }

        public void TryRemove(Guid entityId)
        {
            _memory.RemoveIfStored(entityId);
        }

        public void Rebuild()
        {
            Clear();

            foreach (var entity in _parentCache.Items)
            {
                AddOrUpdate(entity);
            }
        }

        public void Clear() => _memory.Clear();

        public IEnumerable<TEntity> GetWithUndefined()
        {
            foreach (var entityId in _memory.IndexedWithUndefinedKey)
            {
                yield return _parentCache.GetEntity(entityId);
            }
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

            foreach (var entityId in _memory.IndexedWithKey(firstIndexedId))
            {
                yield return _parentCache.GetEntity(entityId);
            }
        }
    }
}
