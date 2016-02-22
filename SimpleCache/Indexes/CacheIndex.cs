using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SimpleCache.ExtensionMethods;
using SimpleCache.Indexes.Memory;

namespace SimpleCache.Indexes
{
    internal class CacheIndex<TEntity, TIndexOn> : 
        ICacheIndex<TEntity, TIndexOn>, 
        ICacheIndex<TEntity>
        where TEntity : IEntity
    {
        private IIndexMemory<TEntity, TIndexOn> _memory;

        private Func<TEntity, TIndexOn> _indexFunc;
        private ISimpleCache<TEntity> _parentCache;

        public CacheIndex(IIndexMemory<TEntity, TIndexOn> memory)
        {
            _memory = memory;
        }

        public bool IsOnExpression(Expression indexExpression) => indexExpression.Comapre(IndexExpression);

        public void AddOrUpdate(TEntity entity)
        {
            var indexKey = IndexKey<TEntity, TIndexOn>.Determine(_indexFunc, entity);

            _memory.RemoveIfStored(entity.Id);

            if (indexKey.Defined)
            {
                _memory.Insert(entity, indexKey.Value);
            }
            else
            {
                _memory.InsertWithUndefinedKey(entity);
            }
        }

        public void TryRemove(Guid entityId) => _memory.RemoveIfStored(entityId);

        public void Rebuild()
        {
            _memory = new IndexMemory<TEntity, TIndexOn>();

            foreach (var entity in _parentCache.Items)
            {
                AddOrUpdate(entity);
            }
        }

        public List<TEntity> GetWithUndefined() => _memory.IndexedWithUndefinedKey();

        public void Initialize(
            Expression<Func<TEntity, TIndexOn>> firstIndexedProperty, 
            ISimpleCache<TEntity> parentCache)
        {
            _indexFunc = firstIndexedProperty.Compile();
            IndexExpression = firstIndexedProperty;
            _parentCache = parentCache;
        }

        public Expression<Func<TEntity, TIndexOn>> IndexExpression
        {
            get;
            private set;
        }

        public IEnumerable<TIndexOn> Keys => _memory.Keys();

        public List<TEntity> Get(TIndexOn key)
        {
            if(key == null) throw  new ArgumentNullException(nameof(key));
            return _memory.IndexedWithKey(key);
        }
    }
}
