namespace AdventOfCode._2023;

using AdventOfCode.Helpers;

using GridPoint     = Helpers.GridPoint<int>;
using GridDirection = Helpers.GridDirection<int>;
using Reflections = IReadOnlyDictionary<Helpers.GridDirection<int>, IEnumerable<Helpers.GridDirection<int>>>;

public class Day16 : AdventBase
{
    MirrorMap? _mirrorMap;

    protected override void InternalOnLoad() {
        _mirrorMap = MirrorMapParser.Parse(Input.Text);
    }

    protected override object InternalPart1() {
        return 0;
    }

    protected override object InternalPart2() {
        return 0;
    }

}

public class MirrorMap(IEnumerable<Mirror> mirrors, GridRectangle<int> boundary) {
    private readonly IReadOnlyDictionary<GridPoint, Mirror> _mirrorLookup 
        = mirrors.ToImmutableDictionary(mirror => mirror.Location);

    public IEnumerable<Mirror> Mirrors { get; } = mirrors;
    public GridRectangle<int> Boundary { get; } = boundary;

    public int EnergizeGrid(Beam beam) {
        List<Beam>     beamList        = [beam];
        HashSet<Beam>  beamHistory     = [];
        HashSet<GridPoint> energizedPoints = [];
        do {
            for (int beamIndex = beamList.Count - 1; beamIndex >= 0; beamIndex--) {
                var newBeam = beam.Step();

                if (!Boundary.ContainsPoint(beam.Location)) { 
                    beamList.RemoveAt(beamIndex);
                    continue;
                }

                if (_mirrorLookup.TryGetValue(newBeam.Location, out Mirror mirror)) {
                    beamList.RemoveAt(beamIndex);

                    // beams can only loop at mirrors, so only add reflections that have not been seen.
                    beamList.AddRange(mirror.Reflect(beam).Where(beamHistory.Add));
                } else {
                    beamList[beamIndex] = newBeam;
                }
            }            
        } while (beamList.Count > 0);
        return beamList.Count;
    }
}

public record struct Beam(GridPoint Location, GridDirection Direction) {
    public Beam Step() => this with { Location = this.Location + this.Direction };
}

public interface IGrid {
    public enum Direction {
        North,
        South,
        East,
        West,
    }
}

public readonly record struct Mirror(GridPoint Location, char Symbol, Reflections Reflections) : IGridSymbol {
    public static Mirror Create(char symbol, GridPoint location) => new(location, symbol, MirrorReflections[symbol]);

    public IEnumerable<Beam> Reflect(Beam beam) {
        var newDirections = Reflections[beam.Direction];
        foreach (var direction in newDirections) {
            yield return direction switch {
                (0,0) => beam,
                _     => beam with { Direction =  direction },
            };
        }
    }

    private readonly static IReadOnlyDictionary<char, Reflections> MirrorReflections
        = new Dictionary<char, Reflections>() {
            { '|',  new Dictionary<GridDirection, IEnumerable<GridDirection>>() {
                { GridDirection.North, [ GridDirection.None ] },
                { GridDirection.South, [ GridDirection.None ] },
                { GridDirection.East,  [ GridDirection.North, GridDirection.South ] },
                { GridDirection.West,  [ GridDirection.North, GridDirection.South ] },
            }.ToImmutableDictionary() },
            { '-',  new Dictionary<GridDirection, IEnumerable<GridDirection>>() {
                { GridDirection.North, [ GridDirection.East,  GridDirection.West  ] },
                { GridDirection.South, [ GridDirection.East,  GridDirection.West  ] },
                { GridDirection.East,  [ GridDirection.None ] },
                { GridDirection.West,  [ GridDirection.None ] },
            }.ToImmutableDictionary() },
            { '\\', new Dictionary<GridDirection, IEnumerable<GridDirection>>() {
                { GridDirection.North, [ GridDirection.West  ] },
                { GridDirection.South, [ GridDirection.East  ] },
                { GridDirection.East,  [ GridDirection.North ] },
                { GridDirection.West,  [ GridDirection.South ] },
            }.ToImmutableDictionary() },
            { '/',  new Dictionary<GridDirection, IEnumerable<GridDirection>>() {
                { GridDirection.North, [ GridDirection.East  ] },
                { GridDirection.South, [ GridDirection.West  ] },
                { GridDirection.East,  [ GridDirection.South ] },
                { GridDirection.West,  [ GridDirection.North ] },
            }.ToImmutableDictionary() },
    }.ToImmutableDictionary();
}

public static class MirrorMapParser {
    public static MirrorMap Parse(string input) {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var text = input.AsSpan();
        int lineEnd = input.IndexOf('\n');
        int width = text[..lineEnd].LastIndexOfAnyExcept('\r', '\n') + 1;

        List<Mirror> mirrors = [];

        int y = 0;
        foreach (var line in text.EnumerateLines()) {
            int index = 0;

            while (index < line.Length) {
                // advance to next non dot character.
                int hitIndex = line[index..].IndexOfAnyExcept('.');
                if (hitIndex == -1) break; // EoL.

                index += hitIndex;
                char symbol = line[index];

                GridPoint location = new(index, y);
                mirrors.Add(Mirror.Create(symbol, location));

                index += 1;
            }
            if (line.Length > 0) y++;
        }

        return new MirrorMap(mirrors, (width, y));
    }
}