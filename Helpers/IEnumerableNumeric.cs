namespace AdventOfCode.Helpers;

using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public static class IEnumerableNumeric
{
    /// <summary>Returns the product (multiplication) of the values.</summary>
    public static TNumber Product<TNumber>(this IEnumerable<TNumber> numbers)
        where TNumber : INumber<TNumber>
        => numbers.Aggregate((left, right) => left * right);

    public static long Concatinate(this IEnumerable<int> numbers) {
        long result = 0;
        long multiplier = 1;

        foreach (long number in numbers) {
            long workingNumber = number;
            while (workingNumber > 0) {
                int digit = (int)(workingNumber % 10);
                result += digit * multiplier;
                workingNumber /= 10;
                multiplier *= 10;
            }
        }

        return result;
    }

    public static long Concatinate(this IEnumerable<long> numbers) {
        long result = 0;
        long multiplier = 1;

        foreach (long number in numbers) {
            long workingNumber = number;
            while (workingNumber > 0) {
                int digit = (int)(workingNumber % 10);
                result += digit * multiplier;
                workingNumber /= 10;
                multiplier *= 10;
            }
        }

        return result;
    }
}