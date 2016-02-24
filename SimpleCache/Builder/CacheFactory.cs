using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCache.Builder
{
    public class CacheFactory
    {
        public static ICacheBuilder<TEntity> CreateFor<TEntity>() where TEntity : IEntity
        {
            return new CacheBuilder<TEntity>();
        }
    }
}
