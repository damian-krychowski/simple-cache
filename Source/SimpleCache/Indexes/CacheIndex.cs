using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SimpleCache.ExtensionMethods;
using SimpleCache.Indexes.Memory;
using SimpleCache.Indexes.Memory.Factory;

namespace SimpleCache.Indexes
{
    internal class CacheIndex<TEntity, TIndexOn> : 
        ICacheIndex<TEntity, TIndexOn>, 
        ICacheIndex<TEntity>
        where TEntity : IEntity
    {
        private IIndexMemory<TEntity, TIndexOn> _memory;

        private readonly Func<TEntity, TIndexOn> _indexFunc;
        private readonly IIndexMemoryFactory<TEntity, TIndexOn> _memoryFactory;
        private readonly ISimpleCache<TEntity> _parentCache;

        public CacheIndex(
            IIndexMemoryFactory<TEntity, TIndexOn> memoryFactory,
            Expression<Func<TEntity, TIndexOn>> indexExpression,
            ISimpleCache<TEntity> parentCache)
        {
            _memory = memoryFactory.Create();

            _indexFunc = indexExpression.Compile();
            IndexExpression = indexExpression;
            _memoryFactory = memoryFactory;
            _parentCache = parentCache;
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
            _memory = _memoryFactory.Create();

            foreach (var entity in _parentCache.Items)
            {
                AddOrUpdate(entity);
            }
        }

        public List<TEntity> GetWithUndefined() => _memory.IndexedWithUndefinedKey();

        public Expression<Func<TEntity, TIndexOn>> IndexExpression { get; }

        public IEnumerable<TIndexOn> Keys => _memory.Keys();

        public List<TEntity> Get(TIndexOn key)
        {
            if(key == null) throw  new ArgumentNullException(nameof(key));
            return _memory.IndexedWithKey(key);
        }
    }
}
