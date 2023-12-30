namespace AdventOfCode.Helpers;

public static class ReadOnlyCharSpanHelpers {
    /// <summary>Attempts to parse a sequence of ASCII digits in <paramref name="source"/> as an integer. 
    /// Expects digits to start at index 0, and continues until it hits a non-AsciiDigit. Returns the
    /// number of digits parsed (0 if no digits parsed) and any number parsed via <paramref name="parsedNumber"/>
    /// </summary>
    /// <param name="source">The span to parse.</param>
    /// <param name="parsedNumber">The parsed number, or 0 if no number could be parsed.</param>
    /// <returns>The number of digits parsed.</returns>
    public static int ParseDigits(this ReadOnlySpan<char> source, out int parsedNumber) {
        int index = 0;
        char digit;
        parsedNumber = default;
        
        // parse the number from right to left.
        while (index < source.Length && char.IsAsciiDigit(digit = source[index])) { 
            parsedNumber = parsedNumber * 10 + (digit - '0');
            index++;
        }
        return index;
    }
}
