namespace AdventOfCodeMax._2023;

using System.Text.RegularExpressions;
using CalibrationValueGroup = IReadOnlyList<CalibrationValue>;

public class Day01 : AdventBase
{
    IEnumerable<CalibrationValueGroup>? _calibrationValues;

    protected override void InternalOnLoad() {
        _calibrationValues = CalibrationValueParser.Parse(Input.Text);
    }

    protected override object InternalPart1() =>
        _calibrationValues!.Select(group => group.First(value => value.IsNumeric).Digit * 10 + 
                                            group.Last(value => value.IsNumeric).Digit).Sum();

    protected override object InternalPart2() =>
        _calibrationValues!.Select(group => group[0].Digit * 10 + group[^1].Digit).Sum();
}

public readonly record struct CalibrationValue(int Digit, bool IsNumeric);

public static class CalibrationValueParser {
    public static IEnumerable<CalibrationValueGroup> Parse(string input) {
        List<CalibrationValueGroup> groups = new(1000);

        foreach (var line in input.AsSpan().EnumerateLines()) {
            var group = new List<CalibrationValue>(10);
            for (var index = 0; index < line.Length; index++) {
                // check digit case
                char character = line[index];
                if (char.IsAsciiDigit(character))
                    group.Add(new CalibrationValue(character - '0', IsNumeric: true));

                // check words
                foreach (var (word, number) in NumberWords) {
                    if (line[index..].StartsWith(word)) {
                        // advance the index to the end of the matched word - 1 in order to handle overlapping words.
                        index += word.Length - 2;
                        group.Add(new CalibrationValue(number, IsNumeric: false));
                    }
                }
            }
            groups.Add(group.ToImmutableArray());
        }

        return groups.ToImmutableArray();
    }

    private static readonly IEnumerable<(string Word, int Number)> NumberWords = new []{
        ("one",   1),
        ("two",   2),
        ("three", 3),
        ("four",  4),
        ("five",  5),
        ("six",   6),
        ("seven", 7),
        ("eight", 8),
        ("nine",  9),
    }.ToImmutableArray();
}