using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCache.Matrix
{
    internal class MatrixEnumerator<TKeyX, TKeyY, TValue> : IEnumerator<CoordinatesValuePair<TKeyX, TKeyY, TValue>>
    {
        #region Globals
        readonly Dictionary<TKeyX, Dictionary<TKeyY, TValue>> _xPerspective;
        readonly List<TKeyX> _keys;
        int _currentXIndex = -1;
        int _currentYIndex = -1;
        #endregion


        #region Constructors
        public MatrixEnumerator(
            Dictionary<TKeyX, Dictionary<TKeyY, TValue>> xPerspective)
        {
            _xPerspective = xPerspective;
            _keys = xPerspective.Keys.ToList();
        }
        #endregion

        #region Properties
        private TKeyX CurrentKeyX
        {
            get { return _keys[_currentXIndex]; }
        }

        private TKeyY CurrentKeyY
        {
            get { return _xPerspective[CurrentKeyX].Keys.ElementAt(_currentYIndex); }
        }

        private TValue CurrentValue
        {
            get { return _xPerspective[CurrentKeyX][CurrentKeyY]; }
        }
        #endregion

        #region IEnumerator
        public void Dispose() { }

        public bool MoveNext()
        {
            if(_currentXIndex == -1)
            {
                _currentXIndex++;
            }

            if(_currentYIndex < _xPerspective[_keys[_currentXIndex]].Count - 1)
            {
                _currentYIndex++;
                return true;
            }
            else
            {
                _currentYIndex = -1;
                while(_currentXIndex < _keys.Count - 1)
                {
                    _currentXIndex++;
                    if (_currentYIndex < _xPerspective[_keys[_currentXIndex]].Count - 1)
                    {
                        _currentYIndex++;
                        return true;
                    }
                }
            }

            return false;
        }

        public void Reset()
        {
            _currentXIndex = -1;
            _currentYIndex = -1;
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public CoordinatesValuePair<TKeyX, TKeyY, TValue> Current
        {
            get
            {
                return new CoordinatesValuePair<TKeyX, TKeyY, TValue>(
                    CurrentKeyX,
                    CurrentKeyY,
                    CurrentValue);
            }
        }
        #endregion
    }
}
