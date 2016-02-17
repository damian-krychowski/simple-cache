using System;
using System.Linq.Expressions;

namespace SimpleCache.Builder
{
    public interface ICacheBuilder<TEntity> 
        where TEntity : IEntity
    {
        ICacheBuilder<TEntity> WithIndex1D<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> indexFisrtDimension);

        ICacheBuilder<TEntity> WithIndex2D<TIndexOnFirst, TInxedOnSecond>(
            Expression<Func<TEntity, TIndexOnFirst>> indexFirstDimentsion, 
            Expression<Func<TEntity, TInxedOnSecond>> indexSecondDimension);  
     
        ISimpleCache<TEntity> BuildUp();
    }
}
