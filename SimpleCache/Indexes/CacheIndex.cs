using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SimpleCache.ExtensionMethods;

namespace SimpleCache.Indexes
{
    internal class CacheIndex<TEntity, TIndexOn> : 
        ICacheIndex<TEntity, TIndexOn>, 
        ICacheIndex<TEntity>
        where TEntity : IEntity
    {
        private readonly IndexMemory<TEntity, TIndexOn> _memory = new IndexMemory<TEntity, TIndexOn>();

        private Func<TEntity, TIndexOn> _indexFunc;
        private ISimpleCache<TEntity> _parentCache;

        public bool IsOnExpression(Expression indexExpression)
        {
            return indexExpression.Comapre(IndexExpression);
        }

        public void AddOrUpdate(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var indexKey = _indexFunc(entity);

            _memory.RemoveIfStored(entity.Id);

            if (indexKey == null)
            {
                _memory.InsertWithUndefinedKey(entity);
            }
            else
            {
                _memory.Insert(entity, indexKey);
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

        public IEnumerable<TEntity> GetWithUndefined() => _memory.IndexedWithUndefinedKey;

        public void Initialize(
            Expression<Func<TEntity, TIndexOn>> firstIndexedProperty, 
            ISimpleCache<TEntity> parentCache)
        {
            if (firstIndexedProperty == null) throw new ArgumentNullException(nameof(firstIndexedProperty));
            if (parentCache == null) throw new ArgumentNullException(nameof(parentCache));

            _indexFunc = firstIndexedProperty.Compile();
            IndexExpression = firstIndexedProperty;
            _parentCache = parentCache;
        }

        public void BuildUp(IEnumerable<TEntity> entities)
        {
            foreach(var entity in entities)
            {
                AddOrUpdate(entity);
            }
        }

        public Expression<Func<TEntity, TIndexOn>> IndexExpression
        {
            get;
            private set;
        }

        public IEnumerable<TIndexOn> Keys => _memory.Keys;

        public IEnumerable<TEntity> Get(TIndexOn key)
        {
            if(key == null) throw  new ArgumentNullException(nameof(key));
            return _memory.IndexedWithKey(key);
        }
    }
}
