namespace AdventOfCode.Helpers;

using System.Collections;

public static class BitArrayHelper {
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
