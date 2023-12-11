namespace AdventOfCode.Helpers;
using System;
using System.Collections.Generic;

public static class IEnumerableHelpers
{
    public static IEnumerable<TResult> Pairwise<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TSource, TResult> resultSelector) {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(resultSelector);

        return PairwiseEnumerate(); 
        
        IEnumerable<TResult> PairwiseEnumerate() {
            using var e = source.GetEnumerator();

            if (!e.MoveNext())
                yield break;

            var previous = e.Current;
            while (e.MoveNext()) {
                yield return resultSelector(previous, e.Current);
                previous = e.Current;
            }
        }
    }

    /// <summary>Generates all possible combinations pairs of this Enumerable.</summary>
    /// <remarks>Note this method does not return the pairs in an ordered sequence.</remarks>
    /// <returns>An enumeration of tuples representing all possible combination pairs.</returns>
    public static IEnumerable<(T, T)> PairCombinations<T>(this IEnumerable<T> source) {
        ArgumentNullException.ThrowIfNull(source);

        return GeneratePairCombinations();
        IEnumerable<(T, T)> GeneratePairCombinations() {
            var list = source.TryGetNonEnumeratedCount(out int count) ? new List<T>(count) : [];

            foreach (var firstItem in source) {
                foreach (var secondItem in list) yield return (firstItem, secondItem);

                list.Add(firstItem);
            }
        }
    }
}
