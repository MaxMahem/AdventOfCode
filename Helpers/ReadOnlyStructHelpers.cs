namespace AdventOfCode.Helpers;

using CommunityToolkit.HighPerformance;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class ReadOnlyStructHelpers {
    public static ReadOnlySpanMultiTokenTokenizer<T> Tokenize<T>(this ReadOnlySpan<T> source, ReadOnlySpan<T> seperators)
        where T : IEquatable<T> => new(source, seperators);

    public static ReadOnlySpanMultiTokenTokenizer<T> Tokenize<T>(this ReadOnlySpan<T> source, IEnumerable<T> seperators)
        where T : IEquatable<T> => new(source, seperators.ToArray().AsSpan());
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