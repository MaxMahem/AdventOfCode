namespace AdventOfCode._2022;

public class Day01 : AdventBase
{
    IEnumerable<SnackBag>? _snacks;

    protected override void InternalOnLoad() {
        _snacks = SnackBagParser.Parse(Input.Text);
    }

    protected override object InternalPart1() => _snacks!.MaxBy(sb => sb.TotalCalories)!.TotalCalories;

    protected override object InternalPart2() => _snacks!.OrderByDescending(sb => sb.TotalCalories).Take(3).Sum(sb => sb.TotalCalories);
}

public class SnackBag(IEnumerable<int> snacks) {
    public IEnumerable<int> Snacks { get; } = snacks?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(snacks));

    public int TotalCalories { get; } = snacks.Sum();
}

internal static class SnackBagParser {
    public static IEnumerable<SnackBag> Parse(string text) {
        List<SnackBag> snackBags = [];
        List<int> snacks = [];
        foreach(var line in text.AsSpan().EnumerateLines()) {
            if (line.IsWhiteSpace()) {
                snackBags.Add(new SnackBag(snacks));
                snacks = [];
                continue;
            }
            snacks.Add(int.Parse(line));
        }
        return snackBags;
    }
}