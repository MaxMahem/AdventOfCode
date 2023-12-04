namespace AdventOfCode._2023;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;

using AdventOfCode.Helpers;
using AdventOfCodeSupport;

public class Day03 : AdventBase
{
    Schematic? _schematic;

    protected override void InternalOnLoad() {
        _schematic = Schematic.ParsePlan(Input.Text);
    }

    protected override object InternalPart1() =>
        _schematic!.Parts.OfType<PartNumber>()
                         .Where(pn => _schematic.GetNeighbors(pn).Any())
                         .Select(pn => pn.Number).Sum();

    protected override object InternalPart2() =>
        _schematic!.Parts.OfType<PartSymbol>().Where(ps => ps.Symbol is '*')
                         .Select(ps => _schematic!.GetNeighbors(ps).OfType<PartNumber>().Take(3))
                         .Where(e => e.Count() is 2).Select(e => e.Select(pn => pn.Number).Product()).Sum();
}

public record PartNumber(int Number,  Point Location, Guid Guid) : IPart {
    public PartNumber(int number, Point location) : this(number, location, Guid.NewGuid()) { }
    public int Length => Number.Digits();
}
public record PartSymbol(char Symbol, Point Location, Guid Guid) : IPart {
    public PartSymbol(char symbol, Point location) : this(symbol, location, Guid.NewGuid()) { }
    public int Length => 1;
}

public interface IPart {
    Point Location { get; }
    int Length { get; }
    Guid Guid { get; }
}

public class Schematic
{
    public int Width { get; }
    public int Height { get; }
    public IReadOnlyDictionary<Point, IPart> PartLocations { get; }
    public IEnumerable<IPart> Parts { get; }

    public Schematic(int width, int height, IEnumerable<IPart> parts) {
        this.Width  = width  > 0 ? width  : throw new ArgumentException("Must be greater than 0.", nameof(width));
        this.Height = height > 0 ? height : throw new ArgumentException("Must be greater than 0.", nameof(height));
        ArgumentNullException.ThrowIfNull(parts, nameof(parts));
        this.Parts = parts.ToImmutableArray();
        this.PartLocations = ParsePartList(parts).ToImmutableDictionary();
    }

    public static IDictionary<Point, IPart> ParsePartList(IEnumerable<IPart> parts) {
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
        ArgumentNullException.ThrowIfNull(input, nameof(input));

        var text = input.AsSpan();
        List<IPart> parts = [];

        int y = 0;
        foreach (var line in text.EnumerateLines()) {
            for (int index = 0; index < line.Length; index++) {
                var window = line[index..];

                // find the first non '.'
                int hitIndex = line[index..].IndexOfAnyExcept('.');
                if (hitIndex is -1) break; // EoL.

                // handle symbols.
                char c;
                Point location = new(index + hitIndex, y);
                if (!char.IsDigit(c = window[hitIndex])) {
                    parts.Add(new PartSymbol(c, location));

                    index += hitIndex; // advance the for loop and window.
                    continue;
                }

                // handle digits.
                int number = 0, digitLength = 0;
                while ((hitIndex + digitLength) < window.Length && char.IsAsciiDigit(c = window[hitIndex + digitLength])) {
                    number = number * 10 + (c - '0');
                    digitLength++;
                }
                parts.Add(new PartNumber(number, location));
                index += hitIndex + digitLength - 1; // advance the for loop and window.
            }
            y++;
        }
        int width = text.IndexOf('\n');
        return new Schematic(width, y, parts);
    }

    public IEnumerable<IPart> GetNeighbors(IPart part) {
        ArgumentNullException.ThrowIfNull(part, nameof(part));

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

        // check middle sides.
        if (PartLocations.TryGetValue(new(tl.X, point.Y), out neighbor) && seenParts.Add(neighbor)) yield return neighbor;
        if (PartLocations.TryGetValue(new(br.X, point.Y), out neighbor) && seenParts.Add(neighbor)) yield return neighbor;
    }
}