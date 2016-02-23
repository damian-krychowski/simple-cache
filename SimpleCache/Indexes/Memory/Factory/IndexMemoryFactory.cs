using System;

namespace SimpleCache.Indexes.Memory.Factory
{
    internal class IndexMemoryFactory<TEntity, TIndexOn> : IIndexMemoryFactory<TEntity, TIndexOn>
        where TEntity : IEntity
    {
        public IIndexMemory<TEntity, TIndexOn> Create()
        {
            return new IndexMemory<TEntity, TIndexOn>();
        }
    }
}