namespace AdventOfCodeMax._2023;

using AdventOfCode.Helpers;


using CalibrationGroup = IReadOnlyList<CalibrationValue>;

public class Day01 : AdventBase
{
    IEnumerable<CalibrationGroup>? calibrationValues;

    protected override void InternalOnLoad() {
        this.calibrationValues = CalibrationValueParser.Parse(Input.Text);
    }

    protected override object InternalPart1() =>
        this.calibrationValues!.Select(group => group.First(CalibrationValue.IsCharDigit).Value * 10 + 
                                                group.Last(CalibrationValue.IsCharDigit).Value).Sum();

    protected override object InternalPart2() =>
        this.calibrationValues!.Select(group => group[0].Value * 10 + group[^1].Value).Sum();
}

public readonly record struct CalibrationValue(int Value, CalibrationValue.CalibrationType Type) {
    public static bool IsCharDigit(CalibrationValue value) => value.Type == CalibrationType.CharDigit;

    public enum CalibrationType { CharDigit, NumberWord }
};

public static class CalibrationValueParser {
    public const int MAX_CALIBRATION_GROUP_COUNT = 10;

    public static IEnumerable<CalibrationGroup> Parse(string input) {
        List<CalibrationGroup> groups = new(1000);
        ListSpan<CalibrationValue> calibrationGroup = stackalloc CalibrationValue[MAX_CALIBRATION_GROUP_COUNT];

        foreach (var line in input.AsSpan().EnumerateLines()) {
            for (var index = 0; index < line.Length; index++) {
                switch (line[index]) {
                    case char digit when char.IsAsciiDigit(digit):
                        calibrationGroup.Add(new CalibrationValue(digit - '0', CalibrationValue.CalibrationType.CharDigit));
                        break;

                    default:    // non-digit case
                        foreach (var (word, value) in NumberWords) {
                            if (line[index..].StartsWith(word)) {
                                // advance the index to the position of the last letter to handle overlapping words.
                                index += word.Length - 2;   // -2 to accout for the for loop increment.
                                calibrationGroup.Add(value);
                            }
                        }
                        break;
                }
            }
            groups.Add(calibrationGroup.AsSpan().ToImmutableArray());
            calibrationGroup.Clear();
        }

        return groups.ToImmutableArray();
    }

    private static readonly IEnumerable<(string Word, CalibrationValue Value)> NumberWords = new []{
        ("one",   new CalibrationValue(1, CalibrationValue.CalibrationType.NumberWord)),
        ("two",   new CalibrationValue(2, CalibrationValue.CalibrationType.NumberWord)),
        ("three", new CalibrationValue(3, CalibrationValue.CalibrationType.NumberWord)),
        ("four",  new CalibrationValue(4, CalibrationValue.CalibrationType.NumberWord)),
        ("five",  new CalibrationValue(5, CalibrationValue.CalibrationType.NumberWord)),
        ("six",   new CalibrationValue(6, CalibrationValue.CalibrationType.NumberWord)),
        ("seven", new CalibrationValue(7, CalibrationValue.CalibrationType.NumberWord)),
        ("eight", new CalibrationValue(8, CalibrationValue.CalibrationType.NumberWord)),
        ("nine",  new CalibrationValue(9, CalibrationValue.CalibrationType.NumberWord)),
    }.ToImmutableArray();
}