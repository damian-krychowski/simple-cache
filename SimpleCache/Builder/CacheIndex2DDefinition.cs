using System;
using System.Linq.Expressions;

namespace SimpleCache.Builder
{
    public class CacheIndex2DDefinition<TEntity, TIndexOnFirst, TIndexOnSecond> : ICacheIndexDefinition2D
        where TEntity : IEntity
    {
        public Expression<Func<TEntity, TIndexOnFirst>> IndexFirstDimension { get; set; }
        public Expression<Func<TEntity, TIndexOnSecond>> IndexSecondDimension { get; set; }

        public Type FirstIndexOn => typeof (TIndexOnFirst);

        public Type SecondIndexOn => typeof (TIndexOnSecond);

        public Expression IndexOnFirstProperty => IndexFirstDimension;

        public Expression IndexOnSecondProperty => IndexSecondDimension;
    }
}
