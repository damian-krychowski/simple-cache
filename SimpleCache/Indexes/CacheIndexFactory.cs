using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SimpleCache.Builder;

namespace SimpleCache.Indexes
{
    internal class CacheIndexFactory<TEntity>
        where TEntity:IEntity
    {
        public ICacheIndex<TEntity> CreateCacheIndex1D(
            ICacheIndexDefinition cacheIndexDefinition,
            ISimpleCache<TEntity> parentCache)
        {
            MethodInfo method = GetType().GetMethods().First(x => x.IsGenericMethod && x.Name == nameof(CreateCacheIndex1D));
            MethodInfo generic = method.MakeGenericMethod(cacheIndexDefinition.IndexType);
            
            var parameters = new object[]
            {
                cacheIndexDefinition.IndexOn,
                parentCache
            };

            return generic.Invoke(this, parameters) as ICacheIndex<TEntity>;
        }

        public ICacheIndex<TEntity> CreateCacheIndex1D<TIndexOn>(
            Expression<Func<TEntity,TIndexOn>> firstIndexedProperty, 
            ISimpleCache<TEntity> parentCache)
        {
            CacheIndex<TEntity, TIndexOn> index = new CacheIndex<TEntity, TIndexOn>();

            index.Initialize(firstIndexedProperty, parentCache);

            return index;
        }
    }
}
