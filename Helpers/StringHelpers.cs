namespace AdventOfCode.Helpers;

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
}