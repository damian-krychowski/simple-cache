namespace SimpleCache.Matrix
{
    public class CoordinatesValuePair<TKeyX, TKeyY, TValue>
    {
        public CoordinatesValuePair(
            TKeyX keyX,
            TKeyY keyY,
            TValue value)
        {
            KeyX = keyX;
            KeyY = keyY;
            Value = value;
        }

        public TKeyX KeyX { get; }

        public TKeyY KeyY { get; }

        public TValue Value { get; }
    }

    public class CoordinatesValuePair<TKeyX, TKeyY, TKeyZ, TValue>
    {
        public CoordinatesValuePair(
            TKeyX keyX,
            TKeyY keyY,
            TKeyZ keyZ,
            TValue value)
        {
            KeyX = keyX;
            KeyY = keyY;
            KeyZ = keyZ;
            Value = value;
        }

        public TKeyX KeyX { get; }

        public TKeyY KeyY { get; }

        public TKeyZ KeyZ { get; }

        public TValue Value { get; }
    }
}
