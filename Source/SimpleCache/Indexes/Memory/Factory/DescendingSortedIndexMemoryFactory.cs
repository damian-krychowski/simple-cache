﻿using System;

namespace SimpleCache.Indexes.Memory.Factory
{
    internal class DescendingSortedIndexMemoryFactory<TEntity, TIndexOn, TOrderBy> : IIndexMemoryFactory<TEntity, TIndexOn>
        where TEntity : IEntity
        where TOrderBy : IComparable<TOrderBy>
    {
        private readonly Func<TEntity, TOrderBy> _orderBySelector;

        public DescendingSortedIndexMemoryFactory(Func<TEntity, TOrderBy> orderBySelector)
        {
            _orderBySelector = orderBySelector;
        }

        public IIndexMemory<TEntity, TIndexOn> Create()
        {
            return new SortedIndexMemory<TEntity, TIndexOn, TOrderBy>(
                new DescendingFuncComparer<TEntity, TOrderBy>(_orderBySelector));
        }
    }
}