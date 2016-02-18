using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SimpleCache.Builder
{
    internal class CacheBuilder<TEntity> : ICacheBuilder<TEntity>
        where TEntity : IEntity
    {
        #region Globals
        readonly List<ICacheIndexDefinition1D> _index1DDefinitions = new List<ICacheIndexDefinition1D>();
        readonly List<ICacheIndexDefinition2D> _index2DDefinitions = new List<ICacheIndexDefinition2D>();
        #endregion

        #region Interface
        public void AddIndexDefinition<TIndexOn>(CacheIndex1DDefinition<TEntity, TIndexOn> definition)
        {
            _index1DDefinitions.Add(definition);
        }

        public void AddIndexDefinition<TIndexOnFisrt, TIndexOnSecond>(CacheIndex2DDefinition<TEntity, TIndexOnFisrt, TIndexOnSecond> definition)
        {
            _index2DDefinitions.Add(definition);
        }
        #endregion

        #region ICacheBuilder

        public ICacheBuilder<TEntity> WithIndex1D<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> indexFisrtDimension)
        {
            AddIndexDefinition(new CacheIndex1DDefinition<TEntity, TIndexOn>()
            {
                IndexFirstDimension = indexFisrtDimension,
            });

            return this;
        }

        public ICacheBuilder<TEntity> WithIndex2D<TIndexOnFirst, TIndexOnSecond>(
            Expression<Func<TEntity, TIndexOnFirst>> indexFirstDimension,
            Expression<Func<TEntity, TIndexOnSecond>> indexSecondDimension)
        {
            AddIndexDefinition(new CacheIndex2DDefinition<TEntity, TIndexOnFirst, TIndexOnSecond>()
            {
                IndexFirstDimension = indexFirstDimension,
                IndexSecondDimension = indexSecondDimension,
            });

            return this;
        }

        ISimpleCache<TEntity> ICacheBuilder<TEntity>.BuildUp()
        {
            SimpleCache<TEntity> cache = new SimpleCache<TEntity>();

            cache.Initialize(new CacheDefinition()
            {
                Indexes1D = _index1DDefinitions,
                Indexes2D = _index2DDefinitions,
            });

            return cache;
        }

        public ISimpleCache<TEntity> BuildUp(IEnumerable<TEntity> entities)
        {
            SimpleCache<TEntity> cache = new SimpleCache<TEntity>();

            cache.Initialize(new CacheDefinition()
            {
                Indexes1D = _index1DDefinitions,
                Indexes2D = _index2DDefinitions,
            });

            cache.AddOrUpdateRange(entities);

            return cache;
        }

        #endregion
    }
}
