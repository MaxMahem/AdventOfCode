namespace AdventOfCode.IEnumerableHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

public static class IEnumerableNumeric
{
    /// <summary>Returns the product (multiplication) of the values.</summary>
    public static TNumber Product<TNumber>(this IEnumerable<TNumber> numbers)
        where TNumber : INumber<TNumber>
        => numbers.Aggregate(TNumber.One, (accumulate, value) => accumulate * value);
}
