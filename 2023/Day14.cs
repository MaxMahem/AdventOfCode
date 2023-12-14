namespace AdventOfCode._2023;

using CommunityToolkit.HighPerformance;

using AdventOfCode.Helpers;

using IntPoint = Helpers.Point<int>;

public class Day14 : AdventBase
{
    SatDishMap? _dishRockMap;

    protected override void InternalOnLoad() {
        _dishRockMap = SatDishMapParser.Parse(Input.Text);
    }

    protected override object InternalPart1() => _dishRockMap!.TiltN().Weigh();

    protected override object InternalPart2() => _dishRockMap!.Cycle(CYCLES).Weigh();

    public const int CYCLES = 1_000_000_000;
}

public class SatDishMap {
    public IEnumerable<IRock> Rocks { get; }
    public Rectangle<int> Boundary { get; }

    public SatDishMap(IEnumerable<IRock> rocks, int width, int height) {
        ArgumentNullException.ThrowIfNull(rocks);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        Rocks    = rocks.ToImmutableArray();
        Boundary = new(width, height, 0, 0);
    }

    private SatDishMap(IReadOnlyDictionary<IntPoint, IRock> rockMap, Rectangle<int> boundary) {
        this.Rocks    = rockMap.Values;
        this.Boundary = boundary;
    }

    public int Weigh() => Rocks.OfType<RollingRock>().Select(rock => Boundary.Height - rock.Location.Y).Sum();

    /// <summary>Translates the current map into a BigInteger bitmask for memoization.</summary>
    public BigInteger GetValue() {
        int byteArraySize = Boundary.Width * Boundary.Height / BYTE_SIZE + 1;
        Span<byte> byteArray = stackalloc byte[byteArraySize];

        foreach (var coordinate in this.Rocks.Select(rock => rock.Location)) {
            int bitIndex = coordinate.X * Boundary.Width + coordinate.Y;

            int byteIndex   = bitIndex / BYTE_SIZE;
            int bitPosition = bitIndex % BYTE_SIZE;

            byteArray[byteIndex] |= (byte)(1 << bitPosition);
        }
        return new BigInteger(byteArray, true);
    }

    public override string ToString() {
        int bufferSize = Boundary.Height * (Boundary.Width + Environment.NewLine.Length);
        return string.Create(bufferSize, this, MapToBuffer);
    }

    public SatDishMap TiltN() => Tilt(North, Rocks.OrderBy(rock => rock.Location.Y));
    public SatDishMap TiltW() => Tilt(West,  Rocks.OrderBy(rock => rock.Location.X));
    public SatDishMap TiltS() => Tilt(South, Rocks.OrderByDescending(rock => rock.Location.Y));
    public SatDishMap TiltE() => Tilt(East,  Rocks.OrderByDescending(rock => rock.Location.X));


    public SatDishMap Tilt(IntPoint direction, IEnumerable<IRock> rockIteration) {
        Dictionary<IntPoint, IRock> newMap = [];

        foreach (var rock in rockIteration) {
            switch (rock) {
                case FixedRock fixedRock:
                    newMap[fixedRock.Location] = fixedRock;
                    break;
                case RollingRock rollingRock:
                    var newRock = Roll(rollingRock);
                    newMap[newRock.Location] = newRock;
                    break;
                default:
                    throw new InvalidOperationException("Invalid rock type");
            }
        }
        return new SatDishMap(newMap, Boundary);

        // moves the rock to the next valid location in the new map.
        RollingRock Roll(RollingRock rock) {
            (IntPoint nextLocation, IntPoint currentLocation) = (rock.Location + direction, rock.Location);
            while (!newMap.ContainsKey(nextLocation) && Boundary.ContainsPoint(nextLocation)) {
                (nextLocation, currentLocation) = (currentLocation + direction, nextLocation);
            }
            return new RollingRock(currentLocation);
        }
    }

    public SatDishMap Cycle(int cycles) {
        Dictionary<BigInteger, int> cache = [];
        SatDishMap cycledMap = this;

        for (int cycle = 0; cycle < cycles; cycle++) {
            cycledMap = cycledMap.TiltN().TiltW().TiltS().TiltE();
            BigInteger currentValue = cycledMap.GetValue();

            if (cache.TryGetValue(currentValue, out var cached)) {
                var remainingCycles = cycles - cycle - 1;
                var loopSize = cycle - cached;

                var loopRemaining = remainingCycles % loopSize;
                cycle = cycles - loopRemaining - 1;
            }
            cache[currentValue] = cycle;
        }
        return cycledMap;
    }

    private static IntPoint North = new(0, -1);
    private static IntPoint South = new(0, +1);
    private static IntPoint East  = new(+1, 0);
    private static IntPoint West  = new(-1, 0);

    private static void MapToBuffer(Span<char> buffer, SatDishMap state) {
        var rockDict = state.Rocks.ToImmutableDictionary(rock => rock.Location);
        for (int y = 0, offset = 0; y < state.Boundary.Height; y++) {
            for (int x = 0; x < state.Boundary.Width; x++) {
                rockDict.TryGetValue(new(x, y), out IRock? rock);
                buffer[offset..(offset + state.Boundary.Width)][x] = rock switch {
                    IRock => rock.Symbol,
                    null  => '.',
                };
            }
            offset += state.Boundary.Width;

            Environment.NewLine.AsSpan().CopyTo(buffer[offset..]);
            offset += Environment.NewLine.Length;
        }
    }

    const int BYTE_SIZE = sizeof(byte) * 8;
}

public interface IRock {
    public IntPoint Location { get; }
    public char Symbol { get; }
}
public record struct RollingRock(IntPoint Location) : IRock {
    public readonly char Symbol { get; } = 'O';
}
public record struct FixedRock(IntPoint Location) : IRock {
    public readonly char Symbol { get; } = '#';
}

public static class SatDishMapParser
{
    public static SatDishMap Parse(string input) {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var text    = input.AsSpan();
        int lineEnd = input.IndexOf('\n');
        int width   = text[..lineEnd].LastIndexOfAnyExcept('\r', '\n') + 1;

        List<IRock> rocks = [];

        int y = 0;
        foreach (var line in text.EnumerateLines()) {
            int index = 0;

            while (index < line.Length) {
                // advance to next non dot character.
                int hitIndex = line[index..].IndexOfAnyExcept('.');
                if (hitIndex == -1) break; // EoL.

                index += hitIndex;
                char symbol = line[index];

                IntPoint location = new(index, y);
                rocks.Add(symbol switch {
                    'O' => new RollingRock(location),
                    '#' => new FixedRock(location),
                    _   => throw new ArgumentOutOfRangeException(nameof(input), symbol, "Unhandled character in input.")
                });

                index += 1;
            }
            if (line.Length > 0) y++;
        }

        return new SatDishMap(rocks, width, y);
    }

    public readonly static IReadOnlySet<char> ValidCharacters = "#O".ToImmutableHashSet();
}