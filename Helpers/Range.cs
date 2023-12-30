namespace AdventOfCode.Helpers;

/// <summary>A generic Range. Uses an [start, end) definition.</summary>
/// <param name="Start"></param>
/// <param name="End"></param>
public readonly record struct Range<T>(T Start, T End) : IComparable<Range<T>> where T : INumber<T>
{
    public T Length => End - Start;
    public bool Contains(T value) => value >= Start && value < End;
    public bool Contains(in Range<T> other) => Start <= other.Start && other.End <= End;
    public bool Intersects(in Range<T> other) => Start < other.End && End > other.Start;
    public int CompareTo(Range<T> other) => Start.CompareTo(other.Start);

    public static implicit operator Range<T>((T start, T end) tuple) => new(tuple.start, tuple.end);

    /// <summary>Splits this range by <paramref name="other"/>.</summary>
    /// <param name="other">The Range(T) to split this range.</param>
    /// <returns>A tuple with nullable components corresponding to the portions of this range to the left of,
    /// right, of, and inside <paramref name="other"/></returns>
    public (Range<T>? Left, Range<T>? Inside, Range<T>? Right) Split(in Range<T> other) {

        if (other.Start >= End)   return (this, null, null); // entirely to the left of other range. 
        if (other.End < Start)    return (null, null, this); // entirely to the right of other range.
        if (other.Contains(this)) return (null, this, null); // entirely within the other range.

        // intersecting cases.
        Range<T>? left = null, right = null;

        // in the case of an intersection there will always be an inside part.
        Range<T> inside = new Range<T>(T.Max(other.Start, Start), T.Min(End, other.End));

        // Check and set the side parts conditionally. The only exist if a portion is outside the range.
        if (other.Start > Start) left = new Range<T>(Start, other.Start);
        if (other.End < End) right = new Range<T>(other.End, End);

        return (left, inside, right);
    }

    public Range<T> Merge(in Range<T> other) {
        if (!Intersects(other)) throw new ArgumentException("Ranges do not overlap.");

        return new Range<T>(T.Min(Start, other.Start), T.Max(End, other.End));
    }

    public bool TryMerge(in Range<T> other, out Range<T> mergedRange) {
        if (!Intersects(other)) {
            mergedRange = default;
            return false;
        }

        mergedRange = new Range<T>(T.Min(Start, other.Start), T.Max(End, other.End));
        return true;
    }

    public static bool operator <(Range<T> left,  Range<T> right) => left.CompareTo(right) <  0;
    public static bool operator >(Range<T> left,  Range<T> right) => left.CompareTo(right) >  0;

    public static bool operator <=(Range<T> left, Range<T> right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Range<T> left, Range<T> right) => left.CompareTo(right) >= 0;

    // shift operators.
    public static Range<T> operator +(Range<T> left, T right) => (left.Start + right, left.End + right);
    public static Range<T> operator -(Range<T> left, T right) => (left.Start - right, left.End - right);
    public static Range<T> operator +(T left, Range<T> right) => (left + right.Start, left + right.End);
    public static Range<T> operator -(T left, Range<T> right) => (left - right.Start, left - right.End);
}

public static class RangeExtensions {
    public static IEnumerable<Range<T>> Merge<T>(this IEnumerable<Range<T>> ranges) where T : INumber<T> {
        if (!ranges.Any()) yield break;
        var sortedRanges = ranges.Order().ToList();

        for (int index = 0; index < sortedRanges.Count - 1; index++) {
            Range<T> current = sortedRanges[index], next = sortedRanges[index + 1];

            // insert merged range into the next index so it can be possibly merged with the next item.
            if (current.TryMerge(next, out Range<T> merged)) sortedRanges[index + 1] = merged; 
            else yield return current;
        }

        yield return sortedRanges.Last();
    }
}