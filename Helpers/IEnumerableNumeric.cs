namespace AdventOfCode.Helpers;

using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public static class IEnumerableNumeric {
    /// <summary>Returns the product (multiplication) of the values.</summary>
    public static TNumber Product<TNumber>(this IEnumerable<TNumber> numbers)
        where TNumber : INumber<TNumber>
        => numbers.Aggregate(TNumber.One, (accumulate, value) => accumulate * value);
}

public static class IListExtensions {
    /// <summary>Performs a binary search on the specified collection.</summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <typeparam name="TSearch">The type of the searched item.</typeparam>
    /// <param name="list">The list to be searched.</param>
    /// <param name="value">The value to search for.</param>
    /// <param name="comparer">The comparer that is used to compare the value
    /// with the list items.</param>
    /// <returns></returns>
    public static int BinarySearch<TItem, TSearch>(this IList<TItem> list,
        TSearch value, Func<TSearch, TItem, int> comparer) {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(comparer);

        int lower = 0;
        int upper = list.Count - 1;

        while (lower <= upper) {
            int middle = lower + (upper - lower) / 2;
            int comparisonResult = comparer(value, list[middle]);
            if (comparisonResult < 0) {
                upper = middle - 1;
            }
            else if (comparisonResult > 0) {
                lower = middle + 1;
            }
            else {
                return middle;
            }
        }

        return ~lower;
    }

    /// <summary>Performs a binary search on the specified collection.</summary>
    public static int BinarySearch<TItem>(this IList<TItem> list, TItem value) {
        return BinarySearch(list, value, Comparer<TItem>.Default);
    }

    /// <summary>Performs a binary search on the specified collection.</summary>
    public static int BinarySearch<TItem>(this IList<TItem> list, TItem value,
        IComparer<TItem> comparer) {
        return list.BinarySearch(value, comparer.Compare);
    }
}