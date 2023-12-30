namespace AdventOfCode.Helpers;

public static class IEnumerableHelpers {
    /// <summary>Flattens a given pair of elements into a new enumeration.</summary>
    public static IEnumerable<T> Flatten<T>(T item1, T item2) { yield return item1; yield return item2; }

    /// <summary>Returns a sequence of <see cref="KeyValuePair{TKey,TValue}"/>
    /// where the key is the index of the value in the source sequence.</summary>
    /// <param name="startIndex">First index in the sequence, defaults to 0.</param>
    /// <remarks>This operator uses deferred execution.</remarks>
    public static IEnumerable<KeyValuePair<int, TSource>> Index<TSource>(this IEnumerable<TSource> source, int startIndex = 0) {
        ArgumentNullException.ThrowIfNull(source);
        return source.Select((item, index) => new KeyValuePair<int, TSource>(startIndex + index, item));
    }

    public delegate Boolean TryFunc<T, TOut>(T input, out TOut value);

    public static IEnumerable<TOut> SelectWhere<T, TOut>(this IEnumerable<T> source, TryFunc<T, TOut> tryFunc) {
        foreach (T item in source) {
            if (tryFunc(item, out TOut value)) {
                yield return value;
            }
        }
    }

    public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> source, Func<T, bool> predicate) {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);
        return source.Where(val => !predicate(val));
    }

    public static IEnumerable<T> FirstAndLast<T>(this IEnumerable<T> source) {
        ArgumentNullException.ThrowIfNull(source);
        yield return source.First();
        yield return source.Last();
    }

    public static IEnumerable<T> FirstAndLast<T>(this IEnumerable<T> source, Func<T, bool> predicate) {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);
        yield return source.First(predicate);
        yield return source.Last(predicate);
    }

    /// <summary>Projects an enumeration against its elements in pairs.</summary>
    /// <exception cref="ArgumentException">If the count of elements is not even.</exception>
    public static IEnumerable<TResult> SelectPair<T, TResult>(this IEnumerable<T> source, Func<T, T, TResult> selector) {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        if (source.TryGetNonEnumeratedCount(out var count) && !count.IsEven()) 
            throw new ArgumentException("Count of source items must be even.", nameof(source));

        return SelectPairEnumerate();
        
        IEnumerable<TResult> SelectPairEnumerate() {
            using var sourceEnumerator = source.GetEnumerator();
            while (sourceEnumerator.MoveNext()) {
                T item1 = sourceEnumerator.Current;
                if (!sourceEnumerator.MoveNext()) throw new ArgumentException("Count of source items must be even.", nameof(source));
                T item2 = sourceEnumerator.Current;
                yield return selector(item1, item2);
            }
        }
    }

    /// <summary>Sliding window enumeration.</summary>
    public static IEnumerable<TResult> Pairwise<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TSource, TResult> resultSelector) {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(resultSelector);

        return PairwiseEnumerate(); 
        
        IEnumerable<TResult> PairwiseEnumerate() {
            using var sourceEnumerator = source.GetEnumerator();

            if (!sourceEnumerator.MoveNext())
                yield break;

            var previous = sourceEnumerator.Current;
            while (sourceEnumerator.MoveNext()) {
                yield return resultSelector(previous, sourceEnumerator.Current);
                previous = sourceEnumerator.Current;
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

    public static string StringConcat(this IEnumerable<char> chars) => string.Concat(chars);
}
