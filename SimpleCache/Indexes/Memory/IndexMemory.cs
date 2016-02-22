using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SimpleCache.Indexes.Memory
{
    internal class IndexMemory<TEntity,TIndexOn> : IIndexMemory<TEntity, TIndexOn>
        where TEntity : IEntity
    {
        protected readonly IndexationList<TEntity> IndexationList = new IndexationList<TEntity>();
        protected readonly List<TEntity> EntitiesWithUndefinedKey = new List<TEntity>();
        protected readonly Dictionary<TIndexOn, List<TEntity>> Index = new Dictionary<TIndexOn, List<TEntity>>();
        protected readonly ReaderWriterLockSlim MemoryLock = new ReaderWriterLockSlim();

        public List<TEntity> IndexedWithUndefinedKey()
        {
            using (MemoryLock.ReadMemory())
            {
                return EntitiesWithUndefinedKey.ToList();
            }
        }

        public List<TIndexOn> Keys()
        {
            using (MemoryLock.ReadMemory())
            {
                return Index.Keys.ToList();
            }
        }

        public List<TEntity> IndexedWithKey(TIndexOn key)
        {
            using (MemoryLock.ReadMemory())
            {
                List<TEntity> list;

                if (Index.TryGetValue(key, out list))
                {
                    return list;
                }

                return new List<TEntity>();
            }
        }
        
        public virtual void InsertWithUndefinedKey(TEntity entity)
        {
            using (MemoryLock.ModifyMemory())
            {
                EntitiesWithUndefinedKey.Add(entity);
                IndexationList.MarkIndexation(entity.Id, EntitiesWithUndefinedKey);
            }
        }
        
        public virtual void Insert(TEntity entity, TIndexOn key)
        {
            using (MemoryLock.ModifyMemory())
            {
                var indexList = GetIndexList(key);
                indexList.Add(entity);
                IndexationList.MarkIndexation(entity.Id, indexList);
            }
        }

        protected List<TEntity> GetIndexList(TIndexOn key)
        {
            List<TEntity> list;

            if (Index.TryGetValue(key, out list))
            {
                return list;
            }

            list = new List<TEntity>();

            Index.Add(key, list);

            return list;
        }
        
        public void RemoveIfStored(Guid id)
        {
            using (MemoryLock.ModifyMemory())
            {
                IndexationList.RemoveFromLookupAndMemory(id);
            }
        }
    }
}