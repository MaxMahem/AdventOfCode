namespace AdventOfCode.Helpers;

using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public static class IEnumerableNumeric {
    /// <summary>Returns the product (multiplication) of the values.</summary>
    public static TNumber Product<TNumber>(this IEnumerable<TNumber> numbers)
        where TNumber : INumber<TNumber>
        => numbers.Aggregate((left, right) => left * right);
}