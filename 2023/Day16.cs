namespace AdventOfCode._2023;

using AdventOfCode.Helpers;

using Point     = Helpers.Point<int>;
using Direction = Helpers.Direction<int>;
using Reflections = IReadOnlyDictionary<Helpers.Direction<int>, IEnumerable<Helpers.Direction<int>>>;

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

public class MirrorMap(IEnumerable<Mirror> mirrors, Rectangle<int> boundary) {
    private readonly IReadOnlyDictionary<Point, Mirror> _mirrorLookup 
        = mirrors.ToImmutableDictionary(mirror => mirror.Location);

    public IEnumerable<Mirror> Mirrors { get; } = mirrors;
    public Rectangle<int> Boundary { get; } = boundary;

    public int EnergizeGrid(Beam beam) {
        List<Beam>     beamList        = [beam];
        HashSet<Beam>  beamHistory     = [];
        HashSet<Point> energizedPoints = [];
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

public record struct Beam(Point Location, Direction Direction) {
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

public readonly record struct Mirror(Point Location, char Symbol, Reflections Reflections) : IGridSymbol {
    public static Mirror Create(char symbol, Point location) => new(location, symbol, MirrorReflections[symbol]);

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
            { '|',  new Dictionary<Direction, IEnumerable<Direction>>() {
                { Direction.North, [ Direction.None ] },
                { Direction.South, [ Direction.None ] },
                { Direction.East,  [ Direction.North, Direction.South ] },
                { Direction.West,  [ Direction.North, Direction.South ] },
            }.ToImmutableDictionary() },
            { '-',  new Dictionary<Direction, IEnumerable<Direction>>() {
                { Direction.North, [ Direction.East,  Direction.West  ] },
                { Direction.South, [ Direction.East,  Direction.West  ] },
                { Direction.East,  [ Direction.None ] },
                { Direction.West,  [ Direction.None ] },
            }.ToImmutableDictionary() },
            { '\\', new Dictionary<Direction, IEnumerable<Direction>>() {
                { Direction.North, [ Direction.West  ] },
                { Direction.South, [ Direction.East  ] },
                { Direction.East,  [ Direction.North ] },
                { Direction.West,  [ Direction.South ] },
            }.ToImmutableDictionary() },
            { '/',  new Dictionary<Direction, IEnumerable<Direction>>() {
                { Direction.North, [ Direction.East  ] },
                { Direction.South, [ Direction.West  ] },
                { Direction.East,  [ Direction.South ] },
                { Direction.West,  [ Direction.North ] },
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

                Point location = new(index, y);
                mirrors.Add(Mirror.Create(symbol, location));

                index += 1;
            }
            if (line.Length > 0) y++;
        }

        return new MirrorMap(mirrors, (width, y));
    }
}