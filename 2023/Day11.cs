using AdventOfCode.Helpers;

namespace AdventOfCode._2023;

using GridPoint = GridPoint<long>;
using Range = Range<long>;

public class Day11 : AdventBase
{
    private GalaxyMap? _galaxyMap;

    protected override void InternalOnLoad() {
        _galaxyMap = GalaxyMapParser.Parse(Input.Text);
    }

    protected override object InternalPart1() => 
        _galaxyMap!.Expand(1).Galaxies.PairCombinations().Select(pair => pair.CalculateManhattanDistance()).Sum();

    protected override object InternalPart2() =>
        _galaxyMap!.Expand(999999).Galaxies.PairCombinations().Select(pair => pair.CalculateManhattanDistance()).Sum();
}

public class GalaxyMap {
    public IEnumerable<Galaxy> Galaxies { get; }
    public IReadOnlyList<SpaceExpansion> SpaceExpansionX { get; }
    public IReadOnlyList<SpaceExpansion> SpaceExpansionY { get; }

    public GalaxyMap(IEnumerable<Galaxy> galaxies, IEnumerable<int> expansionIndexesX, IEnumerable<int> expansionIndexesY) {
        ArgumentNullException.ThrowIfNull(galaxies);
        ArgumentNullException.ThrowIfNull(expansionIndexesX);
        ArgumentNullException.ThrowIfNull(expansionIndexesY);

        SpaceExpansionX = expansionIndexesX.ToSpaceExpansions().ToImmutableArray();
        SpaceExpansionY = expansionIndexesY.ToSpaceExpansions().ToImmutableArray();
        Galaxies = galaxies.ToImmutableArray();
    }

    private GalaxyMap(IEnumerable<Galaxy> galaxies, IEnumerable<SpaceExpansion> expansionX, IEnumerable<SpaceExpansion> expansionY) {
        SpaceExpansionX = expansionX.ToImmutableArray();
        SpaceExpansionY = expansionY.ToImmutableArray();
        Galaxies = galaxies.ToImmutableArray();
    }

    /// <summary>Expands a galaxy map along its <seealso cref="SpaceExpansion"/> joints.</summary>
    /// <param name="multiple">The number of units to expand at each joint.</param>
    /// <returns>A new expanded galaxy.</returns>
    public GalaxyMap Expand(int multiple) {
        List<Galaxy> expandedGalaxies = [];
        foreach (var galaxy in this.Galaxies) {
            int xExpansion = this.SpaceExpansionX.First(expansion => expansion.Contains(galaxy.Location.X)).Index * multiple;
            int yExpansion = this.SpaceExpansionY.First(expansion => expansion.Contains(galaxy.Location.Y)).Index * multiple;

            expandedGalaxies.Add(new Galaxy(galaxy.Location.X + xExpansion, galaxy.Location.Y + yExpansion));
        }
        return new GalaxyMap(expandedGalaxies, this.SpaceExpansionX, this.SpaceExpansionY);
    }
}

public readonly record struct Galaxy(GridPoint Location, Guid Guid) {
    public Galaxy(long x, long y) : this((x, y)) { }
    public Galaxy(GridPoint location) : this(location, Guid.NewGuid()) { }
}

public static class GalaxyHelper {
    public static long CalculateManhattanDistance(this (Galaxy, Galaxy) galaxyPair) =>
        Math.Abs(galaxyPair.Item1.Location.X - galaxyPair.Item2.Location.X) + 
        Math.Abs(galaxyPair.Item1.Location.Y - galaxyPair.Item2.Location.Y);
}

public readonly record struct SpaceExpansion(int Index, Range Range) {
    public long End   { get => this.Range.End; }
    public long Start { get => this.Range.Start; }
    public bool Contains(long value) => Range.Contains(value);
}

public static class SpaceExpansionHelper {
    /// <summary>Transforms a series of expansion indexes into a series of ranges that indicate which coordinates should fall within them.
    /// The first range always starts at 0, and the last range ends at <seealso cref="long.MaxValue"/>.</summary>
    public static IEnumerable<SpaceExpansion> ToSpaceExpansions(this IEnumerable<int> expansionIndexes) {
        int rangeStart = 0, index = 0;
        foreach (var point in expansionIndexes) {
            yield return new SpaceExpansion(index, (rangeStart, point));
            rangeStart = point;
            index++;
        }
        yield return new SpaceExpansion(index, (rangeStart, long.MaxValue));
    }
}

public static class GalaxyMapParser {
    public static GalaxyMap Parse(string input) {
        ArgumentNullException.ThrowIfNull(input);

        var text = input.AsSpan();

        int lineEnd = text.IndexOf('\n');
        int width   = text[..lineEnd].LastIndexOfAnyExcept('\r', '\n');
        int height  = text.Length / lineEnd;

        List<Galaxy> galaxies = [];

        // Initial set of possible expansion indexes. 
        SortedSet<int> expansionIndexX = new(Enumerable.Range(0, width));
        SortedSet<int> expansionIndexY = new(Enumerable.Range(0, height));

        int y = 0;
        foreach (var line in text.EnumerateLines()) {
            int index = 0;

            while (index < line.Length) {
                // advance to next non dot character.
                int hitIndex = line[index..].IndexOfAnyExcept('.');
                if (hitIndex == -1) break; // EoL.

                index += hitIndex;
                char symbol = line[index];

                if (!ValidCharacters.Contains(symbol))
                    throw new ArgumentOutOfRangeException(nameof(input), symbol, "Unhandled character in input.");

                // remove corresponding x and y index, since they have a galaxy and cannot be an expansion index.
                expansionIndexX.Remove(index);
                expansionIndexY.Remove(y);

                galaxies.Add(new(index, y));

                index += 1;
            }
            y++;
        }

        return new GalaxyMap(galaxies, expansionIndexX, expansionIndexY);
    }

    public readonly static IReadOnlySet<char> ValidCharacters = "#".ToImmutableHashSet();
}