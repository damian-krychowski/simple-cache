using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SimpleCache.Exceptions;

namespace SimpleCache.Indexes
{
    internal class CacheIndexQuery<TEntity> : ICacheIndexQuery<TEntity> where TEntity : IEntity
    {
        private HashSet<TEntity> _hashedEntities;
        private readonly ISimpleCache<TEntity> _parentCache;

        public CacheIndexQuery(ISimpleCache<TEntity> parentCache)
        {
            _parentCache = parentCache;
        }

        public ICacheIndexQuery<TEntity> WhereUndefined<TIndexOn>(Expression<Func<TEntity, TIndexOn>> indexExpression)
        {
            var entities = _parentCache
                .Index(indexExpression)
                .GetWithUndefined();

            Hash(entities);

            return this;
        }

        public ICacheIndexQuery<TEntity> Where<TIndexOn>(Expression<Func<TEntity, TIndexOn>> indexExpression, Func<TIndexOn, bool> valueCondition)
        {
            if (valueCondition == null) throw new ArgumentNullException(nameof(valueCondition));

            var index = _parentCache.Index(indexExpression);

            var acceptedKeys = index.Keys
                .Where(valueCondition);

            var entities = acceptedKeys
                .SelectMany(key => index.Get(key));

            Hash(entities);

            return this;
        }

        public void Hash(IEnumerable<TEntity> entities)
        {
            if (_hashedEntities == null)
            {
                _hashedEntities = new HashSet<TEntity>(entities);
            }
            else
            {
                _hashedEntities.IntersectWith(entities);
            }
        }

        public List<TEntity> ToList()
        {
            return _hashedEntities.ToList();
        }

        public int Count()
        {
            return _hashedEntities.Count;
        }
    }
}