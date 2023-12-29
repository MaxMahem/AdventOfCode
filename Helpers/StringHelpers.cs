namespace AdventOfCode.Helpers;

using CommunityToolkit.HighPerformance;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public static class StringHelpers 
{
    /// <summary>Repeats <paramref name="source"/> <paramref name="repetitions"/> times, joined with <paramref name="seperator"/>.</summary>
    /// <param name="source">The string to be repeated.</param>
    /// <param name="seperator">The seperator to be placed between the repetitions.</param>
    /// <param name="repetitions">The number of times to repeat the source string. Must be greater than 0.</param>
    /// <returns><paramref name="source"/> repeated <paramref name="repetitions"/> times, seperated by <paramref name="seperator"/>.</returns>
    public static string RepeatJoin(this string source, char seperator, int repetitions) {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(repetitions);

        return string.Create((source.Length * repetitions) + repetitions - 1, (source, seperator, repetitions), InternalRepeatJoin);
    }

    private static void InternalRepeatJoin(Span<char> buffer, (string Source, char Seperator, int Repetitions) input) {
        // repeat repetitions - 1 times to copy in all the seperators and all but one of the soucre repetitions.
        for (int repetition = input.Repetitions - 1; repetition >= 1; repetition--) {
            int startIndex = repetition * input.Source.Length + repetition;
            input.Source.AsSpan().CopyTo(buffer[startIndex..]);

            buffer[startIndex - 1] = input.Seperator;
        }
        input.Source.AsSpan().CopyTo(buffer);   // last copy.
    }

    public static bool TryParseNumberWord(string words) {
        var wordArray = words.Split(' ', '-');
        int finalNumber = 0;
        bool foundValue = false;

        int multiplier = 1;

        words.AsSpan().Tokenize([' ', '-']);

        for (int i = wordArray.Length - 1; i >= 0; i--) {
            if (AdditionWords.TryGetValue(wordArray[i], out int additionValue)) {
                finalNumber += additionValue * multiplier;
                foundValue = true;
            }
            if (MultiplicationWords.TryGetValue(wordArray[i], out int multiplicationValue)) {
                if (multiplicationValue < multiplier) {
                    multiplier *= multiplicationValue;
                }
                else {
                    multiplier = multiplicationValue;
                }
            }
            if (ZeroWords.TryGetValue(wordArray[i], out int zeroValue)) {
                finalNumber = zeroValue;
                return true;
            }
        }
        return foundValue;
    }

    private static readonly ImmutableSortedDictionary<string, int> ZeroWords = new Dictionary<string, int>() {
        { "zero", 0 },
    }.ToImmutableSortedDictionary();

    private static readonly ImmutableSortedDictionary<string, int> AdditionWords = new Dictionary<string, int>() {
        {"one" ,       1}, 
        {"two",        2},
        {"three",      3},
        {"four",       4},
        {"five",       5},
        {"six",        6},
        {"seven",      7}, 
        {"eight",      8},
        {"nine",       9},
        {"ten",       10},
        {"eleven",    11},
        {"twelve",    12},
        {"thirteen",  13},
        {"fourteen",  14},
        {"fifteen",   15},
        {"sixteen",   16},
        {"seventeen", 17},
        {"eighteen",  18},
        {"nineteen",  19},
        {"twenty",    20},
        {"thirty",    30},
        {"forty",     40},
        {"fifty",     50},
        {"sixty",     60},
        {"seventy",   70},
        {"eighty",    80},
        {"ninety",    90}
    }.ToImmutableSortedDictionary();

    private static readonly ImmutableSortedDictionary<string, int> MultiplicationWords = new Dictionary<string, int>() {
        { "hundred",           100 },
        { "thousand",        1_000 },
        { "million",     1_000_000 },
        { "billion", 1_000_000_000 },
        { "minus",              -1 },
        { "negative",           -1 },
    }.ToImmutableSortedDictionary();
}