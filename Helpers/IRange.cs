namespace AdventOfCode.Helpers;

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

public class RangeDictionary<T, TValue>  
    where T : INumber<T> 
{
    readonly IReadOnlyList<Range<T>> _ranges;
    readonly IReadOnlyList<TValue>   _values;

    public RangeDictionary(IEnumerable<KeyValuePair<Range<T>, TValue>> keyValuePairs) {
        List<Range<T>> ranges = [];
        List<TValue>   values = [];

        Range<T> lastKey = keyValuePairs.First().Key;
        foreach (var kvp in keyValuePairs.Skip(1)) {
            if (kvp.Key.CompareTo(lastKey) > 0) throw new ArgumentException("Ranges must be in order.", nameof(keyValuePairs));
            if (kvp.Key.Intersects(lastKey))    throw new ArgumentException("Ranges may not intersect.", nameof(keyValuePairs));

            lastKey = kvp.Key;
            ranges.Add(kvp.Key);
            values.Add(kvp.Value);
        }

        _ranges = ranges.ToImmutableArray();
        _values = values.ToImmutableArray();
    }

    public TValue this[T key] => throw new NotImplementedException();

    public TValue this[Range<T> key] => throw new NotImplementedException();

    public IEnumerable<Range<T>> Keys => _ranges;
    public IEnumerable<TValue> Values => _values;
    public int Count => _ranges.Count;

    public bool ContainsKey(T key) {
        return false;
    }
    public IEnumerator<KeyValuePair<Range<T>, TValue>> GetEnumerator() => throw new NotImplementedException();

    public bool TryGetValue(T key, [MaybeNullWhen(false)] out TValue value) {
        var nearsetRangeIndex = NearestRangeIndex(key);
        if (nearsetRangeIndex > this._values.Count || !_ranges[nearsetRangeIndex].Contains(key)) { 
            value = default; 
            return false; 
        }
        value = _values[nearsetRangeIndex];
        return true;
    }

    public bool TryGetValue(Range<T> key, [MaybeNullWhen(false)] out TValue value) => throw new NotImplementedException();
    private int NearestRangeIndex(T key) => NearestRangeIndex(new Range<T>(key, key + T.One));
        
    private int NearestRangeIndex(Range<T> key) {
        int nearestIndex = _ranges.BinarySearch(key);
        return nearestIndex >= 0 ? nearestIndex : ~nearestIndex - 1;
    }
}

public static class IReadOnlyListHelper
{
    /// <inheritdoc cref="BinarySearch{TItem, TSearch}(IList{TItem}, TSearch, Func{TSearch, TItem, int})"/>
    public static int BinarySearch<TItem, TSearch>(this IReadOnlyList<TItem> list, TSearch value, Func<TSearch, TItem, int> comparer) {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(comparer);

        int lower = 0;
        int upper = list.Count - 1;

        while (lower <= upper) {
            int middle = lower + (upper - lower) / 2;
            int comparisonResult = comparer(value, list[middle]);
            switch (comparisonResult) {
                case < 0:
                    upper = middle - 1;
                    break;
                case > 0:
                    lower = middle + 1;
                    break;
                default:
                    return middle;
            }
        }

        return ~lower;
    }

    public static int BinarySearch<TItem>(this IReadOnlyList<TItem> list, TItem value) {
        return BinarySearch(list, value, Comparer<TItem>.Default);
    }

    public static int BinarySearch<TItem>(this IReadOnlyList<TItem> list, TItem value,
        IComparer<TItem> comparer) {
        return list.BinarySearch(value, comparer.Compare);
    }
}