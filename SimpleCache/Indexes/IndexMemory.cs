using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;

namespace SimpleCache.Indexes
{
    public static class MemoryLockExtensions
    {
        private sealed class LockForReading : IDisposable
        {
            private readonly ReaderWriterLockSlim _memoryLock;
            public LockForReading(ReaderWriterLockSlim sync)
            {
                _memoryLock = sync;
                sync.EnterReadLock();
            }
            public void Dispose()
            {
                _memoryLock.ExitReadLock();
            }
        }

        private sealed class LockForWriting : IDisposable
        {
            private readonly ReaderWriterLockSlim _memoryLock;
            public LockForWriting(ReaderWriterLockSlim sync)
            {
                _memoryLock = sync;
                sync.EnterWriteLock();
            }
            public void Dispose()
            {
                _memoryLock.ExitWriteLock();
            }
        }

        public static IDisposable ReadMemory(this ReaderWriterLockSlim memoryLock)
        {
            return new LockForReading(memoryLock);
        }
        public static IDisposable ModifyMemory(this ReaderWriterLockSlim memoryLock)
        {
            return new LockForWriting(memoryLock);
        }
    }

    internal class IndexMemory<TEntity,TIndexOn>
        where TEntity : IEntity
    {
        private readonly IndexationList<TEntity> _indexationList = new IndexationList<TEntity>();
        private readonly List<TEntity> _entitiesWithUndefinedKey = new List<TEntity>();
        private readonly Dictionary<TIndexOn, List<TEntity>> _index = new Dictionary<TIndexOn, List<TEntity>>();
        private readonly ReaderWriterLockSlim _memoryLock = new ReaderWriterLockSlim();

        public List<TEntity> IndexedWithUndefinedKey()
        {
            using (_memoryLock.ReadMemory())
            {
                return _entitiesWithUndefinedKey.ToList();
            }
        }

        public List<TIndexOn> Keys()
        {
            using (_memoryLock.ReadMemory())
            {
                return _index.Keys.ToList();
            }
        }

        public List<TEntity> IndexedWithKey(TIndexOn key)
        {
            using (_memoryLock.ReadMemory())
            {
                List<TEntity> list;

                if (_index.TryGetValue(key, out list))
                {
                    return list;
                }

                return new List<TEntity>();
            }
        }
        
        public void InsertWithUndefinedKey(TEntity entity)
        {
            using (_memoryLock.ModifyMemory())
            {
                _indexationList.MarkIndexation(entity, _entitiesWithUndefinedKey);
            }
        }
        
        public void Insert(TEntity entity, TIndexOn key)
        {
            using (_memoryLock.ModifyMemory())
            {
                var indexList = GetIndexList(key);
                _indexationList.MarkIndexation(entity, indexList);
            }
        }

        private List<TEntity> GetIndexList(TIndexOn key)
        {
            List<TEntity> list;

            if (_index.TryGetValue(key, out list))
            {
                return list;
            }

            list = new List<TEntity>();

            _index.Add(key, list);

            return list;
        }
        
        public void RemoveIfStored(Guid id)
        {
            using (_memoryLock.ModifyMemory())
            {
                _indexationList.RemoveFromLookupAndMemory(id);
            }
        }

        public void Clear()
        {
            using (_memoryLock.ModifyMemory())
            {
                _indexationList.Clear();
                _index.Clear();
                _entitiesWithUndefinedKey.Clear();
            }
        }
    }
}