namespace AdventOfCode._2023;

using AdventOfCode.Helpers;
using GridPoint = Helpers.GridPoint<int>;

public class Day10 : AdventBase {
    PipeSchematic? _pipeSchematic;

    protected override void InternalOnLoad() {
        _pipeSchematic = PipeSchematicParser.ParsePlan(Input.Text);
    }

    protected override object InternalPart1() => _pipeSchematic!.Loop.Count() / 2;

    protected override object InternalPart2() {
        var loopPoints = _pipeSchematic!.Loop.Select(pipe => pipe.Location).ToImmutableArray();
        var loopArea   = loopPoints.CalculateArea();
        
        // Picks Theorem.
        var interiorPoints = loopArea - loopPoints.Length / 2 + 1;

        return (int)interiorPoints;
    }
}

public static class PointListHelper {
    /// <summary>Calculates the area of a polygon using the Shoesting method. 
    /// This assumes the list presents the points in order.</summary>
    /// <param name="points">The list of points, must be in order.</param>
    public static double CalculateArea(this IReadOnlyList<GridPoint> points) {
        int pointCount = points.Count;
        double area = 0;

        for (int index = 0; index < pointCount; index++) {
            int nextIndex = (index + 1) % pointCount;
            area += (points[index].X * points[nextIndex].Y) - (points[nextIndex].X * points[index].Y);
        }

        area = 0.5 * Math.Abs(area);
        return area;
    }
}

public class PipeSchematic {
    public GridRectangle<int> Boundary { get; }
    public IReadOnlyDictionary<GridPoint, Pipe> PipeLocations { get; }
    public Pipe StartPipe { get; }
    public IEnumerable<Pipe> Loop => _loop.Value;

    public PipeSchematic(int width, int height, IEnumerable<Pipe> pipes, Pipe startPipe) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentNullException.ThrowIfNull(pipes);
        ArgumentNullException.ThrowIfNull(startPipe);

        Boundary      = new(width, height, 0, 0);
        PipeLocations = pipes.ToImmutableDictionary(pipe => pipe.Location);
        StartPipe     = startPipe;

        _loop = new(() => FindLoop(startPipe));
    }

    private IEnumerable<Pipe> FindLoop(Pipe startingPipe) {
        GridPoint lastLocation    = startingPipe.Location;
        GridPoint currentLocation = startingPipe.Connected.PointA;
        List<Pipe> loopingPipes = [startingPipe];

        do {
            loopingPipes.Add(this.PipeLocations[currentLocation]);
            GridPoint nextLocation = Navigate(currentLocation, lastLocation);
            (currentLocation, lastLocation) = (nextLocation, currentLocation);
        } while (currentLocation != startingPipe.Location);

        return loopingPipes.ToImmutableArray();
    }

    private GridPoint Navigate(GridPoint from, GridPoint last) => 
        this.PipeLocations[from].Connected.GetCorresponding(last);

    private readonly Lazy<IEnumerable<Pipe>> _loop;
}

public readonly record struct ConnectedPoints(GridPoint PointA, GridPoint PointB) {
    public ConnectedPoints(int xa, int ya, int xb, int yb) : this((xa, ya), (xb, yb)) { }

    public GridPoint GetCorresponding(GridPoint point) {
        if (point == PointA) return PointB;
        if (point == PointB) return PointA;
        throw new ArgumentException("No corresponding point.", nameof(point));
    }
    public bool Contains(GridPoint point) => point == PointA || point == PointB;

    public static ConnectedPoints GetConnected(GridPoint point, Pipe.PipeType type) => type switch {
        Pipe.PipeType.NS => new(point.X + 0, point.Y - 1, point.X + 0, point.Y + 1),
        Pipe.PipeType.EW => new(point.X + 1, point.Y + 0, point.X - 1, point.Y + 0),
        Pipe.PipeType.NE => new(point.X + 0, point.Y - 1, point.X + 1, point.Y + 0),
        Pipe.PipeType.NW => new(point.X + 0, point.Y - 1, point.X - 1, point.Y + 0),
        Pipe.PipeType.SW => new(point.X + 0, point.Y + 1, point.X - 1, point.Y + 0),
        Pipe.PipeType.SE => new(point.X + 0, point.Y + 1, point.X + 1, point.Y + 0),
        _ => throw new ArgumentException("Invalid pipe type.", nameof(type))
    };

    public Pipe.PipeType GetPipeType(GridPoint connectedPoint) {
        (int X, int Y) pointADifference = (connectedPoint.X - PointA.X, connectedPoint.Y - PointA.Y);
        (int X, int Y) pointBDifference = (connectedPoint.X - PointB.X, connectedPoint.Y - PointB.Y);
        return (pointADifference, pointBDifference) switch {
            ((+0, +1), (+0, -1)) => Pipe.PipeType.NS,
            ((-1, +0), (+1, +0)) => Pipe.PipeType.EW,
            ((+0, +1), (-1, +0)) => Pipe.PipeType.NE,
            ((+0, +1), (+1, +0)) => Pipe.PipeType.NW,
            ((+0, -1), (+1, +0)) => Pipe.PipeType.SW,
            ((-1, +0), (+0, -1)) => Pipe.PipeType.SE,
            _ => throw new InvalidOperationException("Invalid pipe connection.")
        };
    }
};

