namespace AdventOfCode._2023;

using System.Drawing;

public class Day10 : AdventBase
{
    PipeSchematic? _pipeSchematic;

    protected override void InternalOnLoad() {
        _pipeSchematic = PipeSchematicParser.ParsePlan(Input.Text);
    }

    protected override object InternalPart1() => _pipeSchematic!.Loop.Count() / 2;

    protected override object InternalPart2() {
        var loopPoints = _pipeSchematic!.Loop.Select(pipe => pipe.Location).ToList();
        var loopArea   = loopPoints.CalculateArea();
        
        // Picks Theorem.
        var interiorPoints = loopArea - loopPoints.Count / 2 + 1;

        return (int)interiorPoints;
    }
}

public static class PointListHelper {
    /// <summary>Calculates the area ofa polygon using the Shoesting method. 
    /// This assumes the list presents the points in order.</summary>
    /// <param name="points">The list of points, must be in order.</param>
    public static double CalculateArea(this IReadOnlyList<Point> points) {
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
    public int Width  { get; }
    public int Height { get; }
    public IReadOnlyDictionary<Point, Pipe> PipeLocations { get; }
    public Pipe StartPipe { get; }
    public IEnumerable<Pipe> Loop => _loop.Value;

    public PipeSchematic(int width, int height, IEnumerable<Pipe> pipes, Pipe startPipe) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentNullException.ThrowIfNull(pipes);
        ArgumentNullException.ThrowIfNull(startPipe);

        (Width, Height) = (width, height);
        PipeLocations = pipes.ToDictionary(pipe => pipe.Location);
        StartPipe = startPipe;

        _loop = new(() => FindLoop(startPipe));
    }

    private IEnumerable<Pipe> FindLoop(Pipe startingPipe) {
        Point lastLocation    = startingPipe.Location;
        Point currentLocation = startingPipe.Connected.PointA;
        List<Pipe> loopingPipes = [startingPipe];

        do {
            loopingPipes.Add(this.PipeLocations[currentLocation]);
            Point nextLocation = Navigate(currentLocation, lastLocation);
            (currentLocation, lastLocation) = (nextLocation, currentLocation);
        } while (currentLocation != startingPipe.Location);

        return loopingPipes.ToImmutableArray();
    }

    private Point Navigate(Point from, Point last) => 
        this.PipeLocations[from].Connected.GetCorresponding(last);

    private readonly Lazy<IEnumerable<Pipe>> _loop;
}

public readonly record struct ConnectedPoints(Point PointA, Point PointB) {
    public ConnectedPoints(int xa, int ya, int xb, int yb) : this(new Point(xa, ya), new Point(xb, yb)) { }

    public Point GetCorresponding(Point point) {
        if (point == PointA) return PointB;
        if (point == PointB) return PointA;
        throw new ArgumentException("No corresponding point.", nameof(point));
    }
    public bool Contains(Point point) => point == PointA || point == PointB;

    public static ConnectedPoints GetConnected(Point point, Pipe.PipeType type) => type switch {
        Pipe.PipeType.NS => new(point.X + 0, point.Y - 1, point.X + 0, point.Y + 1),
        Pipe.PipeType.EW => new(point.X + 1, point.Y + 0, point.X - 1, point.Y + 0),
        Pipe.PipeType.NE => new(point.X + 0, point.Y - 1, point.X + 1, point.Y + 0),
        Pipe.PipeType.NW => new(point.X + 0, point.Y - 1, point.X - 1, point.Y + 0),
        Pipe.PipeType.SW => new(point.X + 0, point.Y + 1, point.X - 1, point.Y + 0),
        Pipe.PipeType.SE => new(point.X + 0, point.Y + 1, point.X + 1, point.Y + 0),
        _ => throw new ArgumentException("Invalid pipe type.", nameof(type))
    };

    public Pipe.PipeType GetPipeType(Point connectedPoint) {
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

public readonly record struct Pipe(Point Location, Pipe.PipeType Type, ConnectedPoints Connected) {
    public Pipe(Point location, char symbol) 
        : this(location, ParseSymbol(symbol), ConnectedPoints.GetConnected(location, ParseSymbol(symbol))) { }

    /// <summary>Special method for creating the start pipe, requiring all the other pipes to be defined.</summary>
    public static Pipe? CreateStartPipe(Point location, IEnumerable<Pipe> pipes) {
        ConnectedPoints? connections = GetConnected(pipes, location);
        if (connections is null) return null;

        var pipeType = connections.Value.GetPipeType(location);
        return new(location, pipeType, connections.Value);
    }

    public enum PipeType { NS, EW, NE, NW, SW, SE, Start, }

    public Point Navigate(Point from) => this.Connected.GetCorresponding(from);

    public static PipeType ParseSymbol(char symbol) => symbol switch {
        '|' => PipeType.NS,
        '-' => PipeType.EW,
        'L' => PipeType.NE,
        'J' => PipeType.NW,
        '7' => PipeType.SW,
        'F' => PipeType.SE,
        'S' => PipeType.Start,
        _ => throw new ArgumentException("Invalid pipe symbol.", nameof(symbol)),
    };

    /// <summary>Parses a list of pipes to find connections to a given point.</summary>
    /// <returns>The connected points, or null if no connections exists.</returns>
    public static ConnectedPoints? GetConnected(IEnumerable<Pipe> pipes, Point point) {
        var connectedPoints = pipes.Where(pipe => pipe.Connected.Contains(point)).Select(pipe => pipe.Location).ToArray();
        if (connectedPoints.Length != 2) return null;

        return new(connectedPoints[0], connectedPoints[1]);
    }

    public static readonly ImmutableHashSet<char> PipeSymbols = [.. "|-LJ7FS"];
}

public static class PipeSchematicParser
{
    public static PipeSchematic ParsePlan(string input) {
        ArgumentNullException.ThrowIfNull(input);

        var text = input.AsSpan();
        List<Pipe> pipes = [];
        Point startPoint = default;

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

                Point location = new(index, y);

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