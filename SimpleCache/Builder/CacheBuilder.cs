using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SimpleCache.Indexes;
using SimpleCache.Indexes.Memory;
using SimpleCache.Indexes.Memory.Factory;

namespace SimpleCache.Builder
{
    internal class CacheBuilder<TEntity> : ICacheBuilder<TEntity>
        where TEntity : IEntity
    {
        private readonly List<Func<ISimpleCache<TEntity>, ICacheIndex<TEntity>>> _indexFactories = new List<Func<ISimpleCache<TEntity>, ICacheIndex<TEntity>>>();

        public ICacheBuilder<TEntity> WithIndex<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> indexExpression)
        {
            _indexFactories.Add(parentCache =>
                CreateIndex(indexExpression, parentCache));

            return this;
        }

        private static CacheIndex<TEntity, TIndexOn> CreateIndex<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> indexExpression, 
            ISimpleCache<TEntity> parentCache)
        {
            return new CacheIndex<TEntity, TIndexOn>(
                new IndexMemoryFactory<TEntity,TIndexOn>(), 
                indexExpression,
                parentCache);
        }

        public ISortedIndexBuilder<TEntity> WithSortedIndex<TIndexOn, TOrderBy>(
            Expression<Func<TEntity, TIndexOn>> indexExpression, 
            Func<TEntity, TOrderBy> orderBySelector) where TOrderBy : IComparable<TOrderBy>
        {
            var sortedIndexBuilder = new SortedIndexBuilder<TEntity>(this);

            _indexFactories.Add(parentCache => sortedIndexBuilder.IsAscending
                ? CreateAscendingIndex(indexExpression, orderBySelector, parentCache)
                : CreateDescendingIndex(indexExpression, orderBySelector, parentCache));

            return sortedIndexBuilder;
        }

        private static ICacheIndex<TEntity> CreateDescendingIndex<TIndexOn, TOrderBy>(
            Expression<Func<TEntity, TIndexOn>> indexExpression, 
            Func<TEntity, TOrderBy> orderBySelector, 
            ISimpleCache<TEntity> parentCache)
            where TOrderBy : IComparable<TOrderBy>
        {
            return new CacheIndex<TEntity, TIndexOn>(
                new DescendingSortedIndexMemoryFactory<TEntity,TIndexOn,TOrderBy>(orderBySelector),  
                indexExpression,
                parentCache);
        }

        private static ICacheIndex<TEntity> CreateAscendingIndex<TIndexOn, TOrderBy>(
            Expression<Func<TEntity, TIndexOn>> indexExpression,
            Func<TEntity, TOrderBy> orderBySelector, 
            ISimpleCache<TEntity> parentCache) 
            where TOrderBy : IComparable<TOrderBy>
        {
            return new CacheIndex<TEntity, TIndexOn>(
                new AscendingSortedIndexMemoryFactory<TEntity,TIndexOn,TOrderBy>(orderBySelector), 
                indexExpression,
                parentCache);
        }

        ISimpleCache<TEntity> ICacheBuilder<TEntity>.BuildUp()
        {
            var cache = new SimpleCache<TEntity>(_indexFactories);
            return cache;
        }

        public ISimpleCache<TEntity> BuildUp(IEnumerable<TEntity> entities)
        {
            var cache = new SimpleCache<TEntity>(_indexFactories);
            cache.AddOrUpdateRange(entities);

            return cache;
        }
    }
}
