using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCache.Builder
{
    public class CacheBuilderFactory
    {
        public static ICacheBuilder<TEntity> Create<TEntity>() where TEntity : IEntity
        {
            return new CacheBuilder<TEntity>();
        }
    }
}
