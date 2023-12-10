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
}
