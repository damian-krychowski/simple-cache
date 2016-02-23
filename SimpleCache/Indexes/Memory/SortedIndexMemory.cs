using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCache.Indexes.Memory
{
    internal class SortedIndexMemory<TEntity, TIndexOn, TOrderBy> : IndexMemory<TEntity, TIndexOn>
        where TEntity : IEntity
        where TOrderBy : IComparable<TOrderBy>
    {
        private readonly IComparer<TEntity> _entitiesComparer;

        public SortedIndexMemory(IComparer<TEntity> entitiesComparer)
        {
            _entitiesComparer = entitiesComparer;
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