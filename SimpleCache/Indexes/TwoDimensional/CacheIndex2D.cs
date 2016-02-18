using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using SimpleCache.ExtensionMethods;
using SimpleCache.Matrix;

namespace SimpleCache.Indexes.TwoDimensional
{
    internal class CacheIndex2D<TEntity, TIndexedOnFirst, TIndexedOnSecond> :
        ICacheIndex2D<TEntity, TIndexedOnFirst, TIndexedOnSecond>, 
        ICacheIndex2D<TEntity>
        where TEntity : IEntity
    {
        readonly ConcurrentDictionary<Guid, Tuple<TIndexedOnFirst, TIndexedOnSecond>> _indexationList = new ConcurrentDictionary<Guid, Tuple<TIndexedOnFirst, TIndexedOnSecond>>(); 
        readonly Matrix<TIndexedOnFirst, TIndexedOnSecond, object> _locks = new Matrix<TIndexedOnFirst, TIndexedOnSecond, object>();
        readonly Matrix<TIndexedOnFirst, TIndexedOnSecond, List<Guid>> _index = new Matrix<TIndexedOnFirst, TIndexedOnSecond, List<Guid>>();

        Func<TEntity, TIndexedOnFirst> _firstIndexFunc;
        Func<TEntity, TIndexedOnSecond> _secondIndexFunc;

        ISimpleCache<TEntity> _parentCache;


        public bool IsOnExpression(Expression firstIndexedProperty, Expression secondIndexedProperty)
        {
            return firstIndexedProperty.Comapre(FirstPropertyWithIndex) &&
                   secondIndexedProperty.Comapre(SecondPropertyWithIndex);
        }

        public void AddOrUpdate(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            TIndexedOnFirst indexKeyX = _firstIndexFunc(entity);
            TIndexedOnSecond indexKeyY = _secondIndexFunc(entity);

            var wasIndexed = _indexationList.ContainsKey(entity.Id);

            if (wasIndexed)
            {
                var previousKeys = _indexationList[entity.Id];

                if (!previousKeys.Item1.Equals(indexKeyX) || !previousKeys.Item2.Equals(indexKeyY))
                {
                    RemoveEntity(entity.Id);
                    AddItemToIndex(indexKeyX, indexKeyY, entity.Id);
                }
            }

            AddItemToIndex(indexKeyX, indexKeyY, entity.Id);
        }

        public void TryRemove(TEntity entity)
        {
            if(entity == null) throw new ArgumentNullException(nameof(entity));
            TryRemove(entity.Id);
        }

        public void TryRemove(Guid entityId)
        {
            var wasIndexed = _indexationList.ContainsKey(entityId);

            if (wasIndexed)
            {
                RemoveEntity(entityId);
            }
        }

        public void Rebuild()
        {
            Clear();

            foreach (var entity in _parentCache.Items)
            {
                AddOrUpdate(entity);
            }
        }

        public void Clear()
        {
            _index.Clear();
            _locks.Clear();
            _indexationList.Clear();
        }

        public IEnumerable<TEntity> GetWithUndefined()
        {
            throw new NotImplementedException();
        }

        private void RemoveEntity(Guid entityId)
        {
            var indexKeys = _indexationList[entityId];
            _index[indexKeys.Item1, indexKeys.Item2].Remove(entityId);
        }

        public void Initialize(
            Expression<Func<TEntity, TIndexedOnFirst>> firstIndexedProperty,
            Expression<Func<TEntity, TIndexedOnSecond>> secondIndexedProperty, 
            ISimpleCache<TEntity> parentCache)
        {
            FirstPropertyWithIndex = firstIndexedProperty;
            SecondPropertyWithIndex = secondIndexedProperty;

            _firstIndexFunc = firstIndexedProperty.Compile();
            _secondIndexFunc = secondIndexedProperty.Compile();

            _parentCache = parentCache;
        }

        public void BuildUp(IEnumerable<TEntity> entities)
        {
            foreach (TEntity entity in entities)
            {
                AddOrUpdate(entity);
            }
        }

        public Expression<Func<TEntity, TIndexedOnFirst>> FirstPropertyWithIndex
        {
            get;
            private set;
        }

        public Expression<Func<TEntity, TIndexedOnSecond>> SecondPropertyWithIndex
        {
            get;
            private set;
        }

        public IEnumerable<TEntity> GetFromFirst(TIndexedOnFirst firstIndexedId)
        {
            if(firstIndexedId == null) throw new ArgumentNullException(nameof(firstIndexedId));

            IEnumerable<List<Guid>> idsLists;
            
            if(_index.TryGetValuesX(firstIndexedId, out idsLists))
            {
                foreach(List<Guid> idsList in idsLists)
                {
                    foreach(Guid id in idsList)
                    {
                        yield return _parentCache.GetEntity(id);
                    }
                }
            }
        }

        public IEnumerable<TEntity> GetFromSecond(TIndexedOnSecond secondIndexedId)
        {
            if(secondIndexedId == null) throw new ArgumentNullException(nameof(secondIndexedId));

            IEnumerable<List<Guid>> idsLists;

            if (_index.TryGetValuesY(secondIndexedId, out idsLists))
            {
                foreach (List<Guid> idsList in idsLists)
                {
                    foreach (Guid id in idsList)
                    {
                        yield return _parentCache.GetEntity(id);
                    }
                }
            }
        }

        public IEnumerable<TEntity> Get(TIndexedOnFirst firstIndexedId, TIndexedOnSecond secondIndexedId)
        {
            if(firstIndexedId == null) throw new ArgumentNullException(nameof(firstIndexedId));
            if(secondIndexedId == null) throw new ArgumentNullException(nameof(secondIndexedId));

            List<Guid> idsList;

            if (_index.TryGetValue(firstIndexedId, secondIndexedId, out idsList))
            {
                foreach (Guid id in idsList)
                {
                    yield return _parentCache.GetEntity(id);
                }
            }
        }

        public IEnumerable<TEntity> GetWithFirstUndefined(TIndexedOnSecond secondIndexedId)
        {
            if(secondIndexedId == null) throw new ArgumentNullException(nameof(secondIndexedId));
            throw new NotImplementedException();
        }

        public IEnumerable<TEntity> GetWithSecondUndefined(TIndexedOnFirst firstIndexedId)
        {
            if(firstIndexedId == null) throw new ArgumentNullException(nameof(firstIndexedId));

            throw new NotImplementedException();
        }

        public IEnumerable<TEntity> GetWithBothUndefined()
        {
            throw new NotImplementedException();
        }


        private List<Guid> GetIndexList(TIndexedOnFirst indexKeyX, TIndexedOnSecond indexKeyY)
        {
            if (!_index.ContainsXY(indexKeyX, indexKeyY))
            {
                _index.Add(indexKeyX, indexKeyY, new List<Guid>());
                _locks.Add(indexKeyX, indexKeyY, new object());
            }

            return _index[indexKeyX, indexKeyY];
        }

        private void AddItemToIndex(TIndexedOnFirst indexKeyX, TIndexedOnSecond indexKeyY, Guid itemId)
        {
            List<Guid> indexList = GetIndexList(indexKeyX, indexKeyY);

            _indexationList.AddOrUpdate(
                itemId, 
                new Tuple<TIndexedOnFirst, TIndexedOnSecond>(indexKeyX, indexKeyY), 
                (k, v) => new Tuple<TIndexedOnFirst, TIndexedOnSecond>(indexKeyX, indexKeyY));

            lock (_locks[indexKeyX,indexKeyY])
            {
                indexList.Add(itemId);
            }
        }
    }
}
