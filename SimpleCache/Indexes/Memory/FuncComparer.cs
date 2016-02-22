using System;
using System.Collections.Generic;

namespace SimpleCache.Indexes.Memory
{
    class FuncComparer<T, TOrderBy> : IComparer<T>
        where TOrderBy : IComparable<TOrderBy>
    {
        private readonly Func<T, TOrderBy> _orderBySelector;

        public FuncComparer(Func<T, TOrderBy> orderBySelector)
        {
            _orderBySelector = orderBySelector;
        }

        public int Compare(T x, T y)
        {
            var xCompare = _orderBySelector(x);
            var yCompare = _orderBySelector(y);

            return xCompare.CompareTo(yCompare);
        }
    }
}
