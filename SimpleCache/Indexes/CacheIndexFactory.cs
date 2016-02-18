using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SimpleCache.Builder;
using SimpleCache.Indexes.OneDimensional;
using SimpleCache.Indexes.TwoDimensional;

namespace SimpleCache.Indexes
{
    internal class CacheIndexFactory<TEntity>
        where TEntity:IEntity
    {
        public ICacheIndex1D<TEntity> CreateCacheIndex1D(
            ICacheIndexDefinition1D cacheIndexDefinition,
            ISimpleCache<TEntity> parentCache)
        {
            MethodInfo method = GetType().GetMethods().First(x => x.IsGenericMethod && x.Name == nameof(CreateCacheIndex1D));
            MethodInfo generic = method.MakeGenericMethod(cacheIndexDefinition.FirstIndexOn);
            
            var parameters = new object[]
            {
                cacheIndexDefinition.IndexOnProperty,
                parentCache
            };

            return generic.Invoke(this, parameters) as ICacheIndex1D<TEntity>;
        }

        public ICacheIndex1D<TEntity> CreateCacheIndex1D<TIndexOn>(
            Expression<Func<TEntity,TIndexOn>> firstIndexedProperty, 
            ISimpleCache<TEntity> parentCache)
        {
            CacheIndex1D<TEntity, TIndexOn> index = new CacheIndex1D<TEntity, TIndexOn>();

            index.Initialize(firstIndexedProperty, parentCache);

            return index;
        }

        public ICacheIndex2D<TEntity> CreateCacheIndex2D(
           ICacheIndexDefinition2D cacheIndexDefinition,
           ISimpleCache<TEntity> parentCache)
        {
            MethodInfo method = GetType().GetMethods().First(x => x.IsGenericMethod && x.Name == nameof(CreateCacheIndex2D));
            MethodInfo generic = method.MakeGenericMethod(cacheIndexDefinition.FirstIndexOn,cacheIndexDefinition.SecondIndexOn);

            var parameters = new object[]
            {
                cacheIndexDefinition.IndexOnFirstProperty,
                cacheIndexDefinition.IndexOnSecondProperty,
                parentCache
            };

            return generic.Invoke(this, parameters) as ICacheIndex2D<TEntity>;
        }

        public ICacheIndex2D<TEntity> CreateCacheIndex2D<TIndexOnFirst, TIndexOnSecond>(
            Expression<Func<TEntity, TIndexOnFirst>> firstIndexedProperty,
            Expression<Func<TEntity, TIndexOnSecond>> secondIndexedProperty,
            ISimpleCache<TEntity> parentCache)
        {
            CacheIndex2D<TEntity, TIndexOnFirst,TIndexOnSecond> index = new CacheIndex2D<TEntity, TIndexOnFirst, TIndexOnSecond>();

            index.Initialize(firstIndexedProperty, secondIndexedProperty, parentCache);

            return index;
        }
    }
}
