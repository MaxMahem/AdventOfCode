namespace AdventOfCode._2023;

using System.Drawing;

using AdventOfCode.Helpers;

public class Day03 : AdventBase
{
    Schematic? _schematic;

    protected override void InternalOnLoad() {
        _schematic = Schematic.ParsePlan(Input.Text);
    }

    protected override object InternalPart1() =>
        _schematic!.Parts.OfType<PartNumber>()
                         .Where(pn => _schematic.GetNeighbors(pn).Any())
                         .Select(pn => pn.Data).Sum();

    protected override object InternalPart2() =>
        _schematic!.Parts.OfType<PartSymbol>().Where(ps => ps.Data is '*')
                         .Select(ps => _schematic!.GetNeighbors(ps).OfType<PartNumber>().Take(3))
                         .Where(e => e.Count() is 2).Select(e => e.Select(pn => pn.Data).Product()).Sum();
}

public class Schematic(int width, int height, IEnumerable<IPart> parts)
{
    public readonly static IReadOnlySet<char> PartSymbols = "=*+/&#-%$@".ToImmutableHashSet();
    public int Width  { get; } = width > 0  ? width  : throw new ArgumentException("Must be greater than 0.", nameof(width));
    public int Height { get; } = height > 0 ? height : throw new ArgumentException("Must be greater than 0.", nameof(height));
    public IReadOnlyDictionary<Point, IPart> PartLocations { get; } 
        = ParsePartList(parts ?? throw new ArgumentNullException(nameof(parts))).ToImmutableDictionary();
    public IEnumerable<IPart> Parts { get; } = parts.ToImmutableArray();

    private static Dictionary<Point, IPart> ParsePartList(IEnumerable<IPart> parts) {
        Dictionary<Point, IPart> partDictionary = [];

        // note that a part will get inserted multiple times if it exists in multiple locations.
        foreach (IPart part in parts) {
            int endX = part.Location.X + part.Length;
            for (int x = part.Location.X; x < endX; x++) {
                partDictionary.Add(new Point(x, part.Location.Y), part);
            }
        }
        return partDictionary;
    }

    public static Schematic ParsePlan(string input) {
        ArgumentNullException.ThrowIfNull(input);

        var text = input.AsSpan();
        List<IPart> parts = [];

        int y = 0;
        foreach (var line in text.EnumerateLines()) {
            int index = 0;
            while (index < line.Length) {
                // advance to next non dot character.
                int hitIndex = line[index..].IndexOfAnyExcept('.');
                if (hitIndex == -1) break; // EoL.

                index += hitIndex;
                char currentChar = line[index];
                Point location = new(index, y);

                switch (currentChar) {
                    case var symbol when PartSymbols.Contains(symbol):
                        parts.Add(new PartSymbol(symbol, location));
                        index += 1;
                        break;

                    case var digit when char.IsDigit(digit):
                        int number = 0;
                        do {    // parse the number from right to left.
                            number = number * 10 + (digit - '0');
                            index++;
                        } while (index < line.Length && char.IsDigit(digit = line[index]));
                        parts.Add(new PartNumber(number, location));
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(input), currentChar, "Unhandled character in input.");
                }

            }
            y++;
        }

        int width = text.IndexOf('\n');
        return new Schematic(width, y, parts);
    }

    public IEnumerable<IPart> GetNeighbors(IPart part) {
        ArgumentNullException.ThrowIfNull(part);

        // search box, 1 beyond size in x and y. TL = top left, br = bottom right.
        (Point point, int length) = (part.Location, part.Length);
        (int X, int Y) tl = (point.X - 1,      point.Y - 1);
        (int X, int Y) br = (point.X + length, point.Y + 1);
        IPart? neighbor;

        HashSet<IPart> seenParts = [];

        // check top and bottom row. 
        for (int x = tl.X; x <= br.X; x++) {
            if (PartLocations.TryGetValue(new(x, tl.Y), out neighbor) && seenParts.Add(neighbor)) yield return neighbor;
            if (PartLocations.TryGetValue(new(x, br.Y), out neighbor) && seenParts.Add(neighbor)) yield return neighbor;
        }

        // check middle sides (can never be duplicates).
        if (PartLocations.TryGetValue(new(tl.X, point.Y), out neighbor)) yield return neighbor;
        if (PartLocations.TryGetValue(new(br.X, point.Y), out neighbor)) yield return neighbor;
    }
}

public abstract record AbstractPart<T>(T Data, Point Location, Guid Guid) : IPart<T> where T : notnull
{
    object IPart.Data => this.Data;
    public abstract int Length { get; }
}
public record PartNumber(int Data, Point Location, Guid Guid) : AbstractPart<int>(Data, Location, Guid)
{
    public PartNumber(int data, Point location) : this(data, location, Guid.NewGuid()) { }
    public override int Length => Data.DigitCount();
}
public record PartSymbol(char Data, Point Location, Guid Guid) : AbstractPart<char>(Data, Location, Guid)
{
    public PartSymbol(char symbol, Point location) : this(symbol, location, Guid.NewGuid()) { }
    public override int Length => 1;
}

public interface IPart
{
    object Data { get; }
    Point Location { get; }
    int Length { get; }
    Guid Guid { get; }
}

public interface IPart<T> : IPart
{
    new T Data { get; }
}