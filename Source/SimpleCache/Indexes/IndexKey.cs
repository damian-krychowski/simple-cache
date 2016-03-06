using System;

namespace SimpleCache.Indexes
{
    internal class IndexKey<TEntity, TIndexOn>
        where TEntity : IEntity
    {
        public bool Defined { get; private set; }
        public TIndexOn Value { get; private set; }

        private IndexKey()
        {
        }

        public static IndexKey<TEntity, TIndexOn> Determine(Func<TEntity, TIndexOn> indexFunc, TEntity entity)
        {
            try
            {
                var result = indexFunc(entity);
                return result == null ? UndefinedKey() : DefinedKey(result);
            }
            catch
            {
                return UndefinedKey();
            }
        }

        private static IndexKey<TEntity, TIndexOn> DefinedKey(TIndexOn value)
        {
            return new IndexKey<TEntity, TIndexOn>
            {
                Defined = true,
                Value = value
            };
        }

        private static IndexKey<TEntity, TIndexOn> UndefinedKey()
        {
            return new IndexKey<TEntity, TIndexOn>
            {
                Defined = false,
            };
        }
    }
}