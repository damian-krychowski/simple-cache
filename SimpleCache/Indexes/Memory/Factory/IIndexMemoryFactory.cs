using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCache.Indexes.Memory.Factory
{
    internal interface IIndexMemoryFactory<TEntity, TIndexOn>
        where TEntity : IEntity
    {
        IIndexMemory<TEntity, TIndexOn> Create();
    }
}
