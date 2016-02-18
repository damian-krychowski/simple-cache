using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SimpleCache.Exceptions;

namespace SimpleCache.Indexes
{
    public interface ICacheIndexQuery<TEntity>
        where TEntity : IEntity
    {
        ICacheIndexQuery<TEntity> WhereUndefined<TIndexOn>(Expression<Func<TEntity, TIndexOn>> indexSelector);

        ICacheIndexQuery<TEntity> Where<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> indexSelector, Func<TIndexOn, bool> valueCondition);

        List<TEntity> ToList();
        IEnumerable<TEntity> AsEnumerable();
        int Count();

        ICacheIndexQuery<TEntity> UseTemporaryIndexes();
    }


    internal class CacheIndexQuery<TEntity> : ICacheIndexQuery<TEntity> where TEntity : IEntity
    {
        private readonly List<IEnumerable<Guid>> _selectedIndexedItems = new List<IEnumerable<Guid>>(); 
        private readonly ISimpleCacheInternal<TEntity> _parentCache;
        private bool _shouldUseTemporaryIndexes = false;

        public CacheIndexQuery(ISimpleCacheInternal<TEntity> parentCache)
        {
            _parentCache = parentCache;
        }

        public ICacheIndexQuery<TEntity> WhereUndefined<TIndexOn>(Expression<Func<TEntity, TIndexOn>> indexSelector)
        {
            if(indexSelector == null ) throw new ArgumentNullException(nameof(indexSelector));

            var items = GetIndexOrTemporary(indexSelector)
                .GetIdsWithUndefined();

            _selectedIndexedItems.Add(items);

            return this;
        }

        public ICacheIndexQuery<TEntity> Where<TIndexOn>(Expression<Func<TEntity, TIndexOn>> indexSelector, Func<TIndexOn, bool> valueCondition)
        {
            if (indexSelector == null) throw new ArgumentNullException(nameof(indexSelector));
            if(valueCondition == null) throw new ArgumentNullException(nameof(valueCondition));

            var index = GetIndexOrTemporary(indexSelector);

            var acceptedKeys = index.Keys
                .Where(valueCondition);

            var items = acceptedKeys
                .SelectMany(key => index.GetIds(key))
                .Distinct();

            _selectedIndexedItems.Add(items);

            return this;
        }

        private ICacheIndex<TEntity, TIndexOn> GetIndexOrTemporary<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> indexSelector)
        {
            if (_parentCache.ContainsIndexOn(indexSelector))
            {
                return _parentCache.Index(indexSelector);
            }

            if (_shouldUseTemporaryIndexes)
            {
                return _parentCache.CreateTemporaryIndex(indexSelector);
            }
             
            throw new IndexNotFoundException(indexSelector.ToString());
        }

        private IEnumerable<Guid> IntersectItemIds()
        {
            if (_selectedIndexedItems.Any())
            {
                return _selectedIndexedItems
                    .Skip(1)
                    .Aggregate(
                        new HashSet<Guid>(_selectedIndexedItems.First()), (intersection, list) =>
                        {
                            intersection.IntersectWith(list);
                            return intersection;
                        }
                    );
            }

            return Enumerable.Empty<Guid>();
        }

        public List<TEntity> ToList()
        {
            return IntersectItemIds()
                .Select(entityId => _parentCache.GetEntity(entityId))
                .ToList();
        }

        public IEnumerable<TEntity> AsEnumerable()
        {
            foreach (var entityId in IntersectItemIds())
            {
                yield return _parentCache.GetEntity(entityId);
            }
        }

        public int Count()
        {
            return IntersectItemIds().Count();
        }

        public ICacheIndexQuery<TEntity> UseTemporaryIndexes()
        {
            _shouldUseTemporaryIndexes = true;
            return this;
        }
    }
}
