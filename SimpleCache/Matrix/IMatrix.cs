using System.Collections.Generic;

namespace SimpleCache.Matrix
{
    public interface IMatrix<TKeyX, TKeyY, TValue> :
        ICollection<CoordinatesValuePair<TKeyX, TKeyY, TValue>>
    {
        bool ContainsX(TKeyX key);
        bool ContainsY(TKeyY key);
        bool ContainsXY(TKeyX keyX, TKeyY keyY);
        bool Contains(TValue value);

        void Add(TKeyX keyX, TKeyY keyY, TValue value);

        int RemoveX(TKeyX keyX);
        int RemoveY(TKeyY keyY);
        bool RemoveXY(TKeyX keyX, TKeyY keyY);

        bool TryGetValue(TKeyX keyX, TKeyY keyY, out TValue value);
        bool TryGetValuesX(TKeyX keyX, out IEnumerable<TValue> values);
        bool TryGetValuesY(TKeyY keyY, out IEnumerable<TValue> values);

        TValue this[TKeyX keyX, TKeyY keyY] { get; set; }

        IEnumerable<TKeyX> KeysX { get; }

        IEnumerable<TKeyY> KeysY { get; }

        IEnumerable<TValue> Values { get; }
    }
}
