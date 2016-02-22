using System;
using System.Threading;

namespace SimpleCache.Indexes.Memory
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
}