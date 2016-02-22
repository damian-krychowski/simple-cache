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
        private readonly FuncComparer<TEntity, TOrderBy> _entitiesComparer;

        public SortedIndexMemory(Func<TEntity, TOrderBy> orderBySelector)
        {
            _orderBySelector = orderBySelector;
            _entitiesComparer = new FuncComparer<TEntity, TOrderBy>(orderBySelector);
        }

        public override void Insert(TEntity entity, TIndexOn key)
        {
            using (MemoryLock.ModifyMemory())
            {
                var indexList = GetIndexList(key);
                AddSorted(indexList, entity);
                IndexationList.MarkIndexation(entity.Id, EntitiesWithUndefinedKey);
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
            if (_orderBySelector(entities.Last()).CompareTo(_orderBySelector(item)) <= 0)
            {
                entities.Add(item);
                return;
            }
            if (_orderBySelector(entities.First()).CompareTo(_orderBySelector(item)) >= 0)
            {
                entities.Insert(0, item);
                return;
            }

            int index = entities.BinarySearch(item, _entitiesComparer);

            if (index < 0)
            {
                index = ~index;
            }
               
            entities.Insert(index, item);
        }

    }
}