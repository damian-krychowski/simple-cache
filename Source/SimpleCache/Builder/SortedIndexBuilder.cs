namespace SimpleCache.Builder
{
    internal class SortedIndexBuilder<TEntity> : ISortedIndexBuilder<TEntity>
        where TEntity : IEntity
    {
        private readonly ICacheBuilder<TEntity> _cacheBuilder;
        public bool IsAscending { get; private set; }


        public SortedIndexBuilder(
            ICacheBuilder<TEntity> cacheBuilder)
        {
            _cacheBuilder = cacheBuilder;
        }

        public ICacheBuilder<TEntity> Descending()
        {
            IsAscending = false;
            return _cacheBuilder;
        }

        public ICacheBuilder<TEntity> Ascending()
        {
            IsAscending = true;
            return _cacheBuilder;
        }
    }
}