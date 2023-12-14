namespace AdventOfCode.Helpers;

using System.Collections;

public static class BitArrayHelper {

    public static BitArray CreateBitArray<T>(this IEnumerable<T> source, Func<T, bool> selector) {
        int itemCount = source.Count();
        BitArray bitArray = new(itemCount);

        int arrayIndex = 0;
        foreach (T item in source) {
            bitArray[arrayIndex++] = selector(item);
        }

        return bitArray;
    }

    public static BitArray CreateBitArray<T>(this ReadOnlySpan<T> source, Func<T, bool> selector) {
        int itemCount = source.Length;
        BitArray bitArray = new(itemCount);

        int arrayIndex = 0;
        foreach (T item in source) {
            bitArray[arrayIndex++] = selector(item);
        }

        return bitArray;
    }

    public static IEnumerator<bool> GetTypedEnumerator(this BitArray bitArray) => new BitArrayEnumerator(bitArray);

    private class BitArrayEnumerator(BitArray bitArray) : IEnumerator<bool> {
        private readonly BitArray _bitArray = bitArray;
        private int _index = -1;
        private readonly int _length = bitArray.Length;

        public bool Current => _bitArray[_index];
        object IEnumerator.Current => _bitArray[_index];

        public void Dispose() { }
        public bool MoveNext() { 
            _index++;
            return _index < _length;
        }
        public void Reset() { _index = -1; }
    }
}
