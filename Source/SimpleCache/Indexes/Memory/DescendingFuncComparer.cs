using System;

namespace SimpleCache.Indexes.Memory
{
    internal class DescendingFuncComparer<T, TOrderBy> : AscendingFuncComparer<T, TOrderBy>
        where TOrderBy : IComparable<TOrderBy>
    {
        public DescendingFuncComparer(Func<T, TOrderBy> orderBySelector) :
            base(orderBySelector)
        {

        }

        public override int Compare(T x, T y)
        {
            return -base.Compare(x, y);
        }
    }
}
