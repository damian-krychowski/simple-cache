using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCache.Builder
{
    public interface ISortedIndexBuilder<TEntity>
        where TEntity : IEntity
    {
        ICacheBuilder<TEntity> Descending();
        ICacheBuilder<TEntity> Ascending();
    }
}
