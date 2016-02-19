using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCache.Indexes
{
    public interface ICacheIndexQuery<TEntity>
        where TEntity : IEntity
    {
        ICacheIndexQuery<TEntity> WhereUndefined<TIndexOn>(Expression<Func<TEntity, TIndexOn>> indexExpression);

        ICacheIndexQuery<TEntity> Where<TIndexOn>(
            Expression<Func<TEntity, TIndexOn>> indexExpression, Func<TIndexOn, bool> valueCondition);

        List<TEntity> ToList();
    }
}
