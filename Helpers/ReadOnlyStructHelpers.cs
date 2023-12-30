namespace AdventOfCode.Helpers;

using CommunityToolkit.HighPerformance;

using System.Runtime.CompilerServices;

public static class ReadOnlyStructHelpers {
    public static ReadOnlySpanMultiTokenTokenizer<T> Tokenize<T>(this ReadOnlySpan<T> source, ReadOnlySpan<T> seperators)
        where T : IEquatable<T> => new(source, seperators);

    public static ReadOnlySpanMultiTokenTokenizer<T> Tokenize<T>(this ReadOnlySpan<T> source, IEnumerable<T> seperators)
        where T : IEquatable<T> => new(source, seperators.ToArray().AsSpan());

    public static ReadOnlySpanTokenizerIgnoreEmpty<T> TokenizeIgnoreEmpty<T>(this ReadOnlySpan<T> source, T seperator)
        where T : IEquatable<T> => new(source, seperator);
}

/// <summary>A <see langword="ref"/> <see langword="struct"/> that tokenizes a given <see cref="ReadOnlySpan{T}"/> instance given multiple possible tokens.</summary>
/// <param name="separators">The separator item to use.</param>
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public ref struct ReadOnlySpanTokenizerIgnoreEmpty<T>(ReadOnlySpan<T> span, T separator) where T : IEquatable<T>
{
    private readonly ReadOnlySpan<T> span = span;
    private readonly T separator = separator;

    public (int Start, int End) Offset { get; private set; } = (0, -1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpanTokenizerIgnoreEmpty<T> GetEnumerator() => this;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext() {
        int newStart = Offset.End + 1;

        // check to see if offset is in length
        if (newStart <= this.span.Length) {
            int nearestValueIndex = this.span[newStart..].IndexOfAnyExcept(separator);
            if (nearestValueIndex < 0) return false;    // if no more non-tokens are found, cannot enumerate.

            // token found, offset starts at this position.
            newStart += nearestValueIndex;

            // find where the next seperator is, marks the end of the offset.
            var trailingSearchSpan = this.span[newStart..];
            int seperatorAfterValueIndex = System.MemoryExtensions.IndexOf(trailingSearchSpan, separator);
            int newEnd = seperatorAfterValueIndex < 0 ? newStart + trailingSearchSpan.Length // no trailingn seperator found, span goes to end.
                                                      : newStart + seperatorAfterValueIndex; // seperator found, span goes to that point

            this.Offset = (newStart, newEnd);
            return true;
        }

        return false;
    }

    public readonly ReadOnlySpan<T> Current {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.span[Offset.Start..this.Offset.End];
    }

    public void Reset() => Offset = (0, -1);
}


/// <summary>A <see langword="ref"/> <see langword="struct"/> that tokenizes a given <see cref="ReadOnlySpan{T}"/> instance given multiple possible tokens.</summary>
/// <param name="separators">The separator item to use.</param>
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public ref struct ReadOnlySpanMultiTokenTokenizer<T>(ReadOnlySpan<T> span, ReadOnlySpan<T> separators) where T : IEquatable<T>
{
    private readonly ReadOnlySpan<T> span = span;
    private readonly ReadOnlySpan<T> separators = separators;
    private (int Start, int End) offsets = (0, -1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpanMultiTokenTokenizer<T> GetEnumerator() => this;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext() {
        int newStart = this.offsets.End + 1;
        int length = this.span.Length;

        // check to see if offset is in length
        if (newStart <= length) {
            this.offsets.Start = newStart;

            // find the nearest seperator
            int nearestSeperatorIndex = int.MaxValue;
            for (int seperatorsIndex = 0; seperatorsIndex < separators.Length; seperatorsIndex++) {
                int seperatorIndex = System.MemoryExtensions.IndexOf(this.span[newStart..], this.separators[seperatorsIndex]);
                nearestSeperatorIndex = seperatorIndex != -1 && seperatorIndex < nearestSeperatorIndex ? seperatorIndex : nearestSeperatorIndex;
            }

            // check to see if a nearest seperator is found, if so, the span is from our newStart to this position.
            if (nearestSeperatorIndex != int.MaxValue) {
                this.offsets.End = newStart + nearestSeperatorIndex;
                return true;
            }

            // if a nearest seperator is not found, span is from newStart to the end of the span.
            this.offsets.End = length;
            return true;
        }

        return false;
    }

    public readonly ReadOnlySpan<T> Current {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.span[this.offsets.Start..this.offsets.End];
    }

    public void Reset() => this.offsets = (0, -1);
}