using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCache.Indexes.Memory.Factory
{
    internal class AscendingSortedIndexMemoryFactory<TEntity, TIndexOn, TOrderBy> : IIndexMemoryFactory<TEntity, TIndexOn>
        where TEntity : IEntity
        where TOrderBy : IComparable<TOrderBy>
    {
        private readonly Func<TEntity, TOrderBy> _orderBySelector;

        public AscendingSortedIndexMemoryFactory(Func<TEntity, TOrderBy> orderBySelector)
        {
            _orderBySelector = orderBySelector;
        }

        public IIndexMemory<TEntity, TIndexOn> Create()
        {
            return SortedIndexMemory<TEntity, TIndexOn, TOrderBy>.CreateAscending(_orderBySelector);
        }
    }
}
