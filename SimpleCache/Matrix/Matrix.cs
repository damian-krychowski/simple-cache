using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCache.Matrix
{
    public class Matrix<TKeyX, TKeyY, TValue> : IMatrix<TKeyX, TKeyY, TValue>
    {
        #region Globals
        readonly object _changingLock = new object();
        readonly Dictionary<TKeyX, Dictionary<TKeyY, TValue>> _xPerspective = new Dictionary<TKeyX, Dictionary<TKeyY, TValue>>();
        readonly Dictionary<TKeyY, Dictionary<TKeyX, TValue>> _yPerspective = new Dictionary<TKeyY, Dictionary<TKeyX, TValue>>();
        #endregion

        #region Constructors
        #endregion

        #region
        void ICollection<CoordinatesValuePair<TKeyX, TKeyY, TValue>>.Add(CoordinatesValuePair<TKeyX, TKeyY, TValue> item)
        {
            this[item.KeyX, item.KeyY] = item.Value;
        }

        bool ICollection<CoordinatesValuePair<TKeyX, TKeyY, TValue>>.Contains(CoordinatesValuePair<TKeyX, TKeyY, TValue> item)
        {
            if (ContainsXY(item.KeyX, item.KeyY))
            {
                return EqualityComparer<TValue>.Default.Equals(this[item.KeyX, item.KeyY], item.Value);
            }
            return false;
        }

        bool ICollection<CoordinatesValuePair<TKeyX, TKeyY, TValue>>.Remove(CoordinatesValuePair<TKeyX, TKeyY, TValue> item)
        {
            if (this.ContainsXY(item.KeyX, item.KeyY))
            {
                if (EqualityComparer<TValue>.Default.Equals(this[item.KeyX, item.KeyY], item.Value))
                {
                    return RemoveXY(item.KeyX, item.KeyY);
                }
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<CoordinatesValuePair<TKeyX, TKeyY, TValue>>.CopyTo(CoordinatesValuePair<TKeyX, TKeyY, TValue>[] array, int arrayIndex)
        {
            foreach (var item in this)
            {
                array[arrayIndex] = item;
                arrayIndex++;
            }
        }

        public IEnumerator<CoordinatesValuePair<TKeyX, TKeyY, TValue>> GetEnumerator()
        {
            return new MatrixEnumerator<TKeyX, TKeyY, TValue>(_xPerspective);
        }

        public void Clear()
        {
            _xPerspective.Clear();
            _yPerspective.Clear();
        }

        public int Count
        {
            get
            {
                return _xPerspective.Keys.Sum(key => _xPerspective[key].Count);
            }
        }

        public bool IsReadOnly => false;

        #endregion

        #region IMatrix
        public bool ContainsX(TKeyX key)
        {
            if (_xPerspective.ContainsKey(key))
            {
                return _xPerspective[key].Keys.Count > 0;
            }
            return false;
        }

        public bool ContainsY(TKeyY key)
        {
            if (_yPerspective.ContainsKey(key))
            {
                return _yPerspective[key].Keys.Count > 0;
            }
            return false;
        }

        public bool ContainsXY(TKeyX keyX, TKeyY keyY)
        {
            return _xPerspective.ContainsKey(keyX) &&
                   _xPerspective[keyX].ContainsKey(keyY);
        }

        public bool Contains(TValue value)
        {
            return _xPerspective.Keys.Any(key => _xPerspective[key].ContainsValue(value));
        }

        public void Add(TKeyX keyX, TKeyY keyY, TValue value)
        {
            lock (_changingLock)
            {
                GetXDictionary(keyX).Add(keyY, value);
                GetYDictionary(keyY).Add(keyX, value);
            }
        }

        public bool RemoveXY(TKeyX keyX, TKeyY keyY)
        {
            lock (_changingLock)
            {
                return GetXDictionary(keyX).Remove(keyY) &&
                       GetYDictionary(keyY).Remove(keyX);
            }
        }

        public int RemoveX(TKeyX keyX)
        {
            lock (_changingLock)
            {
                int removedItems = 0;

                if (_xPerspective.ContainsKey(keyX))
                {
                    removedItems = _xPerspective[keyX].Count;

                    List<TKeyY> yKeys = _xPerspective[keyX].Keys.ToList();
                    _xPerspective[keyX].Clear();

                    foreach (TKeyY key in yKeys)
                    {
                        _yPerspective[key].Remove(keyX);
                    }
                }

                return removedItems;
            }
        }

        public int RemoveY(TKeyY keyY)
        {
            lock (_changingLock)
            {
                int removedItems = 0;

                if (_yPerspective.ContainsKey(keyY))
                {
                    removedItems = _yPerspective[keyY].Count;

                    List<TKeyX> xKeys = _yPerspective[keyY].Keys.ToList();
                    _yPerspective[keyY].Clear();

                    foreach (TKeyX key in xKeys)
                    {
                        _xPerspective[key].Remove(keyY);
                    }
                }

                return removedItems;
            }
        }

        public bool TryGetValue(TKeyX keyX, TKeyY keyY, out TValue value)
        {
            if (_xPerspective.ContainsKey(keyX))
            {
                if (_xPerspective[keyX].ContainsKey(keyY))
                {
                    value = _xPerspective[keyX][keyY];
                    return true;
                }
            }

            value = default(TValue);
            return false;
        }

        public bool TryGetValuesX(TKeyX keyX, out IEnumerable<TValue> values)
        {
            if (_xPerspective.ContainsKey(keyX))
            {
                if (_xPerspective[keyX].Values.FirstOrDefault() != null)
                {
                    values = _xPerspective[keyX].Values;
                    return true;
                }
            }

            values = null;
            return false;
        }

        public bool TryGetValuesY(TKeyY keyY, out IEnumerable<TValue> values)
        {
            if (_yPerspective.ContainsKey(keyY))
            {
                if (_yPerspective[keyY].Values.FirstOrDefault() != null)
                {
                    values = _yPerspective[keyY].Values;
                    return true;
                }
            }

            values = null;
            return false;
        }

        public TValue this[TKeyX keyX, TKeyY keyY]
        {
            get
            {
                if (ContainsXY(keyX, keyY))
                {
                    return _xPerspective[keyX][keyY];
                }
                throw new ArgumentException("There is no item under these coordinates!");
            }
            set
            {
                lock (_changingLock)
                {
                    GetXDictionary(keyX)[keyY] = value;
                }
            }
        }

        public IEnumerable<TKeyX> KeysX
        {
            get
            {
                return _xPerspective
                    .Where(x => x.Value.Keys.Count > 0)
                    .Select(x => x.Key);
            }
        }

        public IEnumerable<TKeyY> KeysY
        {
            get
            {
                return _yPerspective
                    .Where(x => x.Value.Keys.Count > 0)
                    .Select(x => x.Key);
            }
        }

        public IEnumerable<TValue> Values
        {
            get { return _xPerspective.Keys.SelectMany(key => _xPerspective[key].Values); }
        }
        #endregion

        #region Help Methods
        private Dictionary<TKeyY, TValue> GetXDictionary(TKeyX keyX)
        {
            if (!_xPerspective.ContainsKey(keyX))
            {
                _xPerspective.Add(keyX, new Dictionary<TKeyY, TValue>());
            }

            return _xPerspective[keyX];
        }

        private Dictionary<TKeyX, TValue> GetYDictionary(TKeyY keyY)
        {
            if (!_yPerspective.ContainsKey(keyY))
            {
                _yPerspective.Add(keyY, new Dictionary<TKeyX, TValue>());
            }

            return _yPerspective[keyY];
        }
        #endregion

    }
}
