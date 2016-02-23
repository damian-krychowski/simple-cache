using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCache.Indexes.Memory
{
    internal class SortedIndexMemory<TEntity, TIndexOn, TOrderBy> : IndexMemory<TEntity, TIndexOn>
        where TEntity : IEntity
        where TOrderBy : IComparable<TOrderBy>
    {
        private readonly Func<TEntity, TOrderBy> _orderBySelector;
        private readonly IComparer<TEntity> _entitiesComparer;

        public SortedIndexMemory(
            Func<TEntity, TOrderBy> orderBySelector,
            IComparer<TEntity> entitiesComparer)
        {
            _orderBySelector = orderBySelector;
            _entitiesComparer = entitiesComparer;
        }

        public static SortedIndexMemory<TEntity, TIndexOn, TOrderBy> CreateAscending(
            Func<TEntity, TOrderBy> orderBySelector)
        {
            return new SortedIndexMemory<TEntity, TIndexOn, TOrderBy>(
                orderBySelector,
                new AscendingFuncComparer<TEntity,TOrderBy>(orderBySelector));
        }

        public static SortedIndexMemory<TEntity, TIndexOn, TOrderBy> CreateDescending(
            Func<TEntity, TOrderBy> orderBySelector)
        {
            return new SortedIndexMemory<TEntity, TIndexOn, TOrderBy>(
                orderBySelector,
                new DescendingFuncComparer<TEntity,TOrderBy>(orderBySelector));
        }

        public override void Insert(TEntity entity, TIndexOn key)
        {
            using (MemoryLock.ModifyMemory())
            {
                var indexList = GetIndexList(key);
                AddSorted(indexList, entity);
                IndexationList.MarkIndexation(entity.Id, indexList);
            }
        }

        public override void InsertWithUndefinedKey(TEntity entity)
        {
            using (MemoryLock.ModifyMemory())
            {
                AddSorted(EntitiesWithUndefinedKey, entity);
                IndexationList.MarkIndexation(entity.Id, EntitiesWithUndefinedKey);
            }
        }


        public void AddSorted(List<TEntity> entities, TEntity item)
        {
            if (entities.Count == 0)
            {
                entities.Add(item);
                return;
            }

            var index = entities.BinarySearch(item, _entitiesComparer);

            if (index < 0)
            {
                index = ~index;
            }
               
            entities.Insert(index, item);
        }

    }
}