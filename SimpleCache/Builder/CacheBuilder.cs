using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SimpleCache.Builder
{
    internal class CacheBuilder<TEntity> : ICacheBuilder<TEntity>
        where TEntity : IEntity
    {
        #region Globals
        readonly List<ICacheIndexDefinition> _indexesDefinitions = new List<ICacheIndexDefinition>();
        #endregion

        #region Interface
        public void AddIndexDefinition<TIndexOn>(CacheIndexDefinition<TEntity, TIndexOn> definition)
        {
            _indexesDefinitions.Add(definition);
        }
        #endregion

        #region ICacheBuilder

        public ICacheBuilder<TEntity> WithIndex<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> indexExpression)
        {
            AddIndexDefinition(new CacheIndexDefinition<TEntity, TIndexOn>()
            {
                IndexExpression = indexExpression,
            });

            return this;
        }

        ISimpleCache<TEntity> ICacheBuilder<TEntity>.BuildUp()
        {
            SimpleCache<TEntity> cache = new SimpleCache<TEntity>();

            cache.Initialize(new CacheDefinition()
            {
                Indexes = _indexesDefinitions,
            });

            return cache;
        }

        public ISimpleCache<TEntity> BuildUp(IEnumerable<TEntity> entities)
        {
            SimpleCache<TEntity> cache = new SimpleCache<TEntity>();

            cache.Initialize(new CacheDefinition()
            {
                Indexes = _indexesDefinitions,
            });

            cache.AddOrUpdateRange(entities);

            return cache;
        }

        #endregion
    }
}
