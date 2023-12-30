namespace AdventOfCode._2023;

using Microsoft.CodeAnalysis;

using AdventOfCode.Helpers;

using Point    = Helpers.GridPoint<int>;
using Rectangle = Helpers.GridRectangle<int>;
using CommunityToolkit.HighPerformance;

public class Day03 : AdventBase
{
    Schematic? schematic;

    protected override void InternalOnLoad() {
        this.schematic = Schematic.ParsePlan(Input.Text);
    }

    protected override object InternalPart1() =>
        this.schematic!.Parts.OfType<PartNumber>()
                             .Where(pn => this.schematic.GetNeighbors(pn).Any())
                             .Select(pn => pn.Data).Sum();

    protected override object InternalPart2() =>
        this.schematic!.Parts.OfType<PartSymbol>().Where(ps => ps.Data is '*')
                             .Select(ps => this.schematic!.GetNeighbors(ps).OfType<PartNumber>().Take(3))
                             .Where(e => e.Count() is 2).Select(e => e.Select(pn => pn.Data).Product()).Sum();
}

public class Schematic(Rectangle boundary, IEnumerable<IPart> parts)
{
    public Rectangle Boundaries { get; } = boundary;
    public IReadOnlyDictionary<Point, IPart> PartLocations { get; } 
        = parts?.SelectMany(part => part.Boundaries.ContainedPoints.Select(location => (Location: location, Part: part)))
                .ToImmutableDictionary(item => item.Location, item => item.Part)
               ?? throw new ArgumentNullException(nameof(parts));
    public IEnumerable<IPart> Parts { get; } = parts.ToImmutableArray();

    public IEnumerable<IPart> GetNeighbors(IPart part) {
        ArgumentNullException.ThrowIfNull(part);

        // search box, 1 beyond size in x and y.
        var searchBox = part.Boundaries.Grow(1);

        HashSet<IPart> seenParts = [];

        foreach (var point in searchBox.Border)
            if (PartLocations.TryGetValue(point, out IPart? neighbor) && seenParts.Add(neighbor)) 
                yield return neighbor;
    }

    public static Schematic ParsePlan(string input) {
        ArgumentNullException.ThrowIfNull(input);

        var text = input.AsSpan();
        List<IPart> parts = [];

        int y = 0;
        foreach (var line in text.EnumerateLines()) {
            for (int index = 0; index < line.Length;) { // index advanced manually.
                // advance to next non dot character.
                int hitIndex = line[index..].IndexOfAnyExcept('.');
                if (hitIndex == -1) break; // EoL.

                index += hitIndex;
                Point location = new(index, y);

                switch (line[index]) {
                    case var symbol when PartSymbols.Contains(symbol):
                        parts.Add(new PartSymbol(symbol, new Rectangle(location, location)));
                        index += 1;
                        break;

                    case var digit when char.IsDigit(digit):
                        index += line[index..].ParseDigits(out int number);
                        parts.Add(new PartNumber(number, new Rectangle(location, (index - 1, location.Y))));
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(input), line[index], "Unhandled character in input.");
                }
            }
            y++;
        }

        int width = text.IndexOf('\n');
        return new Schematic((width, y), parts);
    }

    public readonly static IReadOnlySet<char> PartSymbols = "=*+/&#-%$@".ToImmutableHashSet();
}

public readonly record struct PartNumber(int Data, Rectangle Boundaries, Guid Guid) : IPart<int> {
    public PartNumber(int data, Rectangle boundaries) : this(data, boundaries, Guid.NewGuid()) { }
    object IPart.Data => this.Data;
}
public readonly record struct PartSymbol(char Data, Rectangle Boundaries, Guid Guid) : IPart<char> {
    public PartSymbol(char symbol, Rectangle boundaries) : this(symbol, boundaries, Guid.NewGuid()) { }
    object IPart.Data => this.Data;
}

public interface IPart {
    object    Data       { get; }
    Rectangle Boundaries { get; }
    Guid      Guid       { get; }
}

public interface IPart<T> : IPart {
    new T Data { get; }
}