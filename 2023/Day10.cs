namespace AdventOfCode._2023;

using AdventOfCode.Helpers;
using GridPoint = Helpers.GridPoint<int>;
using Direction = Helpers.GridDirection<int>;
using System.Drawing;

public class Day10 : AdventBase {
    PipeSchematic? _pipeSchematic;

    protected override void InternalOnLoad() {
        _pipeSchematic = PipeSchematicParser.ParsePlan(Input.Text);
    }

    protected override object InternalPart1() => _pipeSchematic!.Loop.Points.Count() / 2;
    protected override object InternalPart2() => _pipeSchematic!.Loop.CountInteriorGridPoints();
}

public class PipeSchematic {
    public GridRectangle<int> Boundary { get; }
    public IReadOnlyDictionary<GridPoint, Pipe> PipeLocations { get; }
    public Pipe StartPipe { get; }
    public GridPolygon<Pipe> Loop => loop.Value;

    public PipeSchematic(GridRectangle<int> boundary, IEnumerable<Pipe> pipes, Pipe startPipe) {
        ArgumentNullException.ThrowIfNull(pipes);
        ArgumentNullException.ThrowIfNull(startPipe);

        Boundary      = boundary;
        PipeLocations = pipes.ToImmutableDictionary(pipe => pipe.Location);
        StartPipe     = startPipe;
        this.loop     = new(() => FindLoop(startPipe));
    }

    private GridPolygon<Pipe> FindLoop(Pipe startingPipe) {
        (Pipe currentPipe, Pipe lastPipe) = (this.PipeLocations[startingPipe.Connections.PointA], startingPipe);
        List<Pipe> loopingPipes = [startingPipe];

        do {
            loopingPipes.Add(currentPipe);
            (currentPipe, lastPipe) = (this.PipeLocations[currentPipe.GetCorresponding(lastPipe.Location)], currentPipe);
        } while (currentPipe != startingPipe);

        return new GridPolygon<Pipe>(loopingPipes);
    }

    private readonly Lazy<GridPolygon<Pipe>> loop;
}

public readonly record struct Pipe(GridPoint Location, Pipe.PipeType Type) : IGridSymbol, IGridPoint<int> {
    public Pipe(GridPoint location, char symbol) : this(location, ParseSymbol(symbol)) { }

    public char Symbol => ToBoxASCII(Type);
    public int X => this.Location.X;
    public int Y => this.Location.Y;
    public (GridPoint PointA, GridPoint PointB) Connections { get; } = Type switch {
        PipeType.NS => (Location + Direction.North, Location + Direction.South),
        PipeType.EW => (Location + Direction.East,  Location + Direction.West),
        PipeType.NE => (Location + Direction.North, Location + Direction.East),
        PipeType.NW => (Location + Direction.North, Location + Direction.West),
        PipeType.SW => (Location + Direction.South, Location + Direction.West),
        PipeType.SE => (Location + Direction.South, Location + Direction.East),
        _ => throw new ArgumentException("Invalid pipe type.", nameof(Type))
    };

    /// <summary>Checks to see if a given point is connected to this pipe.</summary>
    public bool IsConnected(GridPoint point) => point == Connections.PointA || point == Connections.PointB;

    /// <summary>Special method for creating the start pipe, requiring all the other pipes to be defined.</summary>
    public static Pipe CreateStartPipe(GridPoint location, IEnumerable<Pipe> pipes) {
        // parse all the pipes to find the ones that connect to this point.
        var connectedPoints = pipes.Where(pipe => pipe.IsConnected(location)).Select(pipe => pipe.Location).ToArray();
        if (connectedPoints.Length != 2) throw new ArgumentException("Start pipe must have exactly two connections");

        var pipeType = GetPipeType(location, connectedPoints[0], connectedPoints[1]);
        return new(location, pipeType);
    }

    /// <summary>Determines the pipe type by comparing its connections.</summary>
    public static PipeType GetPipeType(GridPoint centerPoint, GridPoint pointA, GridPoint pointB) {
        (int X, int Y) pointADifference = (centerPoint.X - pointA.X, centerPoint.Y - pointA.Y);
        (int X, int Y) pointBDifference = (centerPoint.X - pointB.X, centerPoint.Y - pointB.Y);
        return (pointADifference, pointBDifference) switch {
            ((+0, +1), (+0, -1)) => PipeType.NS,
            ((-1, +0), (+1, +0)) => PipeType.EW,
            ((+0, +1), (-1, +0)) => PipeType.NE,
            ((+0, +1), (+1, +0)) => PipeType.NW,
            ((+0, -1), (+1, +0)) => PipeType.SW,
            ((-1, +0), (+0, -1)) => PipeType.SE,
            _ => throw new InvalidOperationException("Invalid pipe connection.")
        };
    }

    public enum PipeType { NS, EW, NE, NW, SW, SE }

    public GridPoint GetCorresponding(GridPoint from) {
        if (from == Connections.PointA) return Connections.PointB;
        if (from == Connections.PointB) return Connections.PointA;
        throw new ArgumentException("No corresponding point.", nameof(from));
    }

    public static PipeType ParseSymbol(char symbol) => symbol switch {
        '|' => PipeType.NS,
        '-' => PipeType.EW,
        'L' => PipeType.NE,
        'J' => PipeType.NW,
        '7' => PipeType.SW,
        'F' => PipeType.SE,
        _ => throw new ArgumentException("Invalid pipe symbol.", nameof(symbol)),
    };

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
        Pipe startPipe = Pipe.CreateStartPipe(startPoint, pipes);
        pipes.Add(startPipe);

        int width = text.IndexOf('\n');
        return new PipeSchematic((width, y), pipes, startPipe);
    }
}