public readonly record struct Pipe(GridPoint Location, Pipe.PipeType Type, ConnectedPoints Connected) : IGridSymbol {
    public char Symbol => ToBoxASCII(Type);
    public Pipe(GridPoint location, char symbol) 
        : this(location, ParseSymbol(symbol), ConnectedPoints.GetConnected(location, ParseSymbol(symbol))) { }

    /// <summary>Special method for creating the start pipe, requiring all the other pipes to be defined.</summary>
    public static Pipe? CreateStartPipe(GridPoint location, IEnumerable<Pipe> pipes) {
        ConnectedPoints? connections = GetConnected(pipes, location);
        if (connections is null) return null;

        var pipeType = connections.Value.GetPipeType(location);
        return new(location, pipeType, connections.Value);
    }

    public enum PipeType { NS, EW, NE, NW, SW, SE }

    public GridPoint Navigate(GridPoint from) => this.Connected.GetCorresponding(from);

    public static PipeType ParseSymbol(char symbol) => symbol switch {
        '|' => PipeType.NS,
        '-' => PipeType.EW,
        'L' => PipeType.NE,
        'J' => PipeType.NW,
        '7' => PipeType.SW,
        'F' => PipeType.SE,
        _ => throw new ArgumentException("Invalid pipe symbol.", nameof(symbol)),
    };

    /// <summary>Parses a list of pipes to find connections to a given point.</summary>
    /// <returns>The connected points, or null if no connections exists.</returns>
    public static ConnectedPoints? GetConnected(IEnumerable<Pipe> pipes, GridPoint point) {
        var connectedPoints = pipes.Where(pipe => pipe.Connected.Contains(point)).Select(pipe => pipe.Location).ToArray();
        if (connectedPoints.Length != 2) return null;

        return new(connectedPoints[0], connectedPoints[1]);
    }

    public override string ToString() => this.Symbol.ToString();

    public static readonly ImmutableHashSet<char> PipeSymbols = [.. "|-LJ7FS"];
    private static char ToBoxASCII(PipeType type) => type switch {
        PipeType.NS => '║',
        PipeType.EW => '═',
        PipeType.NE => '╚',
        PipeType.NW => '╝',
        PipeType.SW => '╗',
        PipeType.SE => '╔',
        _ => throw new InvalidOperationException("Invalid pipe type."),
    };
}

public static class PipeSchematicParser {
    public static PipeSchematic ParsePlan(string input) {
        ArgumentNullException.ThrowIfNull(input);

        var text = input.AsSpan();
        List<Pipe> pipes = [];
        GridPoint startPoint = default;

        int y = 0;
        foreach (var line in text.EnumerateLines()) {
            int index = 0;
            while (index < line.Length) {
                // advance to next non dot character.
                int hitIndex = line[index..].IndexOfAnyExcept('.');
                if (hitIndex == -1) break; // EoL.

                index += hitIndex;
                char symbol = line[index];

                if (!Pipe.PipeSymbols.Contains(symbol))
                    throw new ArgumentOutOfRangeException(nameof(input), symbol, "Unhandled character in input.");

                GridPoint location = new(index, y);

                // start symbol cannot be created inline, mark its position and continue.
                if (symbol == 'S')
                    startPoint = location;
                else
                    pipes.Add(new Pipe(location, symbol));

                index += 1;
            }
            y++;
        }

        // build the starting pipe given the information about all the other pipes.
        Pipe startPipe = Pipe.CreateStartPipe(startPoint, pipes) ?? throw new ArgumentException("Invalid start location.");
        pipes.Add(startPipe);

        int width = text.IndexOf('\n');
        return new PipeSchematic(width, y, pipes, startPipe);
    }
}